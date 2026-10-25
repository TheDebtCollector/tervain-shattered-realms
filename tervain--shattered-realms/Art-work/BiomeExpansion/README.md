# Biome expansion artwork

180 environmental assets for the fifteen new biomes: twelve native 24×24 RGBA tiles per biome. All tiles have editable single-frame, single-layer Aseprite sources and matching PNG exports.

Scope: environmental tiles, walls, liquids, and flora/formations. No game code or generation integration. Existing assets are style references and must remain untouched.

## Contents

- `Sources/`: 180 editable `.ase` files.
- `Exported/`: 180 native-size `.png` files for future integration.
- `previews/`: fifteen labeled set previews, fifteen native 96×72 atlases, and a complete overview. Preview enlargement uses nearest-neighbor filtering.
- `References/`: the fifteen original generated sheets, retained as provenance and to make the export reproducible.
- `manifest.json`: biome ID, category, tile role, description, dimensions, palette count, transparency coverage, and relative source/export paths for each asset.
- `specs.json`: biome palettes and the twelve requested assets per set.
- `prompts.json`: exact per-biome prompts used with the built-in image generation tool. The existing desert, tundra, jungle, swamp, and water preview sheets were supplied as style references.
- `sources.json`: relative paths to the original generated sheets.

## Tile roles

Each set contains a surface block, a soil/sediment block, a stone block, a wood or special material block, two darker background walls, liquid body and surface tiles, and four transparent decorations (a canopy/formation cluster, a small decoration, a tall decoration, and a hanging decoration). Descriptions in the manifest identify the actual subject: for example, flowstone in limestone caverns, obsidian in volcanic wasteland, and organic fibers in the living biome.

Block dimensions are exactly 24×24. All exports use at most eight opaque colors per tile and binary alpha to keep pixel edges crisp. Use nearest filtering and integer display scales. Upright decorations sit near the bottom of their tile; hanging decorations attach at the top. These are static assets, matching the current art workflow; liquids and luminous materials have no animation or lighting behavior yet.

The liquid colors and fantasy motifs are art directions. They do not introduce gameplay mechanics, generation guarantees, or rules for liquids, corruption, heat, or magical effects. Future structures, enemies, equipment, and interface art require their own designs.

## Editing and exporting

Edit a `.ase` in Aseprite and export its PNG under the same stem in `Exported/`. Do not rerun the sheet converter over manually edited sources: it rebuilds all generated tiles from the retained sheets. The existing project's `Tools/export_blocks.py` scans only the original flat art folder, so use Aseprite export for this separate package.

To reproduce the initial generated package on Windows with PowerShell and System.Drawing, run `./export-biomes.ps1` from this directory. It samples the regular sheets down to the native grid, limits palettes, normalizes alpha, extends transparent gaps in otherwise opaque material cells, and writes matching PNG/ASE pairs. The first forest sheet needs a higher alpha cutoff to remove its faint matte; other sheets use the standard binary cutoff. Narrow transparent side clearances prevent cell-edge bleed in decorations.

Run `python validate-biomes.py` with Pillow installed to verify all 180 pairs. Validation reads the assets without changing them and checks dimensions, frame/layer layout, palette limits, alpha, pixel equality between PNG and ASE, and foreground/background brightness.

No existing artwork, scripts, scenes, world generation, or game behavior is modified by this package.
