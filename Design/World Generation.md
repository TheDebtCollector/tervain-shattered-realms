---
tags:
  - tervain
  - game-design
  - world-generation
created: 2026-10-25
status: rules-in-discussion
---

# World Generation

Related: [[MVP Design]] · [[Sandbox Prototype]]

## Confirmed scope

- Define the rules before implementing the generation tool.
- Do not change the game project during this design phase; only the Obsidian vault may be updated.
- Generate terrain and biome layouts based on a world seed when the player creates a new world.
- The current biome set is **plains, jungle, tundra, desert, and swamp**.
- The system must support adding more biomes and future structures.
- The game uses a 2D side-view world. Generation rules must describe a horizontal surface and vertical terrain, rather than assume a top-down map.

## Proposed generation contract

These are design proposals for discussion, not implemented features or final numeric settings.

- The same seed, generation settings, generator version, and content definitions reproduce the same initial world. New-world creation supplies a fresh seed by default; explicit seed entry is still to be decided.
- Save the seed, settings, generator version, and content version with the world. Existing saved terrain and player changes persist; adding a biome or structure must not silently regenerate an existing world.
- Derive separate deterministic seed streams for terrain, biome layout, vegetation, and structures. Give each feature type a stable identifier so adding a structure does not consume random values used by terrain generation.
- Establish surface elevation and broader terrain shape, assign biome regions using climate and terrain suitability, then apply biome-specific terrain details and materials. Place vegetation and structures in later passes.
- Keep terrain shape and biome identity separate: hills, depressions, and flatter regions can occur within a biome. Oceans, mountains, and caves are not additions to the confirmed biome list.

## Initial biome direction

These terrain identities are proposals; exact climate ranges, materials, frequency, and region sizes remain open.

| Stable biome ID | Biome | Proposed terrain identity |
| --- | --- | --- |
| plains | Plains | Open terrain with gentle slopes and broad flatter areas. |
| jungle | Jungle | Warm, wet terrain with dense vegetation and varied slopes. |
| tundra | Tundra | Cold terrain with snow, frozen ground, and sparse vegetation. |
| desert | Desert | Dry terrain with sandy undulations and exposed rock. |
| swamp | Swamp | Wet, low-lying terrain with shallow depressions; standing water depends on the future water rules. |

## Extension rules proposed for the system

- Define biomes as registered data/resources with stable IDs, selection conditions, terrain parameters, surface and underground materials, vegetation rules, and references to eligible structures.
- Share terrain generation logic across biome definitions. Adding a biome should normally mean adding a definition and its assets, rather than rewriting the generation pipeline. Unique future behavior may use an optional extension hook.
- Keep biome selection rules configurable, with explicit priority/tie-breaking and a fallback when no biome matches. Define transitions independently of the number of registered biomes.
- Define structures separately from biomes, using stable IDs and placement rules for biome eligibility, surface or underground placement, slope, required footprint, ground support, rarity, spacing, and overlap restrictions.
- Place structures after the terrain they depend on exists. Reserve their footprints before final decoration and use deterministic candidate ordering to resolve conflicts.
- Permit the structure registry to be empty initially. Future structures plug into the placement stage without requiring existing biome definitions to contain structure-generation code.
- Do not promise unchanged layouts across new generator/content versions. Reproducibility requires the matching definitions and version; old saves need an explicit compatibility policy.

## Decisions still open

- World dimensions, surface height range, terrain steepness, and underground depth.
- Whether every world must contain all five biomes, their minimum widths, repetition, and frequency.
- Climate distribution, adjacency rules, and biome transition width.
- Water, caves, and resource placement rules.
- Starting biome and safe spawn requirements.
- Seed input format and handling of existing worlds after generation updates.
- First structure types and placement guarantees.
