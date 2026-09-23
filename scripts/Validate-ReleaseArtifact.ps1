[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$PackageDirectory,
    [Parameter(Mandatory)][string]$ReleaseVersion,
    [Parameter(Mandatory)][string]$SourceCommit
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression

$directory = (Resolve-Path -LiteralPath $PackageDirectory).Path
$packageName = "HDev.Hm.Logging.Core.$ReleaseVersion.nupkg"
$symbolName = "HDev.Hm.Logging.Core.$ReleaseVersion.snupkg"
$packagePath = Join-Path $directory $packageName
$symbolPath = Join-Path $directory $symbolName
$packageCandidates = @(Get-ChildItem -LiteralPath $directory -Filter 'HDev.Hm.Logging.Core.*.nupkg')
$symbolCandidates = @(Get-ChildItem -LiteralPath $directory -Filter 'HDev.Hm.Logging.Core.*.snupkg')
if ($packageCandidates.Count -ne 1 -or $packageCandidates[0].Name -cne $packageName -or $symbolCandidates.Count -ne 1 -or $symbolCandidates[0].Name -cne $symbolName) {
    throw 'Exactly the expected .nupkg and .snupkg release artifacts must be present.'
}

$package = [System.IO.Compression.ZipFile]::OpenRead($packagePath)
try {
    $entries = @($package.Entries.FullName)
    foreach ($entry in @('README.md', 'icon.png', 'lib/net10.0/Hm.Logging.dll', 'lib/net10.0/Hm.Logging.xml')) {
        if ($entries -notcontains $entry) { throw "Release package is missing '$entry'." }
    }

    $nuspec = $package.Entries | Where-Object FullName -eq 'HDev.Hm.Logging.Core.nuspec'
    if (@($nuspec).Count -ne 1) { throw 'Release package has an unexpected nuspec.' }
    $reader = [System.IO.StreamReader]::new($nuspec.Open())
    try { [xml]$metadata = $reader.ReadToEnd() } finally { $reader.Dispose() }
    $iconNode = $metadata.SelectSingleNode('/*[local-name()="package"]/*[local-name()="metadata"]/*[local-name()="icon"]')
    if ($null -eq $iconNode -or $iconNode.InnerText -cne 'icon.png') {
        throw "Release package must declare 'icon.png' as its nuspec icon."
    }
    $iconEntries = @($package.Entries | Where-Object { $_.FullName -ceq $iconNode.InnerText })
    if ($iconEntries.Count -ne 1) { throw 'The declared package icon must resolve to exactly one package entry.' }
    $iconStream = $iconEntries[0].Open()
    try {
        $signature = [byte[]]::new(8)
        $iconStream.ReadExactly($signature, 0, $signature.Length)
        if ([Convert]::ToHexString($signature) -cne '89504E470D0A1A0A') {
            throw 'The declared package icon does not have a PNG signature.'
        }
    }
    finally { $iconStream.Dispose() }
    if ($metadata.package.metadata.id -ne 'HDev.Hm.Logging.Core' -or $metadata.package.metadata.version -ne $ReleaseVersion -or $metadata.package.metadata.repository.commit -ne $SourceCommit) {
        throw 'Release package metadata does not match the expected package and source identity.'
    }
}
finally { $package.Dispose() }

$symbols = [System.IO.Compression.ZipFile]::OpenRead($symbolPath)
try {
    $pdbEntries = @($symbols.Entries | Where-Object FullName -eq 'lib/net10.0/Hm.Logging.pdb')
    if ($pdbEntries.Count -ne 1) { throw 'Symbol package does not contain the expected portable PDB.' }
    $stream = $pdbEntries[0].Open()
    try {
        $pdbStream = [System.IO.MemoryStream]::new()
        try {
            $stream.CopyTo($pdbStream)
            $pdbStream.Position = 0
            $provider = [System.Reflection.Metadata.MetadataReaderProvider]::FromPortablePdbStream($pdbStream)
            try {
                $reader = $provider.GetMetadataReader()
                $sourceLinkGuid = [Guid]'CC110556-A091-4D38-9FEC-25AB9A351A6A'
                $sourceLink = @($reader.CustomDebugInformation | Where-Object { $reader.GetGuid($reader.GetCustomDebugInformation($_).Kind) -eq $sourceLinkGuid })
                if ($sourceLink.Count -ne 1) { throw 'Portable PDB does not contain Source Link metadata.' }
            }
            finally { $provider.Dispose() }
        }
        finally { $pdbStream.Dispose() }
    }
    finally { $stream.Dispose() }
}
finally { $symbols.Dispose() }
