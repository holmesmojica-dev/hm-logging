[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$PackageDirectory,
    [Parameter(Mandatory)][string]$ReleaseVersion,
    [Parameter(Mandatory)][string]$SourceCommit
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'NuGetPublication.psm1') -Force

& (Join-Path $PSScriptRoot 'Validate-ReleaseArtifact.ps1') -PackageDirectory $PackageDirectory -ReleaseVersion $ReleaseVersion -SourceCommit $SourceCommit
& (Join-Path $PSScriptRoot 'Assert-ReleaseArtifactManifest.ps1') -PackageDirectory $PackageDirectory -ReleaseVersion $ReleaseVersion
if ([string]::IsNullOrWhiteSpace($env:NUGET_TRUSTED_PUBLISHING_API_KEY)) { throw 'NUGET_TRUSTED_PUBLISHING_API_KEY must be supplied for NuGet publication.' }

$packageName = "HDev.Hm.Logging.Core.$ReleaseVersion.nupkg"
$packagePath = Join-Path (Resolve-Path -LiteralPath $PackageDirectory).Path $packageName
& dotnet nuget push $packagePath --api-key $env:NUGET_TRUSTED_PUBLISHING_API_KEY --source 'https://api.nuget.org/v3/index.json'
if ($LASTEXITCODE -ne 0) { throw "NuGet publication failed for '$packageName'." }

$temporaryDirectory = Join-Path ([IO.Path]::GetTempPath()) "hm-core-nuget-$([Guid]::NewGuid())"
$remotePackagePath = Join-Path $temporaryDirectory $packageName
try {
    New-Item -ItemType Directory -Path $temporaryDirectory | Out-Null
    for ($attempt = 1; $attempt -le 60; $attempt++) {
        $available = $false
        try {
            Invoke-WebRequest -Uri (Get-HmLoggingNuGetPackageUri -ReleaseVersion $ReleaseVersion) -OutFile $remotePackagePath -TimeoutSec 30
            $available = $true
        }
        catch {
            $statusCode = if ($null -ne $_.Exception.Response) { $_.Exception.Response.StatusCode } else { $null }
            if (-not (Test-HmLoggingNuGetNotFoundStatusCode -StatusCode $statusCode)) { throw }
            if ($attempt -eq 60) { throw 'NuGet did not make the published package available for identity verification within 15 minutes.' }
            Start-Sleep -Seconds 15
        }
        if ($available) {
            & (Join-Path $PSScriptRoot 'Validate-ReleaseArtifact.ps1') -PackageDirectory $temporaryDirectory -ReleaseVersion $ReleaseVersion -SourceCommit $SourceCommit -SkipSymbolPackage
            Assert-HmLoggingNuGetContentIdentity -LocalPackagePath $packagePath -RemotePackagePath $remotePackagePath
            return
        }
    }
}
finally {
    if (Test-Path -LiteralPath $temporaryDirectory) { Remove-Item -LiteralPath $temporaryDirectory -Recurse -Force }
}
