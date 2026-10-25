---
tags:
  - tervain
  - art
  - biomes
created: 2026-10-25
status: integrated-in-sandbox
---

# Biome Artwork

Related: [[World Generation]] · [[Artwork]]

## Completed environmental package

The fifteen selected biome additions now have 180 static environmental assets: twelve 24×24 RGBA tiles per biome, each supplied as an editable single-layer, single-frame `.ase` and matching PNG. The package is in `tervain--shattered-realms/Art-work/BiomeExpansion/`.

The existing dirt, desert, tundra, jungle, swamp, and water artwork was inspected first. New art follows its coarse pixel clusters, compact material palettes, darker background walls, transparent decorations, and nearest-neighbor presentation. Generated sheets were sampled to the exact 24-pixel block grid and limited to at most eight opaque colors per tile.

| Surface | Underground | Fantasy |
| --- | --- | --- |
| Temperate forest | Limestone caverns | Shattered islands |
| Pine forest / taiga | Crystal caverns | Enchanted forest |
| Badlands / mesa | Fungal caves | Corrupted wilderness |
| Mountains / alpine | Frozen caves | Flesh / living biome |
| Volcanic wasteland | Magma caverns | Voidlands |

## Assets per biome

- One surface block, one soil/sediment block, one stone block, and one wood/special material block.
- Two recessed background wall textures, corresponding to soil and stone.
- One liquid body tile and one liquid surface tile. Liquids are visual candidates; their presence, physics, and hazards are not decided by the artwork.
- Four transparent decorations: a canopy/formation cluster, a small decoration, a tall decoration, and a hanging decoration.

`manifest.json` provides stable biome IDs, individual subjects, file paths, tile roles, dimensions, palette counts, and transparency coverage. `README.md` explains editing and export. Exact prompts and all fifteen original generated sheets are retained inside the package.

## Preview

![[tervain--shattered-realms/Art-work/BiomeExpansion/previews/all-biomes-overview.png]]

Each biome also has a labeled enlarged preview and a native 96×72 atlas in the `previews/` directory. Preview and reference directories are excluded from Godot import with `.gdignore`.

## Verification and limits

- All 180 PNGs are exactly 24×24 RGBA with at most eight opaque colors per tile and binary alpha.
- All 60 decorations have transparent backgrounds.
- All 180 Aseprite documents contain one editable layer/frame and match their PNG pixel data exactly.
- All 30 wall tiles are darker than their matching foreground materials.
- The native-size exports and enlarged previews were visually inspected across all fifteen sets.

The environmental artwork is now integrated into the authored sandbox; see [[Biome Integration]]. Seed-based world creation and realm progression remain pending. These are environmental tile sets; structures, enemies, equipment, UI, full scenery backgrounds, and animations need their own requirements. Biome distribution, generation guarantees, and the fixed plains spawn at (0, 0) remain as documented in [[World Generation]].
