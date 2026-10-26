Add-Type -AssemblyName System.Drawing
$names=@('temperate-water','desert-water','tundra-water','jungle-water','swamp-water')
$sheet=New-Object Drawing.Bitmap 588,448
$g=[Drawing.Graphics]::FromImage($sheet)
$g.Clear([Drawing.Color]::FromArgb(36,39,44))
$font=New-Object Drawing.Font 'Arial',11
for($i=0;$i -lt $names.Count;$i++) {
 $x=([int]($i%3))*196+8; $y=[int][Math]::Floor($i/3)*224+8
 $img=[Drawing.Bitmap]::FromFile((Join-Path $PSScriptRoot ('output\'+$names[$i]+'-preview.png')))
 $g.InterpolationMode=[Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
 $g.PixelOffsetMode=[Drawing.Drawing2D.PixelOffsetMode]::Half
 $g.DrawImage($img,$x,$y,180,180)
 $g.DrawString($names[$i],$font,[Drawing.Brushes]::White,$x,($y+186))
 $img.Dispose()
}
$sheet.Save((Join-Path $PSScriptRoot 'output\water-set-preview.png'),[Drawing.Imaging.ImageFormat]::Png)
$font.Dispose();$g.Dispose();$sheet.Dispose()
