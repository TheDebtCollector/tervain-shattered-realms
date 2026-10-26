Add-Type -AssemblyName System.Drawing
$outDir = Join-Path $PSScriptRoot 'output'
[void][IO.Directory]::CreateDirectory($outDir)
$source = [Drawing.Bitmap]::FromFile('References/exec-eb2138c7-d9b2-4903-88cc-517cb2cfaa9c.png')
$tile = New-Object Drawing.Bitmap 24,24
$palette = @('383d43','484f57','5a626b','6b747e','7d8791','919ba4','a8b1b7') | ForEach-Object { [Drawing.ColorTranslator]::FromHtml('#' + $_) }
$pixels = New-Object byte[] (24*24*4)
for($y=0;$y -lt 24;$y++) {
 for($x=0;$x -lt 24;$x++) {
  $sample=$source.GetPixel([int][Math]::Floor(($x+0.5)*$source.Width/24),[int][Math]::Floor(($y+0.5)*$source.Height/24))
  $best=$palette[0]; $distance=[double]::PositiveInfinity
  foreach($color in $palette) {
   $d=[Math]::Pow($sample.R-$color.R,2)+[Math]::Pow($sample.G-$color.G,2)+[Math]::Pow($sample.B-$color.B,2)
   if($d -lt $distance) {$best=$color; $distance=$d}
  }
  $tile.SetPixel($x,$y,$best)
  $i=($y*24+$x)*4
  $pixels[$i]=$best.R; $pixels[$i+1]=$best.G; $pixels[$i+2]=$best.B; $pixels[$i+3]=255
 }
}
$tile.Save((Join-Path $outDir 'stone-block-24.png'),[Drawing.Imaging.ImageFormat]::Png)
$preview=New-Object Drawing.Bitmap 384,384
for($y=0;$y -lt 384;$y++) { for($x=0;$x -lt 384;$x++) { $preview.SetPixel($x,$y,$tile.GetPixel([int][Math]::Floor($x/16),[int][Math]::Floor($y/16))) } }
$preview.Save((Join-Path $outDir 'stone-block-preview.png'),[Drawing.Imaging.ImageFormat]::Png)
$stream=New-Object IO.MemoryStream
$writer=New-Object IO.BinaryWriter $stream
$writer.Write((New-Object byte[] 128))
$writer.Write([uint32]0); $writer.Write([uint16]0xF1FA); $writer.Write([uint16]2); $writer.Write([uint16]100); $writer.Write([uint16]0); $writer.Write([uint32]2)
$name=[Text.Encoding]::UTF8.GetBytes('Stone')
$writer.Write([uint32](24+$name.Length)); $writer.Write([uint16]0x2004)
$writer.Write([uint16]3)
1..5 | ForEach-Object {$writer.Write([uint16]0)}
$writer.Write([byte]255); $writer.Write((New-Object byte[] 3)); $writer.Write([uint16]$name.Length); $writer.Write($name)
$writer.Write([uint32](26+$pixels.Length)); $writer.Write([uint16]0x2005)
$writer.Write([uint16]0); $writer.Write([int16]0); $writer.Write([int16]0); $writer.Write([byte]255); $writer.Write([uint16]0); $writer.Write([int16]0); $writer.Write((New-Object byte[] 5)); $writer.Write([uint16]24); $writer.Write([uint16]24); $writer.Write($pixels)
$length=$stream.Length
$stream.Position=0
$writer.Write([uint32]$length); $writer.Write([uint16]0xA5E0); $writer.Write([uint16]1); $writer.Write([uint16]24); $writer.Write([uint16]24); $writer.Write([uint16]32); $writer.Write([uint32]1); $writer.Write([uint16]100)
$stream.Position=34; $writer.Write([byte]1); $writer.Write([byte]1)
$stream.Position=128; $writer.Write([uint32]($length-128))
$asePath=Join-Path $outDir 'stone-block-24.ase'
[IO.File]::WriteAllBytes($asePath,$stream.ToArray())
$reader=New-Object IO.BinaryReader ([IO.MemoryStream]::new([IO.File]::ReadAllBytes($asePath)))
if($reader.ReadUInt32() -ne $length -or $reader.ReadUInt16() -ne 0xA5E0 -or $reader.ReadUInt16() -ne 1 -or $reader.ReadUInt16() -ne 24 -or $reader.ReadUInt16() -ne 24) {throw 'Invalid ASE header'}
$reader.BaseStream.Position=144
for($n=0;$n -lt 2;$n++) { $start=$reader.BaseStream.Position; $size=$reader.ReadUInt32(); $type=$reader.ReadUInt16(); if($size -lt 6 -or $start+$size -gt $length){throw 'Invalid chunk'}; $reader.BaseStream.Position=$start+$size }
if($reader.BaseStream.Position -ne $length) {throw 'Invalid file length'}
$reader.Dispose(); $writer.Dispose(); $stream.Dispose(); $source.Dispose(); $tile.Dispose(); $preview.Dispose()
Write-Output "Validated 24x24 RGBA ASE with one editable Stone layer: $asePath"
