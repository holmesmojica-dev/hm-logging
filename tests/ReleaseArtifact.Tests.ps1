[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression

$repositoryRoot = Split-Path $PSScriptRoot -Parent
$pdbPath = Join-Path $repositoryRoot 'src/Hm.Logging/bin/Release/net10.0/Hm.Logging.pdb'
if (-not (Test-Path -LiteralPath $pdbPath)) { throw 'Build the Release configuration before running artifact tests.' }
$version = '0.1.7-preview.999'
$commit = '1234567890123456789012345678901234567890'
$testDirectory = Join-Path ([IO.Path]::GetTempPath()) ('hm-artifact-tests-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $testDirectory | Out-Null

function Add-ZipBytes {
    param($Archive, [string]$Name, [byte[]]$Bytes)
    $stream = $Archive.CreateEntry($Name).Open()
    try { $stream.Write($Bytes, 0, $Bytes.Length) } finally { $stream.Dispose() }
}

try {
    $cases = @(
        @{ Name = 'valid'; Icon = '<icon>icon.png</icon>'; File = $true; Error = $null },
        @{ Name = 'missing-declaration'; Icon = ''; File = $true; Error = "must declare 'icon.png'" },
        @{ Name = 'missing-file'; Icon = '<icon>icon.png</icon>'; File = $false; Error = "missing 'icon.png'" },
        @{ Name = 'wrong-path'; Icon = '<icon>images/icon.png</icon>'; File = $true; Error = "must declare 'icon.png'" },
        @{ Name = 'wrong-case'; Icon = '<icon>Icon.png</icon>'; File = $true; Error = "must declare 'icon.png'" },
        @{ Name = 'invalid-png'; Icon = '<icon>icon.png</icon>'; File = $true; Error = 'PNG signature' }
    )
    foreach ($case in $cases) {
        $directory = Join-Path $testDirectory $case.Name
        New-Item -ItemType Directory -Path $directory | Out-Null
        $package = [IO.Compression.ZipFile]::Open((Join-Path $directory "HDev.Hm.Logging.Core.$version.nupkg"), 'Create')
        try {
            $xml = "<package><metadata><id>HDev.Hm.Logging.Core</id><version>$version</version>$($case.Icon)<repository commit='$commit'/></metadata></package>"
            Add-ZipBytes $package 'HDev.Hm.Logging.Core.nuspec' ([Text.Encoding]::UTF8.GetBytes($xml))
            foreach ($name in @('README.md', 'lib/net10.0/Hm.Logging.dll', 'lib/net10.0/Hm.Logging.xml')) {
                Add-ZipBytes $package $name ([byte[]]@(1))
            }
            if ($case.File) {
                $bytes = if ($case.Name -eq 'invalid-png') { [byte[]]::new(8) } else { [IO.File]::ReadAllBytes((Join-Path $repositoryRoot 'icon.png')) }
                Add-ZipBytes $package 'icon.png' $bytes
            }
        }
        finally { $package.Dispose() }
        $symbols = [IO.Compression.ZipFile]::Open((Join-Path $directory "HDev.Hm.Logging.Core.$version.snupkg"), 'Create')
        try { Add-ZipBytes $symbols 'lib/net10.0/Hm.Logging.pdb' ([IO.File]::ReadAllBytes($pdbPath)) }
        finally { $symbols.Dispose() }

        $failure = $null
        try {
            & (Join-Path $repositoryRoot 'scripts/Validate-ReleaseArtifact.ps1') -PackageDirectory $directory -ReleaseVersion $version -SourceCommit $commit
        }
        catch { $failure = $_.Exception.Message }
        if ($null -eq $case.Error) {
            if ($null -ne $failure) { throw "Valid package rejected: $failure" }
        }
        elseif ($null -eq $failure -or $failure -notlike "*$($case.Error)*") {
            throw "Case '$($case.Name)' expected '$($case.Error)', received '$failure'."
        }
    }
}
finally { Remove-Item -LiteralPath $testDirectory -Recurse -Force }
Write-Output 'Release artifact tests passed (6 icon scenarios, including portable PDB/Source Link validation).'
