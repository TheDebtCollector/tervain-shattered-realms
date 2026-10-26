---
tags: [tervain, development, handoff]
created: 2026-10-26
status: current
---

# Continue on Another Device

## Get the project and vault

The canonical GitHub repository is https://github.com/TheDebtCollector/tervain-shattered-realms, on branch `main`. The repository root is also the Obsidian vault. The local canonical checkout used for this checkpoint is `C:\Tervain_Shattered_Realms`; the chat's similarly named OneDrive scratch folder is a separate, uninitialized Git history and is not the repository to clone or push.

For a fresh checkout:

```sh
git clone https://github.com/TheDebtCollector/tervain-shattered-realms.git
cd tervain-shattered-realms
git switch main
```

For an existing clean checkout on the other device:

```sh
git switch main
git pull --ff-only
```

Open that repository folder as an existing vault in Obsidian. No cloud drive, absolute user path, or local Obsidian configuration is required to read the notes. `.obsidian/` preferences remain device-local.

## Open and run the game

1. Install **Godot 4.7.2 .NET** and a compatible .NET SDK; the C# project targets .NET 8.
2. Import `tervain--shattered-realms/project.godot` in Godot and let it import assets.
3. Build the C# project and press F5. From a terminal in the game folder, `dotnet restore` followed by `dotnet build` performs a fresh build.

The default scene is `Scenes/SandboxPlayground.tscn`. The twenty-biome authored sandbox uses 24×24 blocks and a 68px player. Use the header selector or Page Up/Page Down to travel. A/D moves, Space jumps, Shift dodges, 1/2/3 selects pickaxe/axe/sword, left mouse or J swings, Ctrl mines ahead, and selected material stacks can be placed with left/right mouse. Collected materials appear in the hotbar. R resets the unsaved scene; F2 opens the combat room, and F3 returns to a fresh sandbox.

## Current state

- All twenty biomes have authored playable regions, native art, distinct materials, walls, vegetation/formations, visual pools, and matching scenery. See [[Biome Integration]].
- The fifteen added biome sets contain 180 editable ASE/PNG pairs. Jungle/swamp and the original biome artwork are also versioned. See [[Biome Artwork]] and [[Artwork]].
- Movement/combat remain at the accepted baseline. Mining, building, pickups, passable natural vegetation, background walls, falling sand, material identity, biome travel, and reset have been verified.
- Added terrain activates nearby colliders instead of constructing physics for the entire map at startup. Terrain data and edits remain available throughout a session.
- The twenty final in-game captures and overview are in `tervain--shattered-realms/Art-work/BiomeExpansion/previews/in-game/`.

## Rules to carry forward

See [[World Generation]] for confirmed rules versus proposals:

- Terrain and biome layouts will be based on a world seed.
- Every generated world must contain plains, jungle, tundra, desert, and swamp.
- Initial spawn for the future world-creation system is always (0, 0) in plains.
- The registry must remain extensible for additional biomes and a separate future structure system.
- The fifteen new biomes are selected and integrated as test areas, but their required frequency and distribution across worlds/realms are not decided.
- Reproducibility must account for generation settings, generator version, and content definitions; the detailed contract is still a design proposal.

Seed-based world creation, realm gates, save/load, crafting, liquid simulation/hazards, and progression are still pending. The authored sandbox's existing start position is preserved; it is not the future generated-world spawn. Pool art does not imply swimming, heat, or damage rules. The biome selector is prototype navigation, not a realm-gate implementation.

Before implementing generation, settle the remaining questions in [[World Generation]]: dimensions, biome widths/repetition, the new-biome distribution, transitions, cave/water/resource rules, and the size and clearance of the starting plains region.

## Verification and artwork tools

From the game folder, build with `dotnet build`. Run the sandbox with Godot arguments `-- --sandbox-test` or `-- --biome-test`; run `Scenes/CombatPlayground.tscn` with `-- --prototype-test` for combat checks. The latest integration passed all three suites, including reset after visiting every biome. For automated headless checks, `--headless --fixed-fps 60` can precede the `--` separator. To render all twenty views, use a real rendering backend with `-- --biome-previews`.

Python with Pillow is optional for artwork tooling; it is not needed to play. `python Tools/export_blocks.py` exports the original flat art folder. `python Tools/export_biome_art.py .` exports any missing PNGs referenced by the biome registry. The biome package's `validate-biomes.py` checks all 180 new ASE/PNG pairs. Edit the authoritative ASE files in Aseprite and export matching PNGs; rerunning the sheet converter rebuilds initial generated art and can overwrite manual edits.

The [[Art Workshop]] archive retains the earlier prompts, previews, source references, and portable foundation/preview scripts that previously existed only in the chat scratch folder. Current biome generation/edit prompts are retained in the main art package.

## Git workflow

Read `AGENTS.md` and [[Version Control]] before changing the project. Commit and push coherent changes after appropriate checks. Preserve unrelated edits, use `codex/` for new branches, and keep cache/build output and credentials out of Git. Generated `.godot/`, `bin/`, `obj/`, Python caches, and local Obsidian settings are rebuilt or device-specific; their omission does not remove game source, art, or design work.
