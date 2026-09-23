[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$Tag,
    [Parameter()][string]$MainBranch = 'main',
    [Parameter()][string]$GitHubOutputPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Import-Module (Join-Path $PSScriptRoot 'ReleaseVersioning.psm1') -Force
$repositoryRoot = (& git rev-parse --show-toplevel).Trim()
$release = Resolve-HmLoggingReleaseContext -RepositoryPath $repositoryRoot -Tag $Tag -MainBranch $MainBranch

if (-not [string]::IsNullOrWhiteSpace($GitHubOutputPath)) {
    @(
        "release_version=$($release.ReleaseVersion)"
        "release_tag=$($release.Tag)"
        "source_commit=$($release.SourceCommit)"
    ) | Add-Content -LiteralPath $GitHubOutputPath
}

$release | ConvertTo-Json -Compress
