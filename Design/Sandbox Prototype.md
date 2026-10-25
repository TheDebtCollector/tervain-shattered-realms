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

Mine materials, collect them, build a small shelter, and fight a creature in the same scene. This joins the movement/combat and sandbox foundations. Crafting comes next.

The accepted movement settings and combat timings are preserved. The main sandbox now includes the training creature, melee combat, mining, pickups, and building together. Solid terrain shields both combatants. The original combat room remains an optional isolated test scene.

## Play

Press **F5** in the Godot .NET project. The main scene is now `Scenes/SandboxPlayground.tscn`. If the editor still launches the combat room, press **F3** there to enter the sandbox, or open the sandbox scene and press **F6**.

| Input | Action |
| --- | --- |
| A / D or Left / Right | Move |
| Space, W, or Up | Jump |
| Shift | Dodge |
| J | Melee attack with any hotbar slot selected |
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
7. Approach the creature to the right of spawn. Read its wind-up, dodge with Shift, and attack with J. Try building a barrier, mining it away, fighting, and then continuing construction in the same world.

The outline is pale green when an action is allowed and red when it is blocked. The mining meter appears inside the selected block.

## Implemented rules

- Small authored terrain: 72 by 24 cells, with 32-pixel blocks, trees, surface stone, and an underground cavity.
- Dirt, stone, and wood are mineable, collectible, and placeable. Each has its own stack in the hotbar.
- Pickaxe is a permanent starter tool. Material stacks start empty.
- Dirt takes 0.28 seconds to mine, wood 0.4 seconds, and stone 0.6 seconds.
- Changing the target or hotbar slot interrupts unfinished mining.
- Reach is 160 pixels from the character's centre. Intervening solid blocks obstruct mining and placement.
- Placed blocks need an existing neighbour and cannot overlap the player or a living enemy.
- Successful placement spends one item. Rejected placement spends nothing.
- Blocks remain suspended if their support is later removed; structural collapse is outside this milestone.
- An unmineable bedrock floor and side boundaries keep the test area contained.
- The camera follows exploration above and below the initial surface.
- Mouse actions over the HUD do not modify terrain.
- One creature starts to the right of spawn. It waits until the player comes within 180 pixels, leaving room to gather first.
- J attacks with any selected slot. Holding attack or dodge takes priority over tool actions; mining/building resumes after releasing the combat key and completing the action.
- Solid terrain blocks melee damage in both directions, including newly placed blocks. Mining away a barrier opens the attack path again.
- Defeating the creature leaves the world and inventory intact. R restores terrain, inventory, player, and enemy together.
- Enemy navigation is still basic: it walks and collides with terrain, without jumping or finding routes around constructed barriers.

## Code map

| File | Responsibility |
| --- | --- |
| `Scripts/BlockWorld.cs` | Block terrain, collision, reach/visibility checks, mining, placement, target rendering |
| `Scripts/SandboxInventory.cs` | Material stack counts and spending |
| `Scripts/MaterialDrop.cs` | Dropped material attraction and collection |
| `Scripts/SandboxHud.cs` | Hotbar, counts, status, and controls |
| `Scripts/SandboxPlayground.cs` | Tool selection, combat/tool priority, integration checks, and optional scene switching |
| `Scripts/CombatRules.cs` | Shared terrain visibility check for melee damage |
| `Scenes/SandboxPlayground.tscn` | Combined world, player, training creature, and camera |

The test terrain uses one static body per block for simplicity. Chunking and procedural generation belong to later milestones; this is not the final large-world implementation.

## Verification

C# compilation passed with zero warnings and errors. Godot integration checks passed for mining input, material collection, material costs, invalid placements, visibility and reach restrictions, player overlap prevention, placed-block collision, removing collision when mining, resource recovery, different mining times, wood harvesting, secondary placement input, interrupted mining, underground camera framing, reset, and return to the combat room. Combined-scene checks also passed for notice range, live enemy placement protection, player and enemy melee obstruction, mining interruption during attacks, resource recovery after combat, enemy damage and defeat, simultaneous attack/build input, continued building after victory, and resetting both combatants. The original combat integration suite also passed. The rendered combined layout was inspected.

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
- Can you switch comfortably between mining, fighting, and building without leaving the scene?
