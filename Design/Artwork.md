---
tags:
  - tervain
  - art
  - development
created: 2026-10-25
---

# Block artwork

Related: [[Sandbox Prototype]]

The developer's twenty-one integrated Aseprite documents are preserved in `tervain--shattered-realms/Art-work/`: the original dirt/stone/wood/ore tiles and dirt/cave walls, plus seven desert assets listed in [[Desert Prototype]] and eight winter assets listed in [[Tundra Prototype]]. Each is a single-frame, single-layer 24×24 RGBA tile. Runtime PNG exports live in `Art-work/Exported/`; both the editable sources and exports are versioned.

`Scripts/BlockArt.cs` loads and shares these textures across foreground blocks, background walls, dropped resources, held building materials, and hotbar icons. Terrain uses the original 24×24 pixels with nearest filtering. Pickups use 12×12 thumbnails; held materials and hotbar icons use 24×24. Exposed dirt keeps the existing grass strip. Dirt walls use `dirt-background-24.png`; stone, iron ore, and bedrock walls use `cave-background-24.png`. These dedicated background tiles are drawn at their authored brightness. Tree walls use the wood tile at 42% brightness. All background walls retain their separate, non-solid, unmineable behaviour. Harvested trees also keep a dim canopy, including after their top trunk segment is removed. Bedrock keeps its existing protected-floor appearance.

Iron ore has a small four-block deposit at columns 55–56, rows 25–26, beside the right wall of the test cave. It requires the starter pickaxe and takes 0.8 seconds to harvest. Each block drops one iron ore block that can be collected, placed from slot **7**, and recovered. Slots 1–6 keep their existing mappings. Smelting, bars, and ore-based crafting are future work.

## Updating the art

1. Edit and save the `.ase` source in Aseprite.
2. Export it as a 24×24 PNG with the same filename into `Art-work/Exported/`, overwriting its previous export. Godot reimports the PNG automatically.
3. Commit the source, PNG, and any changed `.import` metadata together.

Alternatively, `python Tools/export_blocks.py` from the Godot project directory regenerates PNGs for the `.ase` files in `Art-work/`. It requires Python and Pillow (`python -m pip install Pillow`). The exporter supports single opaque normal layers, full-size raw or compressed RGBA cels, and one 24×24 frame; it rejects unsupported document structures. Use Aseprite's normal PNG export when adding layers or animation. The game itself does not require Python or Aseprite.

## Verification

The C# build passed without warnings or errors. The sandbox suite passed existing movement, mining, building, background-wall, tool damage, combat, and reset checks, plus slot-seven selection, seven-slot wheel wrapping, ore tool restrictions, harvest timing, collection, placement cost, and recovery. The standalone combat suite passed. Surface, excavated-wall, and cave/ore previews were rendered and inspected, including the expanded hotbar layout.


## Dedicated wall art and tree backgrounds — 2026-10-25

The new dirt and cave background sources and PNG exports are versioned alongside the original block artwork. Backgrounds remain limited to generated solid cells; original sky and cave air remain without block walls. Mining ore reveals a plain cave wall, so the harvested ore does not remain visible in the background.

Tree trunks already had persistent wooden wall cells; tree canopy rendering now follows those wall cells too. Harvesting the entire trunk leaves a dim background tree. Its wall cells can support foreground construction and cannot be mined for additional wood. New player builds do not create extra background cells.

The build and sandbox checks passed, including full-tree harvesting, preserved wall cells, no repeated harvest, no wall collision, and placing/removing a wall-supported block on the tree. Dirt/cave wall and fully harvested tree previews were rendered and inspected.


## Desert art — 2026-10-25

The desert now uses all seven desert assets: sand and sandstone foreground/background tiles, cracked earth, cactus, and dry bush. Sand uses its sand background; sandstone and cracked earth use the sandstone background. Cacti and bushes leave no background-wall silhouettes. See [[Desert Prototype]] for gathering, bush hit behaviour, falling sand, and controls. The snow and tundra assets are now integrated as described below.


## Snow and tundra art — 2026-10-25

All eight winter sources are integrated: snow and ice foreground/background tiles, frozen earth, tundra wood, snow pine, and tundra shrub. Snow/frozen earth use the snow wall tile, ice uses the ice wall tile, and pine trunks leave dim tundra-wood walls. Pine sprites use their visible 12×17-pixel region at six-times nearest scale. Shrubs use their native 24×24 tile. See [[Tundra Prototype]] for location, gathering rules, and validation.

## Biome expansion art — 2026-10-25

The fifteen new biomes now have a separate environmental art package with 180 native 24×24 tiles, matching editable Aseprite sources, and previews. See [[Biome Artwork]] for the catalog, validation, and editing workflow. Integration into the game is pending.

## Biome integration follow-up — 2026-10-25

All twenty biomes now have playable authored regions, dedicated materials and walls, vegetation/formations, pools, and matching scenery. The frozen-cave sheet was corrected after in-game inspection exposed repeat lines. See [[Biome Integration]] for controls, current limits, and validation.


## Retained workshop resources

[[Art Workshop]] preserves the earlier biome prompts, native exports, enlarged previews, source references, and portable foundation/preview scripts. The workshop guide distinguishes historical exports from the authoritative game art.

## Simplified native 16px artwork — 2026-10-26

The active environmental art now lives in `tervain--shattered-realms/Art-work/Pixel16/`: 220 simplified 16x16 base assets, editable ASE sources, matching PNGs, and three additional variants for repeating terrain, background walls, and liquids. Palettes and silhouettes preserve biome identity with much less detail. Material edges share base colors; exposed surface caps join horizontally. A coordinate hash chooses variants consistently so neighboring blocks avoid a uniform repeated stamp without flickering when the camera moves. Icons use the base tile.

The original 24px packages and Art Workshop are retained as historical sources. Edit/export the new Pixel16 package for current gameplay. The simulation grid remains 24 world units, preserving established movement, collision, combat, and biome positions. Pixel dimensions describe source art, not physics. Native art uses nearest texture filtering.
Verification: native art validation passed for all 676 PNG/ASE pairs (220 base assets and 152 four-variant sets). The C# build passed with no warnings/errors; biome, sandbox, and combat regression suites passed. Twenty updated biome captures and a contact sheet are saved under Pixel16/previews/.
