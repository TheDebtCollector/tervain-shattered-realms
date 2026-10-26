---
tags:
  - tervain
  - biomes
  - development
created: 2026-10-25
status: implemented-and-verified
---

# Biome Integration

Related: [[World Generation]] · [[Biome Artwork]] · [[Sandbox Prototype]]

## Play

Run the sandbox with F5. All twenty biomes are now playable. Use the biome selector in the header, Page Down for the next biome, or Page Up for the previous biome. Walking also connects the regions. Travel keeps inventory, harvested terrain, and construction in the current session. R resets the unsaved sandbox.

The existing plains, desert, and tundra region stays at columns 0–127, with its accepted movement, combat, harvesting, and construction behavior. Jungle, swamp, and the fifteen new biomes extend the authored test trail to 944×32 blocks. Each additional region is 48 blocks wide; a block is now exactly 16×16 pixels, with a 20×40 player (updated 2026-10-26).

This is an authored sandbox integration for reviewing and playing the artwork. It does not implement seed-based world creation, realm gates, save/load, or the future world-creation spawn at (0, 0). Those rules remain in [[World Generation]]; the existing sandbox start is preserved.

## Content and extension

`Data/biomes.json` defines the twenty biome IDs, display names, categories, palettes, terrain profiles, material IDs, wall textures, decoration textures, and liquid visuals. `Scripts/BiomeCatalog.cs` loads these definitions. New biomes can append definitions and unique material IDs without expanding the block or equipment enums. Map width follows the number of definitions.

The original three biome regions retain their materials and shortcuts. Jungle/swamp and the fifteen additions introduce 51 distinct material stacks. Each new biome has soil/sediment, stone, and a third material such as wood, obsidian, flowstone, ice, bone, or organic fiber. Surface caps and buried soil share the same inventory material; caps are drawn only on exposed ground and not beneath liquid.

Pickaxes harvest terrain and minerals; axes harvest wood. New blocks retain their own artwork through mining, pickups, inventory selection, and placement in another biome. Natural trunks are passable vegetation, while placed wood is solid construction. Generated background walls remain after harvesting and support building without adding collision.

The hotbar shows the three tools, collected materials, and any explicitly selected empty stack. Wheel navigation skips empty material stacks; the existing numbered shortcuts still select their original items. Hovering a material shows its full name when the compact label is shortened.

Structure eligibility can refer to stable biome IDs in a separate future structure registry; this integration adds no structures or structure-generation rules.

## Visual polish

- All terrain and decorations use nearest filtering with the active native 16px Pixel16 source tiles and integer-sized decoration scaling.
- New surface regions have shaped ground, material outcrops, vegetation/formations, and recessed pools. Cave regions have irregular ceilings, darker walls, hanging formations, and pools below the cave floor.
- Shattered islands and voidlands include floating terrain fragments and distant island silhouettes.
- Background colors blend near biome borders; distant silhouettes stay subdued so the player and foreground are readable.
- Trees use layered pixel-art crowns, including the original plains trees. Canopies remain dim scenery after their original trunks are harvested.
- The frozen-cave sheet was revised with the built-in image tool after rendered inspection revealed horizontal repeat lines. Updated PNGs and editable ASEs remain matched; the correction prompt is retained in the art package.
- Decorations are anchored to existing solid terrain and hide when construction covers their footprint. Water/lava/ichor are static visual pools; flow, swimming, temperature, and liquid damage remain undecided gameplay work.

The original bushes and shrubs retain their weapon-hit behavior. Newly added flora/formations are scenery, with materials harvested from their corresponding terrain blocks and outcrops.

## Performance and lifecycle

Drawing is culled to the camera area. Original terrain colliders remain active; added terrain activates collision within forty columns of the player and around moving sand. Terrain data, walls, and edits stay loaded when distant collision objects are released. Nearby colliders activate before travel completes. The verified twenty-biome tour stays below 5,000 simultaneous terrain colliders.

Terrain collision shapes and hotbar styles are shared. Temporary native resource owners are explicitly released, and the cross-scene regression check allows deferred teardown to complete before exiting. This avoids allocating an entire expanded map's physics objects at startup and resolves the rapid-reset cleanup failure found during integration.

## Verification

- C# build: zero warnings and zero errors.
- Full sandbox suite: passed, including movement/jump clearance, harvesting, placement, pickups, walls, equipment, combat cover, desert/sand, tundra, Ctrl mining, vegetation, reset, and switching to the combat room; clean process exit.
- Standalone combat prototype suite: passed.
- Biome suite: passed across all twenty regions, including 51 material stacks, native art loading, tool restrictions, safe grounded arrival, mining/wall preservation, placement identity, pools, collider bounds, collected-material selection, Page Up/Down, the actual selector signal, and reset after visiting every biome.
- All 180 new PNG/ASE pairs pass the art package validator at 24×24 with binary alpha and at most eight opaque colors.
- All twenty in-game captures were rendered and visually reviewed. Captures and a contact sheet are in `tervain--shattered-realms/Art-work/BiomeExpansion/previews/in-game/`.

Recheck with `-- --biome-test`, `-- --sandbox-test`, and the combat scene's `-- --prototype-test`. Render all biome views with `-- --biome-previews` using a real rendering backend.

![[tervain--shattered-realms/Art-work/BiomeExpansion/previews/in-game/all-twenty-biomes-in-game.png]]
