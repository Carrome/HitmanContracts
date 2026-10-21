# Builds the Thunderstore package: dist\HitmanContracts-<version>.zip
# Usage: powershell -ExecutionPolicy Bypass -File pack.ps1 [-GameDir "E:\Shadows of Doubt"]
param([string]$GameDir = 'E:\Shadows of Doubt')
$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$meta = Join-Path $root 'thunderstore'
$content = Join-Path $root 'content'
$manifest = Get-Content (Join-Path $meta 'manifest.json') -Raw | ConvertFrom-Json

& dotnet build (Join-Path $root 'src\SOD.Hitman\SOD.Hitman.csproj') -c Release --nologo -v quiet -p:GameDir="$GameDir"
if ($LASTEXITCODE -ne 0) { throw 'Build failed' }
$dll = Join-Path $root 'src\SOD.Hitman\bin\Release\net6.0\SOD.Hitman.dll'

# Gate: no Harmony patch may target a method IL2CPP folded with others (empty bodies, 'return false'...).
# Such a patch runs for every method sharing that machine code, with the wrong 'this', and crashes the game.
$fold = Join-Path $root 'tools\foldcheck'
& dotnet build (Join-Path $fold 'foldcheck.csproj') -c Release --nologo -v quiet -o (Join-Path $fold 'bin\out') -p:GameDir="$GameDir"
if ($LASTEXITCODE -ne 0) { throw 'foldcheck build failed' }
$targets = @(Get-Content (Join-Path $fold 'hitman-patches.txt') | Where-Object { $_.Trim() })
$src = (& ilspycmd -r "$GameDir\BepInEx\core" -r "$GameDir\BepInEx\interop" -r "$GameDir\BepInEx\plugins\Venomaus-SODCommon" $dll 2>$null) -join "`n"
$targets += [regex]::Matches($src, '\[HarmonyPatch\(typeof\(([A-Za-z0-9_.]+)\),\s*"([A-Za-z0-9_]+)"') |
    ForEach-Object { ($_.Groups[1].Value -replace '\.', '+') + '::' + $_.Groups[2].Value }
$targets = $targets | Sort-Object -Unique
$report = & (Join-Path $fold 'bin\out\foldcheck.exe') $GameDir @targets
$exit = $LASTEXITCODE
$report | Where-Object { $_ -match '^(ok|FOLDED|NO CODE|\?\?)|also:' }
if ($exit -ne 0) { throw 'foldcheck: a Harmony patch targets a folded method (see FOLDED above)' }

$dist = Join-Path $root 'dist'
New-Item -ItemType Directory -Force $dist | Out-Null
$zipPath = Join-Path $dist ("{0}-{1}.zip" -f $manifest.name, $manifest.version_number)
if (Test-Path $zipPath) { Remove-Item $zipPath }

Add-Type -AssemblyName System.IO.Compression, System.IO.Compression.FileSystem
$zip = [System.IO.Compression.ZipFile]::Open($zipPath, 'Create')
try {
    # Entry names use forward slashes; mod managers reject backslash paths.
    foreach ($f in 'manifest.json', 'README.md', 'CHANGELOG.md', 'icon.png') {
        [void][System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, (Join-Path $meta $f), $f)
    }
    # MIT notices for the code adapted from Guns and LethalActionReborn
    [void][System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, (Join-Path $root 'THIRD_PARTY_NOTICES.md'), 'THIRD_PARTY_NOTICES.md')
    [void][System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, (Join-Path $root 'THIRD_PARTY_NOTICES.md'), 'plugins/SOD.Hitman/THIRD_PARTY_NOTICES.md')
    [void][System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $dll, 'plugins/SOD.Hitman/SOD.Hitman.dll')
    foreach ($file in Get-ChildItem $content -Recurse -File) {
        $relative = $file.FullName.Substring($content.Length + 1).Replace('\', '/')
        [void][System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $file.FullName, "plugins/SOD.Hitman/$relative")
    }
}
finally { $zip.Dispose() }
"Package: $zipPath"

# Nexus Mods: manual install, so the archive mirrors the game folder (BepInEx/plugins/SOD.Hitman/...).
# devmode.txt is never packed: the public build has developer mode off.
$nexusPath = Join-Path $dist ("{0}-{1}-Nexus.zip" -f $manifest.name, $manifest.version_number)
if (Test-Path $nexusPath) { Remove-Item $nexusPath }
$zip = [System.IO.Compression.ZipFile]::Open($nexusPath, 'Create')
try {
    $plugin = 'BepInEx/plugins/SOD.Hitman'
    [void][System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, (Join-Path $root 'nexus\INSTALL.txt'), 'INSTALL.txt')
    foreach ($f in 'README.md', 'CHANGELOG.md') {
        [void][System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, (Join-Path $meta $f), $f)
    }
    [void][System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, (Join-Path $root 'THIRD_PARTY_NOTICES.md'), "$plugin/THIRD_PARTY_NOTICES.md")
    [void][System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $dll, "$plugin/SOD.Hitman.dll")
    foreach ($file in Get-ChildItem $content -Recurse -File) {
        $relative = $file.FullName.Substring($content.Length + 1).Replace('\', '/')
        [void][System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $file.FullName, "$plugin/$relative")
    }
}
finally { $zip.Dispose() }
"Package: $nexusPath"
