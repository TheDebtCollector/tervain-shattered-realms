param([string]$Repo = (Split-Path (Split-Path $PSScriptRoot)))
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$project = Join-Path $Repo 'tervain--shattered-realms'
$art = Join-Path $project 'Art-work'
$out = Join-Path $art 'Pixel16'
foreach($folder in @('Sources','Exported','previews','References')) { [void][IO.Directory]::CreateDirectory((Join-Path $out $folder)) }
# The retained References/simple-style-guide.png documents the imagegen art direction.
Set-Content (Join-Path $out 'previews\.gdignore') ''
Set-Content (Join-Path $out 'References\.gdignore') ''
function MeanColor($image, [int]$top=0, [int]$bottom=24) {
 $r=0.; $g=0.; $b=0.; $n=0
 for($y=$top;$y -lt [Math]::Min($bottom,$image.Height);$y++){for($x=0;$x -lt $image.Width;$x++){
  $c=$image.GetPixel($x,$y); if($c.A -ge 128){$r+=$c.R;$g+=$c.G;$b+=$c.B;$n++}
 }}
 if(!$n){return [Drawing.Color]::FromArgb(255,60,60,60)}
 return [Drawing.Color]::FromArgb(255,[int]($r/$n),[int]($g/$n),[int]($b/$n))
}
function Shade($c,[double]$factor){return [Drawing.Color]::FromArgb(255,[int][Math]::Clamp($c.R*$factor,0,255),[int][Math]::Clamp($c.G*$factor,0,255),[int][Math]::Clamp($c.B*$factor,0,255))}
function WriteAse($image,$path) {
 $pixels=New-Object byte[] 1024
 for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){$c=$image.GetPixel($x,$y);$i=($y*16+$x)*4;$pixels[$i]=$c.R;$pixels[$i+1]=$c.G;$pixels[$i+2]=$c.B;$pixels[$i+3]=$c.A}}
 $stream=[IO.MemoryStream]::new();$w=[IO.BinaryWriter]::new($stream)
 $w.Write((New-Object byte[] 128));$w.Write([uint32]0);$w.Write([uint16]0xF1FA);$w.Write([uint16]2);$w.Write([uint16]100);$w.Write([uint16]0);$w.Write([uint32]0)
 $name=[Text.Encoding]::UTF8.GetBytes('Pixel16');$w.Write([uint32](24+$name.Length));$w.Write([uint16]0x2004);$w.Write([uint16]3)
 1..5|ForEach-Object{$w.Write([uint16]0)};$w.Write([byte]255);$w.Write((New-Object byte[] 3));$w.Write([uint16]$name.Length);$w.Write($name)
 $w.Write([uint32](26+$pixels.Length));$w.Write([uint16]0x2005);$w.Write([uint16]0);$w.Write([int16]0);$w.Write([int16]0);$w.Write([byte]255);$w.Write([uint16]0);$w.Write([int16]0);$w.Write((New-Object byte[] 5));$w.Write([uint16]16);$w.Write([uint16]16);$w.Write($pixels)
 $length=$stream.Length;$stream.Position=0;$w.Write([uint32]$length);$w.Write([uint16]0xA5E0);$w.Write([uint16]1);$w.Write([uint16]16);$w.Write([uint16]16);$w.Write([uint16]32);$w.Write([uint32]1);$w.Write([uint16]100)
 $stream.Position=34;$w.Write([byte]1);$w.Write([byte]1);$stream.Position=128;$w.Write([uint32]($length-128))
 [IO.File]::WriteAllBytes($path,$stream.ToArray());$w.Dispose();$stream.Dispose()
}
function SaveTile($image,$stem){$image.Save((Join-Path $out "Exported\$stem.png"),[Drawing.Imaging.ImageFormat]::Png);WriteAse $image (Join-Path $out "Sources\$stem.ase")}
$entries=[Collections.Generic.List[object]]::new()
$files=@(Get-ChildItem (Join-Path $art 'Exported') -Filter '*-24.png')+@(Get-ChildItem (Join-Path $art 'BiomeExpansion\Exported') -Filter '*-24.png')
$preview=[Drawing.Bitmap]::new(1024,[int]([Math]::Ceiling($files.Count/8)*112))
$pg=[Drawing.Graphics]::FromImage($preview);$pg.Clear([Drawing.Color]::FromArgb(26,29,34));$pg.InterpolationMode=[Drawing.Drawing2D.InterpolationMode]::NearestNeighbor;$pg.PixelOffsetMode=[Drawing.Drawing2D.PixelOffsetMode]::Half
$font=[Drawing.Font]::new('Arial',7)
$idx=0
foreach($file in $files){
 $src=[Drawing.Bitmap]::FromFile($file.FullName);$stem=$file.BaseName -replace '-24$','-16'
 $transparent=$false;for($y=0;$y -lt $src.Height;$y++){for($x=0;$x -lt $src.Width;$x++){if($src.GetPixel($x,$y).A -lt 128){$transparent=$true}}}
 # Cactus is a continuous trunk tile; decorations preserve their transparent silhouettes.
 $decoration=$transparent -and $stem -notmatch '^cactus-'
 $base=MeanColor $src 4 $src.Height;$dark=Shade $base .92;$light=Shade $base 1.08
 $surface=$stem -match 'surface-block|liquid-surface';$water=$stem -match 'water|liquid';$wood=$stem -match 'wood|material-block'
 $top=MeanColor $src 0 3
 $count=if($decoration -or $stem -match '^cactus-'){1}else{4}
 $variants=@()
 for($v=0;$v -lt $count;$v++){
  $tile=[Drawing.Bitmap]::new(16,16)
  if($decoration -or $stem -match '^cactus-'){
   $counts=@{};for($y=0;$y -lt $src.Height;$y++){for($x=0;$x -lt $src.Width;$x++){$c=$src.GetPixel($x,$y);if($c.A -ge 128){$key=$c.ToArgb();if(!$counts.ContainsKey($key)){$counts[$key]=0};$counts[$key]++}}}
   $palette=@($counts.GetEnumerator()|Sort-Object Value -Descending|Select-Object -First 3|ForEach-Object{[Drawing.Color]::FromArgb([int]$_.Key)})
   for($y=0;$y -lt 16;$y++){for($x=0;$x -lt 16;$x++){
    $c=$src.GetPixel([int][Math]::Floor(($x+.5)*$src.Width/16),[int][Math]::Floor(($y+.5)*$src.Height/16))
    if($c.A -lt 128){$tile.SetPixel($x,$y,[Drawing.Color]::Transparent);continue}
    $best=$palette[0];$dist=[double]::PositiveInfinity;foreach($p in $palette){$d=[Math]::Pow($c.R-$p.R,2)+[Math]::Pow($c.G-$p.G,2)+[Math]::Pow($c.B-$p.B,2);if($d -lt $dist){$best=$p;$dist=$d}}
    $tile.SetPixel($x,$y,$best)
   }}
  }else{
   $g=[Drawing.Graphics]::FromImage($tile);$g.Clear($base)
   # Sparse low-contrast clusters; borders stay shared across all four variants.
   $seed=($idx+1)*1009+($v+1)*7919;$rng=[Random]::new($seed)
   $brushes=@([Drawing.SolidBrush]::new($dark),[Drawing.SolidBrush]::new($light))
   for($n=0;$n -lt 4;$n++){$x=$rng.Next(2,12);$y=$rng.Next(3,13);$w=$rng.Next(2,4);$h=if($water){1}elseif($wood){$rng.Next(2,4)}else{2};$g.FillRectangle($brushes[$n%2],$x,$y,$w,$h)}
   if($surface){$tb=[Drawing.SolidBrush]::new($top);$g.FillRectangle($tb,0,0,16,2);for($n=0;$n -lt 3;$n++){$g.FillRectangle($tb,$rng.Next(1,14),2,2,1)};$tb.Dispose()}
   if($stem -match 'iron-ore'){$ore=MeanColor $src 0 $src.Height;$ob=[Drawing.SolidBrush]::new((Shade $ore 1.5));$g.FillRectangle($ob,3+$v,4,3,2);$g.FillRectangle($ob,9-$v,10,3,2);$ob.Dispose()}
   foreach($b in $brushes){$b.Dispose()};$g.Dispose()
  }
  $name=if($v -eq 0){$stem}else{"$stem-v$v"};SaveTile $tile $name
  if($v -gt 0){$variants+="res://Art-work/Pixel16/Exported/$name.png"}
  if($v -eq 0){$px=($idx%8)*128;$py=[int][Math]::Floor($idx/8)*112;$pg.DrawImage($tile,[Drawing.Rectangle]::new($px,$py,64,64));$pg.DrawString(($stem -replace '-16$','' -replace '-',' '),$font,[Drawing.Brushes]::White,[Drawing.RectangleF]::new($px,$py+66,126,42))}
  $tile.Dispose()
 }
 $entries.Add([ordered]@{name=$stem;source="Sources/$stem.ase";png="res://Art-work/Pixel16/Exported/$stem.png";width=16;height=16;variants=$variants;transparent=$transparent})
 $src.Dispose();$idx++
}
 $pg.Dispose();$font.Dispose();$preview.Save((Join-Path $out 'previews\all-assets.png'));$preview.Dispose()
 $entries|ConvertTo-Json -Depth 5|Set-Content (Join-Path $out 'manifest.json')
 Write-Output "Created $($entries.Count) native 16x16 assets with matching editable ASE sources and terrain variants."
