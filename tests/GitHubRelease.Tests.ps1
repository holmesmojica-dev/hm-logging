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

# Real native stdout is captured as separate lines, including the HTTP separator.
Assert-HmLoggingGitHubRepositoryAccess -Repository 'owner/repository' -ExitCode 0 -Output @('HTTP/2.0 200 OK', 'Content-Type: application/json', '')
Assert-Throws { Assert-HmLoggingGitHubRepositoryAccess -Repository 'owner/repository' -ExitCode 0 -Output @() }
Assert-Throws { Assert-HmLoggingGitHubRepositoryAccess -Repository 'owner/repository' -ExitCode 0 -Output @('') }
Assert-Throws { Resolve-HmLoggingGitHubReleaseLookup -Tag $tag -ExpectedPrerelease $true -ExitCode 2 -Output @('HTTP/2.0 404 Not Found', '') }

$testDirectory = Join-Path ([IO.Path]::GetTempPath()) ('hm-github-release-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $testDirectory | Out-Null
try {
    $fakePath = Join-Path $testDirectory 'fake-gh.ps1'
    $runnerPath = Join-Path $testDirectory 'runner.ps1'
    $callsPath = Join-Path $testDirectory 'calls.txt'
    @'
param([string]$Scenario, [string]$CallsPath, [string]$ArgumentsBase64)
$CommandArguments = [Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($ArgumentsBase64)) | ConvertFrom-Json
[IO.File]::AppendAllText($CallsPath, ($CommandArguments -join ' ') + "`n")
if ($CommandArguments[0] -eq 'release') {
    if ($Scenario -eq 'create-failure') { [Console]::Error.WriteLine('creation failed'); exit 1 }
    exit 0
}
$repositoryLookup = $CommandArguments[-1] -eq 'repos/owner/repository'
if ($repositoryLookup) {
    switch ($Scenario) {
        'access-empty' { exit 0 }
        'access-stderr' { [Console]::Error.WriteLine('network failure'); exit 1 }
        'access-401' { [Console]::WriteLine('HTTP/2.0 401 Unauthorized'); exit 1 }
        'access-403' { [Console]::WriteLine('HTTP/2.0 403 Forbidden'); exit 1 }
        'access-500' { [Console]::WriteLine('HTTP/2.0 500 Server Error'); exit 1 }
        'access-bad-exit' { [Console]::WriteLine('HTTP/2.0 200 OK'); exit 1 }
        'single-line' { [Console]::WriteLine('HTTP/2.0 200 OK'); exit 0 }
    }
    [Console]::WriteLine('HTTP/2.0 200 OK')
    [Console]::WriteLine('Content-Type: application/json')
    [Console]::WriteLine('')
    exit 0
}
switch ($Scenario) {
    'lookup-empty' { exit 0 }
    'lookup-network' { [Console]::Error.WriteLine('network failure'); exit 1 }
    'lookup-401' { [Console]::WriteLine('HTTP/2.0 401 Unauthorized'); exit 1 }
    'lookup-403' { [Console]::WriteLine('HTTP/2.0 403 Forbidden'); exit 1 }
    'lookup-500' { [Console]::WriteLine('HTTP/2.0 500 Server Error'); exit 1 }
    'lookup-bad-exit' { [Console]::WriteLine('HTTP/2.0 200 OK'); exit 1 }
    '404-success-exit' { [Console]::WriteLine('HTTP/2.0 404 Not Found'); exit 0 }
    '404-unexpected-exit' { [Console]::WriteLine('HTTP/2.0 404 Not Found'); exit 2 }
}
if ($Scenario -in @('absent', 'single-line', 'create-failure')) {
    [Console]::WriteLine('HTTP/2.0 404 Not Found')
    [Console]::WriteLine('')
    [Console]::WriteLine('{"message":"Not Found"}')
    [Console]::Error.WriteLine('arbitrary diagnostic, not used to classify absence')
    exit 1
}
[Console]::WriteLine('HTTP/2.0 200 OK')
[Console]::WriteLine('Content-Type: application/json')
[Console]::WriteLine('')
switch ($Scenario) {
    'invalid-json' { [Console]::WriteLine('{invalid json}'); exit 0 }
    'wrong-tag' { [Console]::WriteLine('{"tag_name":"v0.1.8","prerelease":true}'); exit 0 }
    'wrong-prerelease' { [Console]::WriteLine('{"tag_name":"v0.1.7-preview.999","prerelease":false}'); exit 0 }
}
[Console]::WriteLine('{')
[Console]::WriteLine('"tag_name":"v0.1.7-preview.999",')
[Console]::WriteLine('"prerelease":true}')
exit 0
'@ | Set-Content -LiteralPath $fakePath
    @'
param($FakePath, $CallsPath, $Scenario, $ProductionScript)
$ErrorActionPreference = 'Stop'
# A function wins command resolution; it invokes only our local native pwsh fake.
# No PATH shadowing or real gh executable is involved.
function gh {
    $argumentsBase64 = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes((ConvertTo-Json -InputObject @($args) -Compress)))
    & pwsh -NoProfile -File $FakePath -Scenario $Scenario -CallsPath $CallsPath -ArgumentsBase64 $argumentsBase64
    $global:LASTEXITCODE = $LASTEXITCODE
}
& $ProductionScript -Tag 'v0.1.7-preview.999' -ReleaseVersion '0.1.7-preview.999' -Repository 'owner/repository'
'@ | Set-Content -LiteralPath $runnerPath
    $productionScript = Join-Path $PSScriptRoot '../scripts/Publish-GitHubRelease.ps1'
    $scenarios = @('absent', 'single-line', 'existing', 'access-empty', 'access-stderr', 'access-401', 'access-403', 'access-500', 'access-bad-exit', 'lookup-empty', 'lookup-network', 'lookup-401', 'lookup-403', 'lookup-500', 'lookup-bad-exit', '404-success-exit', '404-unexpected-exit', 'invalid-json', 'wrong-tag', 'wrong-prerelease', 'create-failure')
    foreach ($scenario in $scenarios) {
        [IO.File]::WriteAllText($callsPath, '')
        $output = @(& pwsh -NoProfile -File $runnerPath -FakePath $fakePath -CallsPath $callsPath -Scenario $scenario -ProductionScript $productionScript 2>&1)
        $exitCode = $LASTEXITCODE
        $calls = @(Get-Content -LiteralPath $callsPath)
        $expectedSuccess = $scenario -in @('absent', 'single-line', 'existing')
        if (($exitCode -eq 0) -ne $expectedSuccess) { throw "Scenario '$scenario' failed: $($output -join [Environment]::NewLine)" }
        $expectedCreates = if ($scenario -in @('absent', 'single-line', 'create-failure')) { 1 } else { 0 }
        Assert-Equal $expectedCreates @($calls | Where-Object { $_ -like 'release create *' }).Count
        if ($scenario -like 'access-*') { Assert-Equal 1 $calls.Count }
        if (($output -join '') -match 'Cannot bind argument') { throw "Scenario '$scenario' failed during parameter binding." }
    }
}
finally {
    Remove-Item -LiteralPath $testDirectory -Recurse -Force
}
Write-Output 'GitHub Release tests passed (including 21 isolated behavioral scenarios).'
