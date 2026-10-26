param([string]$Repo=(Split-Path (Split-Path $PSScriptRoot)))
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$project=Join-Path $Repo 'tervain--shattered-realms'
$root=Join-Path $project 'Art-work\Pixel16'
$manifest=Get-Content (Join-Path $root 'manifest.json') -Raw|ConvertFrom-Json
$total=0;$variantSets=0
foreach($entry in $manifest){
 $paths=@($entry.png)+@($entry.variants);$border=$null
 foreach($resource in $paths){
  $png=Join-Path $project $resource.Replace('res://','');$ase=Join-Path $root ('Sources\'+[IO.Path]::GetFileNameWithoutExtension($png)+'.ase')
  $im=[Drawing.Bitmap]::FromFile($png);if($im.Width -ne 16 -or $im.Height -ne 16){throw "Wrong dimensions: $png"}
  $colors=[Collections.Generic.HashSet[int]]::new();$pixels=New-Object byte[] 1024;$edges=[Collections.Generic.List[int]]::new()
  for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){$c=$im.GetPixel($x,$y);if($c.A -notin @(0,255)){throw "Nonbinary alpha: $png"};if($c.A -eq 255){[void]$colors.Add($c.ToArgb())};$i=($y*16+$x)*4;$pixels[$i]=$c.R;$pixels[$i+1]=$c.G;$pixels[$i+2]=$c.B;$pixels[$i+3]=$c.A;if($x -eq 0 -or $x -eq 15 -or $y -eq 0 -or $y -eq 15){$edges.Add($c.ToArgb())}}}
  if($colors.Count -gt 4){throw "Too many colors: $png"}
  if($paths.Count -gt 1){$signature=$edges -join ',';if($null -eq $border){$border=$signature}elseif($border -ne $signature){throw "Variant border mismatch: $png"}}
  $bytes=[IO.File]::ReadAllBytes($ase);if([BitConverter]::ToUInt16($bytes,6) -ne 1 -or [BitConverter]::ToUInt16($bytes,8) -ne 16 -or [BitConverter]::ToUInt16($bytes,10) -ne 16){throw "Invalid ASE header: $ase"}
  $actual=[Convert]::ToBase64String($bytes[($bytes.Length-1024)..($bytes.Length-1)]);if($actual -ne [Convert]::ToBase64String($pixels)){throw "PNG/ASE mismatch: $ase"}
  $im.Dispose();$total++
 }
 if($paths.Count -gt 1){if($paths.Count -ne 4){throw 'Expected four variants'};$variantSets++}
}
# Render a large patch for assessing joins and repeated motifs.
$names=@('dirt-block','stone-block','wood-block','sand-block','snow-block','frozen-caves-stone-block','temperate-forest-soil-background','magma-caverns-liquid-body')
$preview=[Drawing.Bitmap]::new(1024,($names.Count*144));$g=[Drawing.Graphics]::FromImage($preview);$g.Clear([Drawing.Color]::FromArgb(26,29,34));$g.InterpolationMode=[Drawing.Drawing2D.InterpolationMode]::NearestNeighbor;$g.PixelOffsetMode=[Drawing.Drawing2D.PixelOffsetMode]::Half;$font=[Drawing.Font]::new('Arial',10)
$row=0
foreach($name in $names){$e=$manifest|Where-Object name -eq "$name-16";if(!$e){throw "Missing $name"};$ims=@(@($e.png)+@($e.variants)|ForEach-Object{[Drawing.Bitmap]::FromFile((Join-Path $project $_.Replace('res://','')))})
 $g.DrawString("$name : mixed seamless variants",$font,[Drawing.Brushes]::White,8,($row*144))
 for($y=0;$y -lt 3;$y++){for($x=0;$x -lt 24;$x++){$v=($x*13+$y*7+($x*$y)%11)%4;$g.DrawImage($ims[$v],[Drawing.Rectangle]::new(($x*32),($row*144+24+$y*32),32,32))}}
 foreach($im in $ims){$im.Dispose()};$row++
}
$g.Dispose();$font.Dispose();$preview.Save((Join-Path $root 'previews\repetition.png'));$preview.Dispose()
Write-Output "PASS: $($manifest.Count) base assets; $variantSets four-variant sets; $total native 16px PNG/ASE pairs; <=4 opaque colors; binary alpha; all variant borders match."
