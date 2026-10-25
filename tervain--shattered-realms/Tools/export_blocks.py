"""Export the project's single-layer RGBA Aseprite tiles without changing their sources.

Run with Python and Pillow: python Tools/export_blocks.py
Unsupported document structures fail rather than silently dropping artwork.
"""
from pathlib import Path
import struct
import zlib
from PIL import Image


def export(source: Path, destination: Path):
    data = source.read_bytes()
    size, magic, frames, width, height, depth = struct.unpack_from('<IHHHHH', data)
    if (size, magic, frames, width, height, depth) != (len(data), 0xA5E0, 1, 24, 24, 32):
        raise ValueError(f'{source.name}: expected a single 24x24 RGBA frame')
    frame_size, frame_magic, chunks = struct.unpack_from('<IHH', data, 128)
    if frame_magic != 0xF1FA or frame_size + 128 != len(data):
        raise ValueError(f'{source.name}: invalid frame')
    position = 144
    layers, cels = [], []
    for _ in range(chunks):
        chunk_size, kind = struct.unpack_from('<IH', data, position)
        payload = data[position + 6:position + chunk_size]
        if chunk_size < 6 or len(payload) != chunk_size - 6:
            raise ValueError(f'{source.name}: invalid chunk')
        if kind == 0x2004:
            flags, layer_type, _, _, _, blend, opacity = struct.unpack_from('<HHHHHHB', payload)
            layers.append((flags, layer_type, blend, opacity))
        if kind == 0x2005:
            layer, x, y, opacity, cel_type = struct.unpack_from('<HhhBH', payload)
            if cel_type not in (0, 2):
                raise ValueError(f'{source.name}: unsupported cel type {cel_type}')
            cel_width, cel_height = struct.unpack_from('<HH', payload, 16)
            pixels = payload[20:] if cel_type == 0 else zlib.decompress(payload[20:])
            cels.append((layer, x, y, opacity, cel_width, cel_height, pixels))
        position += chunk_size
    if layers != [(3, 0, 0, 255)] or len(cels) != 1:
        raise ValueError(f'{source.name}: expected one visible, opaque, normal layer')
    layer, x, y, opacity, cel_width, cel_height, pixels = cels[0]
    if (layer, x, y, opacity, cel_width, cel_height) != (0, 0, 0, 255, width, height):
        raise ValueError(f'{source.name}: expected a full-size opaque cel')
    image = Image.frombytes('RGBA', (width, height), pixels)
    destination.mkdir(parents=True, exist_ok=True)
    image.save(destination / (source.stem + '.png'))
    return image


if __name__ == '__main__':
    import sys
    project = Path(sys.argv[1]) if len(sys.argv) > 1 else Path(__file__).resolve().parents[1]
    art = project / 'Art-work'
    for source in sorted(art.glob('*.ase')):
        export(source, art / 'Exported')
        print(f'Exported {source.name}')
