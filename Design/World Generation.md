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
- The five existing biomes are **plains, jungle, tundra, desert, and swamp**. Fifteen additional biomes are selected for the design roster below. Their environmental artwork is authored (see [[Biome Artwork]]); game integration is pending.
- Every generated world must contain all five current biomes. The seed determines their sizes, positions, and terrain shapes; minimum region widths remain to be defined.
- Initial player spawn is always at world coordinates **(0, 0)** and always within the **plains** biome, for every seed. Terrain and biome generation must accommodate this fixed spawn rather than move the spawn to a different location.
- The system must support adding more biomes and future structures.
- The game uses a 2D side-view world. Generation rules must describe a horizontal surface and vertical terrain, rather than assume a top-down map.

## Proposed generation contract

These are design proposals for discussion, not implemented features or final numeric settings.

- The same seed, generation settings, generator version, and content definitions reproduce the same initial world. New-world creation supplies a fresh seed by default; explicit seed entry is still to be decided.
- Save the seed, settings, generator version, and content version with the world. Existing saved terrain and player changes persist; adding a biome or structure must not silently regenerate an existing world.
- Derive separate deterministic seed streams for terrain, biome layout, vegetation, and structures. Give each feature type a stable identifier so adding a structure does not consume random values used by terrain generation.
- Establish surface elevation and broader terrain shape, assign biome regions using climate and terrain suitability, then apply biome-specific terrain details and materials. Place vegetation and structures in later passes.
- Keep terrain shape and biome identity separate: hills, depressions, and flatter regions can occur within a biome. Generic hills, cliffs, and caves remain reusable terrain features. The selected mountain and underground biomes add specific environmental identities; oceans are not currently selected.

## Initial biome direction

These terrain identities are proposals; exact climate ranges, materials, frequency, and region sizes remain open.

| Stable biome ID | Biome | Proposed terrain identity |
| --- | --- | --- |
| plains | Plains | Open terrain with gentle slopes and broad flatter areas. |
| jungle | Jungle | Warm, wet terrain with dense vegetation and varied slopes. |
| tundra | Tundra | Cold terrain with snow, frozen ground, and sparse vegetation. |
| desert | Desert | Dry terrain with sandy undulations and exposed rock. |
| swamp | Swamp | Wet, low-lying terrain with shallow depressions; standing water depends on the future water rules. |

## Selected biome additions

The developer requested five selections in each of the three previously discussed categories: natural surface, underground, and fantasy. These fifteen selections expand the design roster to twenty biomes including the five existing biomes. The descriptions below establish the intended distinctions; exact assets, mechanics, hazards, and numerical parameters remain design work.

### Natural surface additions

| Stable biome ID | Biome | Terrain and exploration identity |
| --- | --- | --- |
| temperate_forest | Temperate forest | Broadleaf woodland, rolling ground, and clearings; establishes a wooded counterpart to open plains. |
| taiga | Pine forest / taiga | Dense conifers and rocky, cold ground; distinct from tundra through its substantial tree cover. |
| badlands | Badlands / mesa | Layered red rock, flat-topped plateaus, and deep ravines; distinct from the desert's sandy terrain. |
| alpine | Mountains / alpine | Tall peaks, steep slopes, exposed stone, and high-altitude vegetation; emphasizes vertical travel. |
| volcanic_wasteland | Volcanic wasteland | Basalt ridges, ash fields, and volcanic vents; potential surface lava rules remain to be defined. |

### Underground additions

| Stable biome ID | Biome | Terrain and exploration identity |
| --- | --- | --- |
| limestone_caverns | Limestone caverns | Broad chambers, branching passages, stalactites, and stalagmites; a recognizable natural underground baseline. |
| crystal_caverns | Crystal caverns | Jagged crystal formations and mineral-lined chambers; creates a distinct mineral-focused underground environment. |
| fungal_caves | Fungal caves | Giant fungi, glowing growth, and organic clusters; a living underground ecosystem with a different silhouette from crystal caves. |
| frozen_caves | Frozen caves | Ice-lined passages, frozen chambers, and crevasses; exact ice and movement mechanics remain undecided. |
| magma_caverns | Magma caverns | Volcanic chambers and magma channels; underground counterpart to volcanic terrain, with depth and heat rules still open. |

### Fantasy additions

| Stable biome ID | Biome | Terrain and exploration identity |
| --- | --- | --- |
| shattered_islands | Shattered islands | Floating land fragments, broken shelves, and large gaps; directly expresses the Shattered Realms identity through vertical exploration. |
| enchanted_forest | Enchanted forest | Luminous trees, magical plants, and unusual growth patterns; establishes a magical woodland distinct from ordinary forest and jungle. |
| corrupted_wilderness | Corrupted wilderness | Twisted trees, infected soil, and hostile-looking growth; any spreading corruption mechanic requires a separate decision. |
| living_biome | Flesh / living biome | Organic ground, tissue-like formations, and pulsing growth; supports the game's increasingly hellish visual direction. |
| voidlands | Voidlands | Isolated ground fragments, deep darkness, and fractured terrain; contrasts with shattered islands through its sparse, hostile void identity. |

### Generation implications

- The original guarantee remains: every generated world contains plains, jungle, tundra, desert, and swamp, with initial spawn at (0, 0) in plains.
- Whether all fifteen additions must also appear in every world, or are distributed among realms, remains undecided. Selecting them does not impose a twenty-biome minimum on each generated map.
- Surface, underground, and fantasy are design categories. Underground biomes need depth and host-terrain rules; fantasy biomes may occupy surface, underground, or separate realms once placement is defined.
- Selection must support explicit eligibility and placement constraints alongside climate, so underground and fantasy biomes are not forced into temperature/moisture classification alone.
- Keep structure definitions independent of the twenty-biome roster. Ruins, villages, temples, and dungeons remain structures that may be eligible in multiple biomes.

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
- Minimum biome widths, repetition, and frequency, while guaranteeing all five current biomes in every world.
- Placement and frequency of the fifteen new biomes, including which belong in the starting world versus other realms and whether any are guaranteed.
- Climate distribution, adjacency rules, biome transition width, and underground depth/host-terrain rules.
- Water, caves, and resource placement rules.
- Starting plains region size, spawn clearance and ground support, and any additional safe spawn requirements. Initial spawn coordinates and biome are fixed at (0, 0) in plains; respawn behavior remains undecided.
- Seed input format and handling of existing worlds after generation updates.
- First structure types and placement guarantees.
