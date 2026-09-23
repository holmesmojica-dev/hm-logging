[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot '..\scripts\GitHubRelease.psm1') -Force
Import-Module (Join-Path $PSScriptRoot '..\scripts\ReleaseVersioning.psm1') -Force

function Assert-Equal {
    param($Expected, $Actual)
    if ($Expected -ne $Actual) { throw "Expected '$Expected', received '$Actual'." }
}

function Assert-Throws {
    param([scriptblock]$Action)
    try { & $Action } catch { return }
    throw 'Expected an exception.'
}

$tag = 'v0.1.7-preview.999'
$release = Resolve-HmLoggingReleaseTag -Tag $tag
Assert-Equal '0.1.7-preview.999' $release.ReleaseVersion
Assert-Throws { Resolve-HmLoggingReleaseTag -Tag 'v0.1.7-alpha.1' }

Assert-HmLoggingGitHubRepositoryAccess -Repository 'owner/repository' -ExitCode 0 -Output @('HTTP/2.0 200 OK')
Assert-Throws { Assert-HmLoggingGitHubRepositoryAccess -Repository 'owner/repository' -ExitCode 1 -Output @('HTTP/2.0 401 Unauthorized') }
Assert-Throws { Assert-HmLoggingGitHubRepositoryAccess -Repository 'owner/repository' -ExitCode 1 -Output @('HTTP/2.0 404 Not Found') }

$existing = Resolve-HmLoggingGitHubReleaseLookup -Tag $tag -ExpectedPrerelease $true -ExitCode 0 -Output @('HTTP/2.0 200 OK' + [Environment]::NewLine + [Environment]::NewLine + '{"tag_name":"v0.1.7-preview.999","prerelease":true}')
Assert-Equal 'existing' $existing.State

$absent = Resolve-HmLoggingGitHubReleaseLookup -Tag $tag -ExpectedPrerelease $true -ExitCode 1 -Output @('HTTP/2.0 404 Not Found' + [Environment]::NewLine + [Environment]::NewLine + '{"message":"Not Found"}')
Assert-Equal 'absent' $absent.State

Assert-Throws { Resolve-HmLoggingGitHubReleaseLookup -Tag $tag -ExpectedPrerelease $true -ExitCode 1 -Output @('HTTP/2.0 401 Unauthorized' + [Environment]::NewLine + [Environment]::NewLine + '{"message":"Bad credentials"}') }
Assert-Throws { Resolve-HmLoggingGitHubReleaseLookup -Tag $tag -ExpectedPrerelease $true -ExitCode 0 -Output @('HTTP/2.0 404 Not Found') }
Assert-Throws { Resolve-HmLoggingGitHubReleaseLookup -Tag $tag -ExpectedPrerelease $true -ExitCode 0 -Output @('HTTP/2.0 200 OK' + [Environment]::NewLine + [Environment]::NewLine + '{"tag_name":"v0.1.7","prerelease":true}') }
Assert-Throws { Resolve-HmLoggingGitHubReleaseLookup -Tag $tag -ExpectedPrerelease $true -ExitCode 0 -Output @('HTTP/2.0 200 OK' + [Environment]::NewLine + [Environment]::NewLine + '{"tag_name":"v0.1.7-preview.999","prerelease":false}') }
Assert-Throws { Resolve-HmLoggingGitHubReleaseLookup -Tag $tag -ExpectedPrerelease $true -ExitCode 0 -Output @('HTTP/2.0 200 OK' + [Environment]::NewLine + [Environment]::NewLine + '{invalid json}') }

$prereleaseArguments = Get-HmLoggingGitHubReleaseCreateArguments -Tag $tag -Repository 'owner/repository' -Prerelease $true
Assert-Equal $true ($prereleaseArguments -contains '--prerelease')
Assert-Equal $true ($prereleaseArguments -contains '--generate-notes')
Assert-Equal $true ($prereleaseArguments -contains '--verify-tag')

$stableArguments = Get-HmLoggingGitHubReleaseCreateArguments -Tag 'v0.1.7' -Repository 'owner/repository' -Prerelease $false
Assert-Equal $false ($stableArguments -contains '--prerelease')

Write-Output 'GitHub Release tests passed.'
