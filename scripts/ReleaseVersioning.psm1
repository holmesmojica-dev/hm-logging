Set-StrictMode -Version Latest

function Resolve-HmLoggingReleaseContext {
    param(
        [Parameter(Mandatory)][string]$RepositoryPath,
        [Parameter(Mandatory)][string]$Tag,
        [Parameter(Mandatory)][string]$MainBranch
    )

    if ($Tag -notmatch '^v([0-9]+\.[0-9]+\.[0-9]+(?:-preview\.[0-9]+)?)$') {
        throw "Release tag '$Tag' must use the supported v<major.minor.patch[-preview.number]> form."
    }

    $releaseVersion = $Matches[1]

    $commit = (& git -C $RepositoryPath rev-parse --verify "$Tag^{commit}").Trim()
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($commit)) {
        throw "Release tag '$Tag' does not resolve to a commit."
    }

    & git -C $RepositoryPath rev-parse --verify "$MainBranch^{commit}" *> $null
    if ($LASTEXITCODE -ne 0) {
        throw "Main branch reference '$MainBranch' does not resolve to a commit."
    }

    & git -C $RepositoryPath merge-base --is-ancestor $commit $MainBranch
    if ($LASTEXITCODE -ne 0) {
        throw "Tagged commit '$commit' is not in the history of '$MainBranch'."
    }

    [pscustomobject]@{
        Tag = $Tag
        ReleaseVersion = $releaseVersion
        SourceCommit = $commit
    }
}

function Assert-HmLoggingCheckedOutCommit {
    param(
        [Parameter(Mandatory)][string]$RepositoryPath,
        [Parameter(Mandatory)][string]$SourceCommit
    )

    $checkedOutCommit = (& git -C $RepositoryPath rev-parse HEAD).Trim()
    if ($LASTEXITCODE -ne 0 -or $checkedOutCommit -ne $SourceCommit) {
        throw "Checked-out commit '$checkedOutCommit' does not match release source commit '$SourceCommit'."
    }
}

Export-ModuleMember -Function Resolve-HmLoggingReleaseContext, Assert-HmLoggingCheckedOutCommit
