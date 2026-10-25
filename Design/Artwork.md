---
tags:
  - tervain
  - art
  - development
created: 2026-10-25
---

# Block artwork

Related: [[Sandbox Prototype]]

The developer's four original Aseprite documents are preserved in `tervain--shattered-realms/Art-work/`: dirt, stone, wood, and iron ore. Each is a single-frame, single-layer 24×24 RGBA tile. Runtime PNG exports live in `Art-work/Exported/`; both the editable sources and exports are versioned.

`Scripts/BlockArt.cs` loads and shares these textures across foreground blocks, background walls, dropped resources, held building materials, and hotbar icons. Terrain uses the original 24×24 pixels with nearest filtering. Pickups use 12×12 thumbnails; held materials and hotbar icons use 24×24. Exposed dirt keeps the existing grass strip. Background walls use the same art at 42% brightness and retain their separate, non-solid, unmineable behaviour. Bedrock keeps its existing protected-floor appearance.

Iron ore has a small four-block deposit at columns 55–56, rows 25–26, beside the right wall of the test cave. It requires the starter pickaxe and takes 0.8 seconds to harvest. Each block drops one iron ore block that can be collected, placed from slot **7**, and recovered. Slots 1–6 keep their existing mappings. Smelting, bars, and ore-based crafting are future work.

## Updating the art

1. Edit and save the `.ase` source in Aseprite.
2. Export it as a 24×24 PNG with the same filename into `Art-work/Exported/`, overwriting its previous export. Godot reimports the PNG automatically.
3. Commit the source, PNG, and any changed `.import` metadata together.

Alternatively, `python Tools/export_blocks.py` from the Godot project directory regenerates these four PNGs. It requires Python and Pillow (`python -m pip install Pillow`). The exporter supports single opaque normal layers, full-size raw or compressed RGBA cels, and one 24×24 frame; it rejects unsupported document structures. Use Aseprite's normal PNG export when adding layers or animation. The game itself does not require Python or Aseprite.

## Verification

The C# build passed without warnings or errors. The sandbox suite passed existing movement, mining, building, background-wall, tool damage, combat, and reset checks, plus slot-seven selection, seven-slot wheel wrapping, ore tool restrictions, harvest timing, collection, placement cost, and recovery. The standalone combat suite passed. Surface, excavated-wall, and cave/ore previews were rendered and inspected, including the expanded hotbar layout.
