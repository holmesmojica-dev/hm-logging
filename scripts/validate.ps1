[CmdletBinding()]
param(
    [Parameter()]
    [switch]$CollectCoverage,

    [Parameter()]
    [string]$ReleaseVersion,

    [Parameter()]
    [string]$SourceCommit
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Invoke-ValidationCommand {
    param(
        [Parameter(Mandatory)]
        [string]$Command,

        [Parameter()]
        [string[]]$Arguments = @()
    )

    if (-not (Get-Command $Command -ErrorAction SilentlyContinue)) {
        throw "Required command '$Command' was not found on PATH."
    }

    Write-Output "==> $Command $($Arguments -join ' ')"
    & $Command @Arguments

    if ($LASTEXITCODE -ne 0) {
        throw "Validation command '$Command' failed with exit code $LASTEXITCODE."
    }
}

$repositoryRoot = (& git rev-parse --show-toplevel).Trim()
if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($repositoryRoot)) {
    throw 'The validation script must run from within a Git repository.'
}

Push-Location $repositoryRoot
try {
    if ([string]::IsNullOrWhiteSpace($ReleaseVersion) -xor [string]::IsNullOrWhiteSpace($SourceCommit)) {
        throw 'ReleaseVersion and SourceCommit must be supplied together for release validation.'
    }

    $buildArguments = @('build', 'Hm.Logging.sln', '--configuration', 'Release', '--no-restore')
    if (-not [string]::IsNullOrWhiteSpace($ReleaseVersion)) {
        $buildArguments += @(
            "-p:ReleaseVersion=$ReleaseVersion",
            "-p:PackageVersion=$ReleaseVersion",
            "-p:RepositoryCommit=$SourceCommit",
            "-p:SourceRevisionId=$SourceCommit",
            '-p:ContinuousIntegrationBuild=true'
        )
    }

    $testArguments = @('test', 'Hm.Logging.sln', '--configuration', 'Release', '--no-build', '--no-restore')
    if ($CollectCoverage) {
        $testArguments += @(
            '/p:CollectCoverage=true',
            '/p:CoverletOutputFormat=opencover',
            '/p:CoverletOutput=TestResults/'
        )
    }

    Invoke-ValidationCommand -Command dotnet -Arguments @('restore', 'Hm.Logging.sln')
    Invoke-ValidationCommand -Command dotnet -Arguments @('format', 'Hm.Logging.sln', '--no-restore', '--verify-no-changes')
    Invoke-ValidationCommand -Command dotnet -Arguments $buildArguments
    Invoke-ValidationCommand -Command dotnet -Arguments $testArguments
    Invoke-ValidationCommand -Command pwsh -Arguments @('-NoProfile', '-File', 'tests/GitHubRelease.Tests.ps1')
    Invoke-ValidationCommand -Command pwsh -Arguments @('-NoProfile', '-File', 'tests/ReleaseWorkflow.Tests.ps1')
}
finally {
    Pop-Location
}
