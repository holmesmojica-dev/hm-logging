[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$ReleaseVersion,
    [Parameter(Mandatory)][string]$SourceCommit,
    [Parameter(Mandatory)][string]$OutputDirectory
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Import-Module (Join-Path $PSScriptRoot 'ReleaseVersioning.psm1') -Force
$repositoryRoot = (& git rev-parse --show-toplevel).Trim()
Assert-HmLoggingCheckedOutCommit -RepositoryPath $repositoryRoot -SourceCommit $SourceCommit

$outputPath = Join-Path $repositoryRoot $OutputDirectory
if (Test-Path -LiteralPath $outputPath) { throw "Release artifact output directory '$outputPath' must not already exist." }
New-Item -ItemType Directory -Path $outputPath | Out-Null

& (Join-Path $PSScriptRoot 'validate.ps1') -ReleaseVersion $ReleaseVersion -SourceCommit $SourceCommit
& dotnet pack src/Hm.Logging/Hm.Logging.csproj --configuration Release --no-build --no-restore --output $outputPath "-p:ReleaseVersion=$ReleaseVersion" "-p:PackageVersion=$ReleaseVersion" "-p:RepositoryCommit=$SourceCommit" "-p:SourceRevisionId=$SourceCommit" '-p:ContinuousIntegrationBuild=true'
if ($LASTEXITCODE -ne 0) { throw 'Release artifact packaging failed.' }

& (Join-Path $PSScriptRoot 'Validate-ReleaseArtifact.ps1') -PackageDirectory $outputPath -ReleaseVersion $ReleaseVersion -SourceCommit $SourceCommit
& (Join-Path $PSScriptRoot 'New-ReleaseArtifactManifest.ps1') -PackageDirectory $outputPath -ReleaseVersion $ReleaseVersion
