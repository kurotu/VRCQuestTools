#!/usr/bin/env pwsh

# Downloads the astcenc CLI binaries used for ASTC texture compression and
# extracts them into Packages/com.github.kurotu.vrc-quest-tools/Editor/Tools/astcenc/.
# The binaries are not committed to the repository (see .gitignore); this
# script is the only way to (re)populate them for local development and CI.
#
# Usage: pwsh scripts/download-astcenc.ps1 [--platform windows|linux|all]

$ErrorActionPreference = "Stop"

$Platform = "all"
[string[]]$RemainingArgs = $args
for ($i = 0; $i -lt $RemainingArgs.Count; $i++) {
    $arg = $RemainingArgs[$i]
    if ($arg -eq "--platform") {
        if ($i + 1 -ge $RemainingArgs.Count) {
            Write-Error "Missing value for --platform"
            exit 1
        }
        $Platform = $RemainingArgs[$i + 1]
        $i++
    }
    elseif ($arg -like "--platform=*") {
        $Platform = $arg.Substring("--platform=".Length)
    }
    else {
        Write-Error "Unknown argument: $arg"
        exit 1
    }
}

if ($Platform -notin @("windows", "linux", "all")) {
    Write-Error "Invalid --platform: $Platform (expected windows|linux|all)"
    exit 1
}

$Version = "5.3.0"
$BaseUrl = "https://github.com/ARM-software/astc-encoder/releases/download/$Version"
$ToolsDir = "Packages/com.github.kurotu.vrc-quest-tools/Editor/Tools/astcenc"

$WinArchive = "astcenc-$Version-windows-x64.zip"
$WinArchiveSha256 = "199b2287be0264182292869798bef91c35a64791e52bfd43824d1d3ac3e7846f"
$WinAvx2Sha256 = "c2eb4bbbad344666ccd9c176f3b915077c057366681a3093c4047a7677e6504b"
$WinSse2Sha256 = "02154aaab77770cf279d09629fd75c1ae912e3748067616a38a2d75137429cdd"

$LinuxArchive = "astcenc-$Version-linux-x64.zip"
$LinuxArchiveSha256 = "495b2f0cf0357ae05728a727e3d0e81d6e7f27b242c21cb5ef6254dd56dba5ff"
$LinuxAvx2Sha256 = "eaa0d194e82790bd338ef00e69e5d085ae6a2134da30bf7b3f186b78fb19f5cb"
$LinuxSse2Sha256 = "61071c177f3c4b873097a223283be9b47d9c14f6a0f3de34d2e7c97019344300"

function Get-Sha256 {
    param([string]$Path)
    return (Get-FileHash -Path $Path -Algorithm SHA256).Hash.ToLowerInvariant()
}

function Test-Sha256 {
    param([string]$Path, [string]$Expected)
    if (-not (Test-Path $Path)) {
        return $false
    }
    return (Get-Sha256 -Path $Path) -eq $Expected.ToLowerInvariant()
}

function Assert-Sha256 {
    param([string]$Path, [string]$Expected)
    $actual = Get-Sha256 -Path $Path
    if ($actual -ne $Expected.ToLowerInvariant()) {
        Write-Error "SHA256 mismatch for $Path`n  expected: $Expected`n  actual:   $actual"
        exit 1
    }
}

function Install-AstcencPlatform {
    param(
        [string]$Label,
        [string]$Archive,
        [string]$ArchiveSha256,
        [string]$OutDir,
        [string]$Avx2Name,
        [string]$Avx2Sha256,
        [string]$Sse2Name,
        [string]$Sse2Sha256
    )

    $dest = Join-Path $ToolsDir $OutDir
    $avx2Dest = Join-Path $dest $Avx2Name
    $sse2Dest = Join-Path $dest $Sse2Name

    if ((Test-Sha256 $avx2Dest $Avx2Sha256) -and (Test-Sha256 $sse2Dest $Sse2Sha256)) {
        Write-Output "astcenc ($Label): already up to date, skipping."
        return
    }

    Write-Output "astcenc ($Label): downloading $Archive..."
    $work = Join-Path $TmpRoot $OutDir
    New-Item -ItemType Directory -Force -Path $work | Out-Null
    $archivePath = Join-Path $work $Archive
    Invoke-WebRequest -Uri "$BaseUrl/$Archive" -OutFile $archivePath -UseBasicParsing
    Assert-Sha256 $archivePath $ArchiveSha256

    $extractDir = Join-Path $work "extracted"
    New-Item -ItemType Directory -Force -Path $extractDir | Out-Null
    Expand-Archive -Path $archivePath -DestinationPath $extractDir -Force

    New-Item -ItemType Directory -Force -Path $dest | Out-Null
    Copy-Item -Path (Join-Path $extractDir "bin/$Avx2Name") -Destination $avx2Dest -Force
    Copy-Item -Path (Join-Path $extractDir "bin/$Sse2Name") -Destination $sse2Dest -Force

    Assert-Sha256 $avx2Dest $Avx2Sha256
    Assert-Sha256 $sse2Dest $Sse2Sha256

    Write-Output "astcenc ($Label): installed to $dest."
}

$TmpRoot = Join-Path ([System.IO.Path]::GetTempPath()) ([System.Guid]::NewGuid().ToString())
New-Item -ItemType Directory -Force -Path $TmpRoot | Out-Null

try {
    if ($Platform -eq "windows" -or $Platform -eq "all") {
        Install-AstcencPlatform -Label "windows" -Archive $WinArchive -ArchiveSha256 $WinArchiveSha256 `
            -OutDir "win-x64" -Avx2Name "astcenc-avx2.exe" -Avx2Sha256 $WinAvx2Sha256 `
            -Sse2Name "astcenc-sse2.exe" -Sse2Sha256 $WinSse2Sha256
    }

    if ($Platform -eq "linux" -or $Platform -eq "all") {
        # Note: extracted Linux binaries keep the executable bit set by the zip archive on
        # platforms that preserve POSIX permissions. This script does not chmod them; on
        # Windows there is no POSIX executable bit to set, and AstcencBinaryLocator chmods
        # the Linux binaries itself (via /bin/chmod) before first use when running on Linux.
        Install-AstcencPlatform -Label "linux" -Archive $LinuxArchive -ArchiveSha256 $LinuxArchiveSha256 `
            -OutDir "linux-x64" -Avx2Name "astcenc-avx2" -Avx2Sha256 $LinuxAvx2Sha256 `
            -Sse2Name "astcenc-sse2" -Sse2Sha256 $LinuxSse2Sha256
    }
}
finally {
    Remove-Item -Recurse -Force -Path $TmpRoot -ErrorAction SilentlyContinue
}
