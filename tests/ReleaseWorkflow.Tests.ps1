[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Assert-True {
    param([Parameter(Mandatory)][bool]$Condition, [Parameter(Mandatory)][string]$Message)
    if (-not $Condition) { throw $Message }
}

$repositoryRoot = (& git rev-parse --show-toplevel).Trim()
$workflow = Get-Content -LiteralPath (Join-Path $repositoryRoot '.github/workflows/publish-nuget.yml') -Raw
$releaseScript = Get-Content -LiteralPath (Join-Path $repositoryRoot 'scripts/Publish-GitHubRelease.ps1') -Raw
$releaseModule = Get-Content -LiteralPath (Join-Path $repositoryRoot 'scripts/GitHubRelease.psm1') -Raw

Assert-True ($workflow -match '(?ms)^permissions:\s+contents: read') 'The release workflow must remain read-only by default.'
Assert-True ($workflow -match '(?ms)^  post-publication:.*?needs: \[preflight, build-artifact, attest-artifacts, publish-nuget\].*?permissions:\s+contents: write') 'GitHub Release creation must follow successful NuGet publication with contents-write scoped to its job.'
Assert-True ($workflow -match '(?ms)^  post-publication:.*?uses: actions/checkout@.*?ref: \$\{\{ needs\.preflight\.outputs\.source_commit \}\}.*?Publish-GitHubRelease\.ps1.*?-Tag.*?needs\.preflight\.outputs\.release_tag.*?-ReleaseVersion.*?needs\.preflight\.outputs\.release_version') 'GitHub Release creation must use the immutable release source and preflight identity.'
Assert-True ($releaseScript -match 'Resolve-HmLoggingReleaseTag') 'GitHub Release creation must validate its tag identity.'
Assert-True ($releaseScript -match 'gh api --include --silent --method GET "repos/\$Repository"') 'GitHub Release creation must verify repository access before treating a release lookup as absent.'
Assert-True ($releaseScript -match 'gh api --include --method GET "repos/\$Repository/releases/tags/\$Tag"') 'GitHub Release creation must query the documented release-by-tag API endpoint.'
Assert-True ($releaseScript -match '\$lookup\.State -eq ''existing''') 'An existing GitHub Release must satisfy a retry without recreation.'
Assert-True ($releaseScript -notmatch 'gh release edit|gh release delete') 'GitHub Release retries must not modify or delete an existing release.'
Assert-True ($releaseModule -match '--generate-notes') 'GitHub Release creation must use generated release notes.'
Assert-True ($releaseModule -match '--verify-tag') 'GitHub Release creation must not create or move a release tag.'
Assert-True ($releaseModule -match '\$status -eq 404') 'Only an HTTP 404 from the release-by-tag endpoint may represent an absent release.'
Assert-True ($releaseModule -notmatch '\.nupkg|\.snupkg') 'GitHub Release creation must not attach package artifacts.'

Write-Output 'Release workflow tests passed.'
