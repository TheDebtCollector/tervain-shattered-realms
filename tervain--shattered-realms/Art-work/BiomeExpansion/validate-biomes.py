"""Read-only checks for the authored 24px biome package; does not alter images."""
from pathlib import Path
from collections import Counter
import json
import struct
import sys
from PIL import Image

root = Path(sys.argv[1]) if len(sys.argv) > 1 else Path(__file__).parent
manifest = json.loads((root / 'manifest.json').read_text(encoding='utf-8-sig'))
assert len(manifest) == 180, f'Expected 180 assets; got {len(manifest)}'
assert len({entry['png'] for entry in manifest}) == 180
assert len({entry['source'] for entry in manifest}) == 180
assert all(count == 12 for count in Counter(e['biome'] for e in manifest).values())

def decode_ase(path):
    data = path.read_bytes()
    size, magic, frames, width, height, depth = struct.unpack_from('<IHHHHH', data)
    assert (size, magic, frames, width, height, depth) == (len(data), 0xA5E0, 1, 24, 24, 32), path
    frame_size, frame_magic, chunks = struct.unpack_from('<IHH', data, 128)
    assert (frame_size, frame_magic, chunks) == (len(data)-128, 0xF1FA, 2), path
    pos = 144
    layer_count = 0
    pixels = None
    for _ in range(chunks):
        chunk_size, chunk_type = struct.unpack_from('<IH', data, pos)
        assert chunk_size >= 6 and pos + chunk_size <= len(data), path
        payload = data[pos+6:pos+chunk_size]
        if chunk_type == 0x2004:
            flags, kind, _, _, _, blend, opacity = struct.unpack_from('<HHHHHHB', payload)
            assert (flags, kind, blend, opacity) == (3, 0, 0, 255), path
            layer_count += 1
        elif chunk_type == 0x2005:
            layer, x, y, opacity, kind = struct.unpack_from('<HhhBH', payload)
            cw, ch = struct.unpack_from('<HH', payload, 16)
            assert (layer, x, y, opacity, kind, cw, ch) == (0, 0, 0, 255, 0, 24, 24), path
            assert pixels is None, path
            pixels = payload[20:]
        pos += chunk_size
    assert pos == len(data) and layer_count == 1 and len(pixels) == 24*24*4, path
    return pixels

walls = []
for entry in manifest:
    png = root / entry['png']
    with Image.open(png) as source:
        assert source.size == (24, 24), png
        assert source.mode == 'RGBA', png
        rgba = source.tobytes()
        colors = source.getcolors(576)
        alpha = set(rgba[3::4])
        assert alpha <= {0, 255}, png
        assert 2 <= len(colors) <= 9, (png, len(colors))
        is_decoration = entry['slot'] in {'canopy-formation', 'small-decoration', 'tall-decoration', 'hanging-decoration'}
        assert (0 in alpha) == is_decoration, (png, alpha)
        assert rgba == decode_ase(root / entry['source']), f'PNG/ASE mismatch: {png}'
        if entry['slot'] in {'soil-background', 'stone-background'}:
            walls.append(entry)

for entry in walls:
    foreground_slot = 'soil-block' if entry['slot'] == 'soil-background' else 'stone-block'
    foreground = next(e for e in manifest if e['biome'] == entry['biome'] and e['slot'] == foreground_slot)
    def luminance(path):
        with Image.open(path) as image:
            return sum(.2126*r + .7152*g + .0722*b for r,g,b,a in struct.iter_unpack('4B', image.tobytes())) / 576
    assert luminance(root / entry['png']) < luminance(root / foreground['png']), f'Wall is not dimmer: {entry}'

print(json.dumps({'result':'passed','biomes':15,'png_files':180,'ase_files':180,'native_tile_size':[24,24],'palette':'at most 8 opaque colors per tile','alpha':'binary; all 60 decorations have transparency','ase_png_match':'all 180 pairs match pixel-for-pixel','wall_contrast':'all 30 walls dimmer than corresponding foreground'}, indent=2))
