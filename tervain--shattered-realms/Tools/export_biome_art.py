from pathlib import Path
import importlib.util
import json
import sys

project = Path(sys.argv[1])
definitions = json.loads((project / 'Data/biomes.json').read_text(encoding='utf-8-sig'))
spec = importlib.util.spec_from_file_location('export_blocks', project / 'Tools/export_blocks.py')
exporter = importlib.util.module_from_spec(spec)
spec.loader.exec_module(exporter)
paths = set()
for biome in definitions:
    paths.update(biome['decorations'])
    paths.update([biome['liquid'], biome['liquidSurface']])
    for material in biome['materials']:
        paths.update(material[k] for k in ('texture', 'wall', 'surface') if material[k])
for resource in sorted(paths):
    destination = project / resource.removeprefix('res://')
    if destination.exists():
        continue
    source = project / 'Art-work' / (destination.stem + '.ase')
    if not source.exists():
        raise FileNotFoundError(f'Missing both PNG and source for {resource}')
    exporter.export(source, destination.parent)
    print(f'Exported {source.name}')
print(f'All {len(paths)} referenced biome textures are present.')
