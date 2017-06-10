
param(
 [string] $gameName,
 [string] $mainProgramName
)

$inpath = Join-Path (Join-Path . $gameName) $gameName
$outpath = Join-Path . $gameName
$outTempFile = Join-Path $outpath "CombinedOutput.txt"
$outFinalFile = Join-Path $outpath "$GameName.cs"
$usingFile = Join-Path $outpath "UsingStatements.cs"
$mainPath = Join-Path $inpath $mainProgramName

if (Test-Path -Path $outTempFile) { Remove-Item $outTempFile }
if (Test-Path -Path $outFinalFile) { Remove-Item $outFinalFile }

#Set-Content -Path $outTempFile -Value (get-content -Path $mainPath)

Get-ChildItem -path $inpath |?{ ! $_.PSIsContainer } |?{($_.Extension).Equals(".cs") -and $_.Name -ne $mainProgramName} | %{ Out-File -filepath $outTempFile -inputobject (get-content $_.fullname) -Append}

Set-Content -Path $outFinalFile -Value (get-content -Path $usingFile)
Add-Content -Path $outFinalFile -Value (get-content -Path $mainPath | Select-String -Pattern 'using ' -NotMatch)
Add-Content -Path $outFinalFile -Value (get-content -Path $outTempFile | Select-String -Pattern 'using ' -NotMatch)

if (Test-Path -Path $outTempFile) { Remove-Item $outTempFile }
