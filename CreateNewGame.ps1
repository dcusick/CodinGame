param(
 [string] $gameName
)

Add-Type -AssemblyName System.IO.Compression.FileSystem
function Unzip
{
    param([string]$zipfile, [string]$outpath)

    [System.IO.Compression.ZipFile]::ExtractToDirectory($zipfile, $outpath)
}

$zipFile = Resolve-Path -LiteralPath "C:\dev\CodinGame\BackupProject.zip"
$gamePath = Join-Path "C:\dev\CodinGame" "$gameName"

if (!(Test-Path -Path $gamePath)) { New-Item -ItemType Directory -Force -Path $gamePath}

$gamePath = Resolve-Path -LiteralPath $gamePath

$oldPath = Join-Path $gamePath "Code4Life"
$newPath = Join-Path $gamePath $gameName
$assemblyInfo = Join-Path (Join-Path $newPath "Properties") "AssemblyInfo.cs"

$slnProj = Join-Path $newPath "Code4Life.sln"

Write-Output $zipFile
Write-Output $gamePath

Unzip $zipFile $gamePath

Rename-Item $oldPath $newPath
Rename-Item (Join-Path $newPath "Code4Life.csproj") (Join-Path $newPath "$gameName.csproj")
Rename-Item (Join-Path $newPath "Code4Life.sln") (Join-Path $newPath "$gameName.sln")

(Get-Content $assemblyInfo).replace('Code4Life', $gameName) | Set-Content $assemblyInfo
