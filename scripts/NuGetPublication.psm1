Set-StrictMode -Version Latest

function Get-HmLoggingNuGetPackageUri {
    param([Parameter(Mandatory)][string]$ReleaseVersion)

    $packageName = "hdev.hm.logging.core.$($ReleaseVersion.ToLowerInvariant()).nupkg"
    return "https://api.nuget.org/v3-flatcontainer/hdev.hm.logging.core/$($ReleaseVersion.ToLowerInvariant())/$packageName"
}

function Assert-HmLoggingNuGetContentIdentity {
    param(
        [Parameter(Mandatory)][string]$LocalPackagePath,
        [Parameter(Mandatory)][string]$RemotePackagePath
    )

    # Builds only private Delivery tooling; it never rebuilds the Core package.
    $tool = Join-Path (Split-Path $PSScriptRoot -Parent) 'tools/Hm.Logging.ReleaseTools/Hm.Logging.ReleaseTools.csproj'
    & dotnet run --project $tool --configuration Release --no-launch-profile -- assert-content-identity $LocalPackagePath $RemotePackagePath
    if ($LASTEXITCODE -ne 0) { throw 'NuGet content identity verification failed.' }
}

function Test-HmLoggingNuGetNotFoundStatusCode {
    param([object]$StatusCode)

    return $null -ne $StatusCode -and [int]$StatusCode -eq 404
}

Export-ModuleMember -Function Assert-HmLoggingNuGetContentIdentity, Get-HmLoggingNuGetPackageUri, Test-HmLoggingNuGetNotFoundStatusCode
