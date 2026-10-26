param([string]$Source,[string]$AssetName,[string]$LayerName,[string[]]$Colors,[switch]$Transparent)
$ErrorActionPreference='Stop'
$template=Get-Content -LiteralPath (Join-Path $PSScriptRoot 'make-dirt.ps1') -Raw
$template=$template.Replace('$PSScriptRoot',("'" + $PSScriptRoot.Replace("'","''") + "'"))
$template=$template.Replace('References/exec-719b9670-27b3-44c1-9c89-fb3b26cc36df.png',$Source)
$colorList=($Colors | ForEach-Object { "'$_'" }) -join ','
$template=$template.Replace("'50321f','654129','795032','895a33','9c683b','b27e46','cc9856','958579'",$colorList)
$template=$template.Replace('dirt-block',$AssetName).Replace("GetBytes('Dirt')","GetBytes('$LayerName')").Replace('editable Dirt layer',"editable $LayerName layer")
if($Transparent) {
 $template=$template.Replace('$tile.SetPixel($x,$y,$best)', 'if($sample.A -lt 128) {$best=[Drawing.Color]::FromArgb(0,0,0,0)}; $tile.SetPixel($x,$y,$best)')
 $template=$template.Replace('$pixels[$i+3]=255','$pixels[$i+3]=$best.A')
}
& ([scriptblock]::Create($template))
