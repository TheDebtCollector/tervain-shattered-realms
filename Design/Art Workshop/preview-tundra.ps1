Add-Type -AssemblyName System.Drawing
$names=@('snow-block','ice-block','frozen-earth-block','tundra-wood-block','tundra-shrub','snow-background','ice-background')
$sheet=New-Object Drawing.Bitmap 784,448
$g=[Drawing.Graphics]::FromImage($sheet)
$g.Clear([Drawing.Color]::FromArgb(36,39,44))
$font=New-Object Drawing.Font 'Arial',11
for($i=0;$i -lt $names.Count;$i++) {
 $x=([int]($i%4))*196+8; $y=[int][Math]::Floor($i/4)*224+8
 $img=[Drawing.Bitmap]::FromFile((Join-Path $PSScriptRoot ('output\'+$names[$i]+'-preview.png')))
 $g.InterpolationMode=[Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
 $g.PixelOffsetMode=[Drawing.Drawing2D.PixelOffsetMode]::Half
 $g.DrawImage($img,$x,$y,180,180)
 $g.DrawString($names[$i],$font,[Drawing.Brushes]::White,$x,($y+186))
 $img.Dispose()
}
$sheet.Save((Join-Path $PSScriptRoot 'output\tundra-set-preview.png'),[Drawing.Imaging.ImageFormat]::Png)
$font.Dispose();$g.Dispose();$sheet.Dispose()
foreach($name in @('snow-pine','tundra-shrub')) {
 $img=[Drawing.Bitmap]::FromFile((Join-Path $PSScriptRoot ('output\'+$name+'-24.png')))
 $transparent=0;$opaque=0
 for($y=0;$y -lt 24;$y++){for($x=0;$x -lt 24;$x++){if($img.GetPixel($x,$y).A -eq 0){$transparent++}else{$opaque++}}}
 if($transparent -eq 0 -or $opaque -eq 0){throw "Transparency invalid: $name"}
 Write-Output "$name verified: $opaque visible pixels, $transparent transparent pixels"
 $img.Dispose()
}
