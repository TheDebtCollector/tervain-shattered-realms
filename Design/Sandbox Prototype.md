---
tags:
  - tervain
  - development
  - prototype
created: 2026-10-25
status: ready-for-playtest
---

# Sandbox Prototype

Related: [[MVP Design]] · [[Combat Prototype]]

## Goal

Mine materials, collect them, and use them to build a small shelter. This is the first part of the sandbox foundation milestone. Crafting comes next.

The accepted Player movement and combat script is unchanged. The sandbox uses a separate scene, leaving the original combat room available.

## Play

Press **F5** in the Godot .NET project. The main scene is now `Scenes/SandboxPlayground.tscn`. If the editor still launches the combat room, press **F3** there to enter the sandbox, or open the sandbox scene and press **F6**.

| Input | Action |
| --- | --- |
| A / D or Left / Right | Move |
| Space, W, or Up | Jump |
| Shift | Dodge |
| J | Melee attack |
| 1 | Select pickaxe |
| 2 / 3 / 4 | Select dirt / stone / wood |
| Mouse wheel or hotbar buttons | Change selection |
| Hold left mouse with pickaxe | Mine the targeted block |
| Hold left or right mouse with material selected | Place the selected block |
| F2 | Enter the combat playground |
| F3 in the combat playground | Return to a fresh sandbox |
| R | Reset the current scene |

**Progress is not saved yet.** Resetting, leaving the sandbox, or closing the game discards terrain changes and inventory.

## First playtest

1. Select the pickaxe and hold left mouse on a nearby dirt block.
2. Move near the dropped item to collect it; nearby drops are drawn toward the character.
3. Select dirt and place it on the ground nearby.
4. Harvest tree trunks for wood. Stone is available underground and in the surface outcrop to the right of spawn.
5. Build walls and a roof. Leave a two-block-high entrance so the character can enter.
6. Try jumping onto your construction, mining underfoot, and recovering placed blocks.

The outline is pale green when an action is allowed and red when it is blocked. The mining meter appears inside the selected block.

## Implemented rules

- Small authored terrain: 72 by 24 cells, with 32-pixel blocks, trees, surface stone, and an underground cavity.
- Dirt, stone, and wood are mineable, collectible, and placeable. Each has its own stack in the hotbar.
- Pickaxe is a permanent starter tool. Material stacks start empty.
- Dirt takes 0.28 seconds to mine, wood 0.4 seconds, and stone 0.6 seconds.
- Changing the target or hotbar slot interrupts unfinished mining.
- Reach is 160 pixels from the character's centre. Intervening solid blocks obstruct mining and placement.
- Placed blocks need an existing neighbour and cannot overlap the character.
- Successful placement spends one item. Rejected placement spends nothing.
- Blocks remain suspended if their support is later removed; structural collapse is outside this milestone.
- An unmineable bedrock floor and side boundaries keep the test area contained.
- The camera follows exploration above and below the initial surface.
- Mouse actions over the HUD do not modify terrain.
- No enemies spawn in the sandbox; use the combat room to test combat.

## Code map

| File | Responsibility |
| --- | --- |
| `Scripts/BlockWorld.cs` | Block terrain, collision, reach/visibility checks, mining, placement, target rendering |
| `Scripts/SandboxInventory.cs` | Material stack counts and spending |
| `Scripts/MaterialDrop.cs` | Dropped material attraction and collection |
| `Scripts/SandboxHud.cs` | Hotbar, counts, status, and controls |
| `Scripts/SandboxPlayground.cs` | Tool selection, interaction timing, scene switching, and integration checks |
| `Scenes/SandboxPlayground.tscn` | Sandbox scene, unchanged Player, and camera |

The test terrain uses one static body per block for simplicity. Chunking and procedural generation belong to later milestones; this is not the final large-world implementation.

## Verification

C# compilation passed with zero warnings and errors. Godot integration checks passed for mining input, material collection, material costs, invalid placements, visibility and reach restrictions, player overlap prevention, placed-block collision, removing collision when mining, resource recovery, different mining times, wood harvesting, secondary placement input, interrupted mining, underground camera framing, reset, and return to the combat room. The original combat integration suite also passed. The rendered sandbox layout was inspected.

Run the sandbox checks with:

```powershell
& '<path-to-Godot-.NET.exe>' --headless --path 'C:\Tervain_Shattered_Realms\tervain--shattered-realms' -- --sandbox-test
```

Run the original combat checks with:

```powershell
& '<path-to-Godot-.NET.exe>' --headless --path 'C:\Tervain_Shattered_Realms\tervain--shattered-realms' 'res://Scenes/CombatPlayground.tscn' -- --prototype-test
```

## Not included yet

Crafting, a separate inventory window, background walls, doors, furniture, automatic shelter detection, save/load, generated terrain, realm travel, audio, and final art.

## Feedback to gather

- Do mining times feel satisfying?
- Is selecting and placing blocks comfortable?
- Are targeting, reach, and blocked placement understandable?
- Can you comfortably construct and enter a small shelter?
- Does movement still feel as good as the accepted baseline?
