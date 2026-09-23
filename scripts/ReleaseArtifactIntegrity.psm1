Set-StrictMode -Version Latest

function Get-HmLoggingReleaseArtifactNames {
    param([Parameter(Mandatory)][string]$ReleaseVersion)

    @(
        "HDev.Hm.Logging.Core.$ReleaseVersion.nupkg"
        "HDev.Hm.Logging.Core.$ReleaseVersion.snupkg"
    )
}

function New-HmLoggingReleaseArtifactManifest {
    param([Parameter(Mandatory)][string]$PackageDirectory, [Parameter(Mandatory)][string]$ReleaseVersion)

    $directory = (Resolve-Path -LiteralPath $PackageDirectory).Path
    $entries = foreach ($name in Get-HmLoggingReleaseArtifactNames -ReleaseVersion $ReleaseVersion) {
        $path = Join-Path $directory $name
        if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Release artifact '$name' was not produced." }
        "$((Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant()) *$name"
    }

    [System.IO.File]::WriteAllLines((Join-Path $directory 'release-artifacts.sha256'), $entries, [System.Text.UTF8Encoding]::new($false))
}

function Assert-HmLoggingReleaseArtifactManifest {
    param([Parameter(Mandatory)][string]$PackageDirectory, [Parameter(Mandatory)][string]$ReleaseVersion)

    $directory = (Resolve-Path -LiteralPath $PackageDirectory).Path
    $manifest = Join-Path $directory 'release-artifacts.sha256'
    if (-not (Test-Path -LiteralPath $manifest -PathType Leaf)) { throw 'The release artifact integrity manifest was not produced.' }

    $lines = @(Get-Content -LiteralPath $manifest)
    $names = @(Get-HmLoggingReleaseArtifactNames -ReleaseVersion $ReleaseVersion)
    if ($lines.Count -ne $names.Count) { throw 'The release artifact integrity manifest has an unexpected number of entries.' }

    foreach ($name in $names) {
        $line = @($lines | Where-Object { $_ -match "^[0-9a-f]{64} \*$([regex]::Escape($name))$" })
        if ($line.Count -ne 1) { throw "The release artifact integrity manifest is missing '$name'." }
        $actualHash = (Get-FileHash -LiteralPath (Join-Path $directory $name) -Algorithm SHA256).Hash.ToLowerInvariant()
        if ($line[0].Substring(0, 64) -cne $actualHash) { throw "Release artifact '$name' does not match the integrity manifest." }
    }
}

Export-ModuleMember -Function Get-HmLoggingReleaseArtifactNames, New-HmLoggingReleaseArtifactManifest, Assert-HmLoggingReleaseArtifactManifest
