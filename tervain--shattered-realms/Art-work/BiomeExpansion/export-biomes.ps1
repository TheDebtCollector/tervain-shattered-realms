param([string]$Root = $PSScriptRoot, [string]$Only = '')
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

function Write-Ase([string]$Path, [Drawing.Bitmap]$Tile, [string]$Name) {
    $pixels = New-Object byte[] (24*24*4)
    for ($y=0; $y -lt 24; $y++) { for ($x=0; $x -lt 24; $x++) {
        $c=$Tile.GetPixel($x,$y); $i=($y*24+$x)*4
        $pixels[$i]=$c.R; $pixels[$i+1]=$c.G; $pixels[$i+2]=$c.B; $pixels[$i+3]=$c.A
    }}
    $stream = New-Object IO.MemoryStream
    $w = New-Object IO.BinaryWriter $stream
    $w.Write((New-Object byte[] 128))
    $w.Write([uint32]0); $w.Write([uint16]0xF1FA); $w.Write([uint16]2); $w.Write([uint16]100); $w.Write([uint16]0); $w.Write([uint32]2)
    $nameBytes=[Text.Encoding]::UTF8.GetBytes($Name)
    $w.Write([uint32](24+$nameBytes.Length)); $w.Write([uint16]0x2004)
    $w.Write([uint16]3)
    1..5 | ForEach-Object { $w.Write([uint16]0) }
    $w.Write([byte]255); $w.Write((New-Object byte[] 3)); $w.Write([uint16]$nameBytes.Length); $w.Write($nameBytes)
    $w.Write([uint32](26+$pixels.Length)); $w.Write([uint16]0x2005)
    $w.Write([uint16]0); $w.Write([int16]0); $w.Write([int16]0); $w.Write([byte]255); $w.Write([uint16]0); $w.Write([int16]0); $w.Write((New-Object byte[] 5)); $w.Write([uint16]24); $w.Write([uint16]24); $w.Write($pixels)
    $length=$stream.Length; $stream.Position=0
    $w.Write([uint32]$length); $w.Write([uint16]0xA5E0); $w.Write([uint16]1); $w.Write([uint16]24); $w.Write([uint16]24); $w.Write([uint16]32); $w.Write([uint32]1); $w.Write([uint16]100)
    $stream.Position=34; $w.Write([byte]1); $w.Write([byte]1)
    $stream.Position=128; $w.Write([uint32]($length-128))
    [IO.File]::WriteAllBytes($Path,$stream.ToArray())
    $w.Dispose(); $stream.Dispose()
}

function Save-NearestPreview([Drawing.Bitmap]$Source, [string]$Path, [int]$Scale=5) {
    $big = New-Object Drawing.Bitmap ($Source.Width*$Scale),($Source.Height*$Scale)
    $g=[Drawing.Graphics]::FromImage($big)
    $g.Clear([Drawing.Color]::FromArgb(35,39,44))
    $g.InterpolationMode=[Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
    $g.PixelOffsetMode=[Drawing.Drawing2D.PixelOffsetMode]::Half
    $g.DrawImage($Source,[Drawing.Rectangle]::new(0,0,$big.Width,$big.Height),0,0,$Source.Width,$Source.Height,[Drawing.GraphicsUnit]::Pixel)
    $big.Save($Path,[Drawing.Imaging.ImageFormat]::Png)
    $g.Dispose(); $big.Dispose()
}

$specs=Get-Content -LiteralPath (Join-Path $Root 'specs.json') -Raw | ConvertFrom-Json
if ($Only) { $specs=@($specs | Where-Object { $_.id -eq $Only }) }
$sources=Get-Content -LiteralPath (Join-Path $Root 'sources.json') -Raw | ConvertFrom-Json
$art=Join-Path $Root 'Sources'
$exports=Join-Path $Root 'Exported'
$previews=Join-Path $Root 'previews'
foreach ($dir in @($art,$exports,$previews)) { [void][IO.Directory]::CreateDirectory($dir) }
$inventory=@()
$overview=New-Object Drawing.Bitmap 960,1560
$og=[Drawing.Graphics]::FromImage($overview); $og.Clear([Drawing.Color]::FromArgb(27,31,36))
$og.InterpolationMode=[Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
$og.PixelOffsetMode=[Drawing.Drawing2D.PixelOffsetMode]::Half
$font=New-Object Drawing.Font 'Segoe UI',12
$smallFont=New-Object Drawing.Font 'Segoe UI',8
$index=0
foreach ($spec in $specs) {
    $entry=$sources | Where-Object { $_.id -eq $spec.id }
    if (-not $entry) { throw "Missing generation source for $($spec.id)" }
    $sourcePath=if ([IO.Path]::IsPathRooted($entry.path)) { $entry.path } else { Join-Path $Root $entry.path }
    $source=[Drawing.Bitmap]::FromFile($sourcePath)
    # The first forest source has a low-alpha matte; the remaining sources have clear alpha cutouts.
    $cutoutAlphaThreshold=if ($spec.id -eq 'temperate_forest') { 238 } else { 128 }
    $sheet=New-Object Drawing.Bitmap 96,72
    $setPreview=New-Object Drawing.Bitmap 672,560
    $sg=[Drawing.Graphics]::FromImage($setPreview); $sg.Clear([Drawing.Color]::FromArgb(35,39,44))
    $sg.InterpolationMode=[Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
    $sg.PixelOffsetMode=[Drawing.Drawing2D.PixelOffsetMode]::Half
    $sg.DrawString($spec.name,$font,[Drawing.Brushes]::White,8,8)
    for ($cell=0; $cell -lt 12; $cell++) {
        $cx=$cell%4; $cy=[int][Math]::Floor($cell/4)
        $tile=New-Object Drawing.Bitmap 24,24
        $colors=@{}
        for ($y=0; $y -lt 24; $y++) { for ($x=0; $x -lt 24; $x++) {
            $px=[int][Math]::Min($source.Width-1,[Math]::Floor(($cx+($x+0.5)/24)*$source.Width/4))
            $py=[int][Math]::Min($source.Height-1,[Math]::Floor(($cy+($y+0.5)/24)*$source.Height/3))
            $c=$source.GetPixel($px,$py)
            # Opaque square blocks cannot retain transparent atlas-edge pixels or their hidden matte RGB.
            # Extend the nearest valid sample in this column into those export-only gaps.
            if ($cell -lt 8 -and $c.A -lt 128) {
                $found=$false
                for ($radius=1; $radius -lt 24 -and -not $found; $radius++) {
                    foreach ($ny in @(($y+$radius),($y-$radius))) {
                        if ($ny -lt 0 -or $ny -ge 24) { continue }
                        $spy=[int][Math]::Min($source.Height-1,[Math]::Floor(($cy+($ny+0.5)/24)*$source.Height/3))
                        $sample=$source.GetPixel($px,$spy)
                        if ($sample.A -ge 238) { $c=$sample; $found=$true; break }
                    }
                }
                if (-not $found) { throw "No opaque sample in material column: $($spec.id) $cell x=$x" }
            }
            if ($cell -lt 8) { $c=[Drawing.Color]::FromArgb(255,$c.R,$c.G,$c.B) }
            elseif ($c.A -lt $cutoutAlphaThreshold) { $c=[Drawing.Color]::FromArgb(0,0,0,0) }
            else { $c=[Drawing.Color]::FromArgb(255,$c.R,$c.G,$c.B) }
            # Keep the requested one-pixel side clearance and remove atlas-edge alpha fringe.
            if ($cell -ge 8 -and ($x -eq 0 -or $x -eq 23 -or $y -eq 23 -or ($cell -lt 11 -and $y -eq 0))) {
                $c=[Drawing.Color]::FromArgb(0,0,0,0)
            }
            $tile.SetPixel($x,$y,$c)
            if ($c.A -gt 0) {
                $key=('{0:X2}{1:X2}{2:X2}' -f $c.R,$c.G,$c.B)
                if ($colors.ContainsKey($key)) { $colors[$key]++ } else { $colors[$key]=1 }
            }
        }}
        # Pick eight frequent, distinct colors, keeping the generated palette rather than inventing new colors.
        $palette=New-Object 'System.Collections.Generic.List[System.Drawing.Color]'
        foreach ($pair in ($colors.GetEnumerator() | Sort-Object Value -Descending)) {
            $candidate=[Drawing.ColorTranslator]::FromHtml('#'+$pair.Key)
            $near=$false
            foreach ($pc in $palette) {
                $d=($pc.R-$candidate.R)*($pc.R-$candidate.R)+($pc.G-$candidate.G)*($pc.G-$candidate.G)+($pc.B-$candidate.B)*($pc.B-$candidate.B)
                if ($d -lt 300) { $near=$true; break }
            }
            if (-not $near) { $palette.Add($candidate) }
            if ($palette.Count -ge 8) { break }
        }
        if ($palette.Count -eq 0) { throw "Empty tile $($spec.id) $cell" }
        $opaque=0
        for ($y=0; $y -lt 24; $y++) { for ($x=0; $x -lt 24; $x++) {
            $c=$tile.GetPixel($x,$y)
            if ($c.A -gt 0) {
                $best=$palette[0]; $distance=[double]::PositiveInfinity
                foreach ($pc in $palette) {
                    $d=($pc.R-$c.R)*($pc.R-$c.R)+($pc.G-$c.G)*($pc.G-$c.G)+($pc.B-$c.B)*($pc.B-$c.B)
                    if ($d -lt $distance) { $best=$pc; $distance=$d }
                }
                $tile.SetPixel($x,$y,$best); $opaque++
            }
            $sheet.SetPixel($cx*24+$x,$cy*24+$y,$tile.GetPixel($x,$y))
        }}
        if ($cell -ge 8 -and ($opaque -eq 576 -or $opaque -lt 6)) { throw "Cutout transparency invalid: $($spec.id) $cell ($opaque opaque pixels)" }
        $stem=$spec.id.Replace('_','-')+'-'+$spec.slots[$cell]+'-24'
        $png=Join-Path $exports ($stem+'.png'); $ase=Join-Path $art ($stem+'.ase')
        $tile.Save($png,[Drawing.Imaging.ImageFormat]::Png)
        Write-Ase $ase $tile $spec.slots[$cell]
        $sg.DrawImage($tile,[Drawing.Rectangle]::new(8+$cx*166,40+$cy*170,144,144),0,0,24,24,[Drawing.GraphicsUnit]::Pixel)
        $sg.DrawString($spec.slots[$cell],$smallFont,[Drawing.Brushes]::White,(8+$cx*166),(186+$cy*170))
        $inventory += [pscustomobject]@{biome=$spec.id;category=$spec.category;slot=$spec.slots[$cell];description=$spec.cells[$cell];source="Sources/$stem.ase";png="Exported/$stem.png";width=24;height=24;opaquePixels=$opaque;colors=$palette.Count;animated=$false}
        $tile.Dispose()
    }
    $sheet.Save((Join-Path $previews ($spec.id+'-atlas-96x72.png')),[Drawing.Imaging.ImageFormat]::Png)
    $setPreview.Save((Join-Path $previews ($spec.id+'-preview.png')),[Drawing.Imaging.ImageFormat]::Png)
    $ox=($index%3)*320; $oy=[int][Math]::Floor($index/3)*312
    $og.DrawString($spec.name,$font,[Drawing.Brushes]::White,($ox+8),($oy+8))
    $og.DrawImage($sheet,[Drawing.Rectangle]::new($ox+8,$oy+40,288,216),0,0,96,72,[Drawing.GraphicsUnit]::Pixel)
    $og.DrawString('24px tiles | terrain / walls / liquid / details',$smallFont,[Drawing.Brushes]::LightGray,($ox+8),($oy+266))
    $source.Dispose(); $sheet.Dispose(); $sg.Dispose(); $setPreview.Dispose(); $index++
}
$overview.Save((Join-Path $previews 'all-biomes-overview.png'),[Drawing.Imaging.ImageFormat]::Png)
$og.Dispose(); $overview.Dispose(); $font.Dispose(); $smallFont.Dispose()
$inventory | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $Root 'manifest.json') -Encoding utf8
Write-Output "Exported $($inventory.Count) 24x24 PNGs and matching editable ASE sources for $($specs.Count) biomes."
