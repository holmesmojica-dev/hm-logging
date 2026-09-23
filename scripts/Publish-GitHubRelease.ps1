[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$Tag,
    [Parameter(Mandatory)][string]$ReleaseVersion,
    [Parameter(Mandatory)][string]$Repository
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Import-Module (Join-Path $PSScriptRoot 'ReleaseVersioning.psm1') -Force
Import-Module (Join-Path $PSScriptRoot 'GitHubRelease.psm1') -Force

if (-not (Get-Command gh -ErrorAction SilentlyContinue)) { throw "Required command 'gh' was not found on PATH." }
$release = Resolve-HmLoggingReleaseTag -Tag $Tag
if ($release.ReleaseVersion -cne $ReleaseVersion) { throw "Release tag '$Tag' does not match release version '$ReleaseVersion'." }
$isPrerelease = $release.ReleaseVersion.Contains('-', [System.StringComparison]::Ordinal)

$repositoryLookupOutput = @(& gh api --include --silent --method GET "repos/$Repository" 2>&1 | ForEach-Object { $_.ToString() })
Assert-HmLoggingGitHubRepositoryAccess -Repository $Repository -ExitCode $LASTEXITCODE -Output $repositoryLookupOutput

$lookupOutput = @(& gh api --include --method GET "repos/$Repository/releases/tags/$Tag" 2>&1 | ForEach-Object { $_.ToString() })
$lookup = Resolve-HmLoggingGitHubReleaseLookup -Tag $Tag -ExpectedPrerelease $isPrerelease -ExitCode $LASTEXITCODE -Output $lookupOutput
if ($lookup.State -eq 'existing') {
    Write-Output "GitHub Release '$Tag' is already verified."
    return
}

& gh @(Get-HmLoggingGitHubReleaseCreateArguments -Tag $Tag -Repository $Repository -Prerelease $isPrerelease)
if ($LASTEXITCODE -ne 0) { throw "GitHub Release creation failed for '$Tag'." }
Write-Output "GitHub Release '$Tag' was created."
