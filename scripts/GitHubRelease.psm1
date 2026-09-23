Set-StrictMode -Version Latest

function Get-HmLoggingGitHubHttpStatus {
    param([Parameter(Mandatory)][string[]]$Output)

    $text = $Output -join [Environment]::NewLine
    $statusMatches = [regex]::Matches($text, '(?m)^HTTP/\S+\s+(\d{3})(?:\s|$)')
    if ($statusMatches.Count -eq 0) { throw 'GitHub API lookup did not return an HTTP status line.' }

    return [int]$statusMatches[$statusMatches.Count - 1].Groups[1].Value
}

function Assert-HmLoggingGitHubRepositoryAccess {
    param(
        [Parameter(Mandatory)][string]$Repository,
        [Parameter(Mandatory)][int]$ExitCode,
        [Parameter(Mandatory)][string[]]$Output
    )

    $status = Get-HmLoggingGitHubHttpStatus -Output $Output
    if ($ExitCode -ne 0 -or $status -ne 200) {
        throw "GitHub repository access check for '$Repository' failed with HTTP status $status."
    }
}

function Resolve-HmLoggingGitHubReleaseLookup {
    param(
        [Parameter(Mandatory)][string]$Tag,
        [Parameter(Mandatory)][bool]$ExpectedPrerelease,
        [Parameter(Mandatory)][int]$ExitCode,
        [Parameter(Mandatory)][string[]]$Output
    )

    $text = ($Output -join [Environment]::NewLine).Trim()
    $status = Get-HmLoggingGitHubHttpStatus -Output $Output
    if ($status -eq 404) {
        if ($ExitCode -eq 0) { throw "GitHub Release lookup for '$Tag' returned HTTP 404 with a successful exit code." }
        return [pscustomobject]@{ State = 'absent' }
    }

    if ($ExitCode -ne 0 -or $status -ne 200) { throw "GitHub Release lookup for '$Tag' failed with HTTP status $status." }

    $jsonStart = $text.IndexOf('{', [System.StringComparison]::Ordinal)
    if ($jsonStart -lt 0) { throw "GitHub Release lookup for '$Tag' did not return a JSON response." }
    try { $release = $text.Substring($jsonStart) | ConvertFrom-Json } catch { throw "GitHub Release lookup for '$Tag' returned malformed JSON." }
    if ($release.tag_name -cne $Tag) { throw "GitHub Release lookup returned an unexpected tag identity for '$Tag'." }
    if ([bool]$release.prerelease -ne $ExpectedPrerelease) { throw "GitHub Release '$Tag' has an unexpected prerelease state." }

    return [pscustomobject]@{ State = 'existing' }
}

function Get-HmLoggingGitHubReleaseCreateArguments {
    param(
        [Parameter(Mandatory)][string]$Tag,
        [Parameter(Mandatory)][string]$Repository,
        [Parameter(Mandatory)][bool]$Prerelease
    )

    $arguments = @('release', 'create', $Tag, '--repo', $Repository, '--title', $Tag, '--generate-notes', '--verify-tag')
    if ($Prerelease) { $arguments += '--prerelease' }
    return $arguments
}

Export-ModuleMember -Function Assert-HmLoggingGitHubRepositoryAccess, Get-HmLoggingGitHubReleaseCreateArguments, Resolve-HmLoggingGitHubReleaseLookup
