[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$PackageDirectory,
    [Parameter(Mandatory)][string]$ReleaseVersion,
    [Parameter(Mandatory)][string]$SourceCommit
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

& (Join-Path $PSScriptRoot 'Validate-ReleaseArtifact.ps1') -PackageDirectory $PackageDirectory -ReleaseVersion $ReleaseVersion -SourceCommit $SourceCommit
& (Join-Path $PSScriptRoot 'Assert-ReleaseArtifactManifest.ps1') -PackageDirectory $PackageDirectory -ReleaseVersion $ReleaseVersion
if ([string]::IsNullOrWhiteSpace($env:NUGET_TRUSTED_PUBLISHING_API_KEY)) { throw 'NUGET_TRUSTED_PUBLISHING_API_KEY must be supplied for NuGet publication.' }

$packageName = "HDev.Hm.Logging.Core.$ReleaseVersion.nupkg"
$packagePath = Join-Path (Resolve-Path -LiteralPath $PackageDirectory).Path $packageName
& dotnet nuget push $packagePath --api-key $env:NUGET_TRUSTED_PUBLISHING_API_KEY --source 'https://api.nuget.org/v3/index.json'
if ($LASTEXITCODE -ne 0) { throw "NuGet publication failed for '$packageName'." }
