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

Run speed, dodge settings, and combat timings are preserved. Jump height and player size use the newer tuning described below. The main sandbox now includes the training creature, melee combat, mining, pickups, and building together. Solid terrain shields both combatants. The original combat room remains an optional isolated test scene.

## Play

Press **F5** in the Godot .NET project. The main scene is now `Scenes/SandboxPlayground.tscn`. If the editor still launches the combat room, press **F3** there to enter the sandbox, or open the sandbox scene and press **F6**.

| Input | Action |
| --- | --- |
| A / D or Left / Right | Move |
| Space, W, or Up | Jump |
| Shift | Dodge |
| J | Swing the sword when it is equipped |
| 1 | Select pickaxe |
| 2 | Select axe |
| 3 | Select sword |
| 4 / 5 / 6 | Select dirt / stone / wood |
| Mouse wheel or hotbar buttons | Change selection |
| Hold left mouse with pickaxe | Mine dirt or stone |
| Hold left mouse with axe | Harvest wood |
| Left mouse with sword | Melee attack |
| Hold left or right mouse with material selected | Place the selected block |
| F2 | Enter the combat playground |
| F3 in the combat playground | Return to a fresh sandbox |
| R | Reset the current scene |

**Progress is not saved yet.** Resetting, leaving the sandbox, or closing the game discards terrain changes and inventory.

## First playtest

1. Select the pickaxe and hold left mouse on a nearby dirt block.
2. Move near the dropped item to collect it; nearby drops are drawn toward the character.
3. Select dirt and place it on the ground nearby.
4. Select the axe (2) to harvest tree trunks for wood. Stone is available underground and in the surface outcrop to the right of spawn.
5. Build walls and a roof. The player is three blocks tall; leave a four-block-high entrance for comfortable headroom.
6. Try jumping onto your construction, mining underfoot, and recovering placed blocks.
7. Approach the creature to the right of spawn. Read its wind-up, dodge with Shift, select the sword (3), and attack with left click or J. Try building a barrier, mining it away, fighting, and then continuing construction in the same world.

The outline is pale green when an action is allowed and red when it is blocked. The mining meter appears inside the selected block.

## Implemented rules

- Small authored terrain: 96 by 32 cells, with 24-pixel blocks, trees, surface stone, and an underground cavity. Physical world dimensions remain 2304 by 768 pixels.
- Dirt, stone, and wood are mineable, collectible, and placeable. Each has its own stack in the hotbar.
- Pickaxe, axe, and sword are permanent starter equipment. The pickaxe is selected on spawn. Material stacks start empty.
- Dirt takes 0.28 seconds to mine, wood 0.4 seconds, and stone 0.6 seconds.
- Changing the target or hotbar slot interrupts unfinished mining.
- Mining reaches 192 pixels (8 blocks) from the player body centre and ignores intervening blocks. The correct equipped tool is also required: pickaxe for dirt/stone and axe for wood. Bedrock and background walls remain unmineable. Building reaches 160 pixels (about 6.7 blocks) and still requires a clear path.
- Placed blocks need an adjacent foreground block or a background wall in the target cell. They cannot overlap the player or a living enemy.
- Successful placement spends one item. Rejected placement spends nothing.
- Blocks remain suspended if their support is later removed; structural collapse is outside this milestone.
- An unmineable bedrock floor and side boundaries keep the test area contained.
- The camera follows exploration above and below the initial surface.
- Mouse actions over the HUD do not modify terrain.
- One creature starts to the right of spawn. It waits until the player comes within 180 pixels, leaving room to gather first.
- Only the equipped sword can deal melee damage. Left click or J swings it. Switching equipment cancels the previous mining action or sword swing. Pickaxe, axe, and building materials cannot damage enemies.
- Solid terrain blocks melee damage in both directions, including newly placed blocks. Mining away a barrier opens the attack path again.
- Defeating the creature leaves the world and inventory intact. R restores terrain, inventory, player, and enemy together.
- Enemy navigation is still basic: it walks and collides with terrain, without jumping or finding routes around constructed barriers.

## Code map

| File | Responsibility |
| --- | --- |
| `Scripts/BlockWorld.cs` | Foreground terrain, generated background walls, collision, reach/visibility checks, mining, placement, target rendering |
| `Scripts/SandboxInventory.cs` | Material stack counts and spending |
| `Scripts/Equipment.cs` | Shared six-slot equipment mapping and tool/material rules |
| `Scripts/MaterialDrop.cs` | Dropped material attraction and collection |
| `Scripts/SandboxHud.cs` | Hotbar, counts, status, and controls |
| `Scripts/SandboxPlayground.cs` | Tool selection, combat/tool priority, integration checks, and optional scene switching |
| `Scripts/CombatRules.cs` | Shared terrain visibility check for melee damage |
| `Scenes/SandboxPlayground.tscn` | Combined world, player, training creature, and camera |

The test terrain uses one static body per block for simplicity. Chunking and procedural generation belong to later milestones; this is not the final large-world implementation.

## Verification

C# compilation passed with zero warnings and errors. Godot integration checks passed for mining input, material collection, material costs, invalid placements, placement visibility checks and separate mining/building reach limits, player overlap prevention, placed-block collision, removing collision when mining, resource recovery, different mining times, wood harvesting, secondary placement input, interrupted mining, underground camera framing, reset, and return to the combat room. Combined-scene checks also passed for notice range, live enemy placement protection, player and enemy melee obstruction, mining interruption during attacks, resource recovery after combat, enemy damage and defeat, simultaneous attack/build input, continued building after victory, and resetting both combatants. The original combat integration suite also passed. The rendered combined layout was inspected.

Run the sandbox checks with:

```powershell
& '<path-to-Godot-.NET.exe>' --headless --path 'C:\Tervain_Shattered_Realms\tervain--shattered-realms' -- --sandbox-test
```

Run the original combat checks with:

```powershell
& '<path-to-Godot-.NET.exe>' --headless --path 'C:\Tervain_Shattered_Realms\tervain--shattered-realms' 'res://Scenes/CombatPlayground.tscn' -- --prototype-test
```

## Not included yet

Crafting, a separate inventory window, placing or mining background walls, doors, furniture, automatic shelter detection, save/load, generated terrain, realm travel, audio, and final art.

## Feedback to gather

- Do mining times feel satisfying?
- Is selecting and placing blocks comfortable?
- Are targeting, reach, and blocked placement understandable?
- Can you comfortably construct and enter a small shelter?
- Does movement still feel as good as the accepted baseline?
- Can you switch comfortably between mining, fighting, and building without leaving the scene?

## Earlier 16px tuning — 2026-10-25

The developer requested a finer 16px block grid, a four-block-tall player, a jump one block higher, much longer mining range, and mining through solid terrain.

- Player drawing and collision body are now 64 pixels high. Combat targeting, item pickup, and building overlap checks use the resized body.
- The held-jump apex was measured in Godot at approximately 123.8 pixels, compared with 107.8 pixels previously: one additional 16px block. Jump speed is 606.3542; gravity remains 1550. Early release still shortens the jump.
- Mining range increased from 160 to 384 pixels. A buried block can be selected and mined directly without clearing the blocks in front of it.
- Drops still need to be collected nearby; remote mining does not automatically add distant materials to inventory.
- Building reach remains 160 pixels, and terrain still blocks placement and melee attacks.
- World width and height in pixels, spawn positions, and the overall landscape footprint are preserved by doubling the grid dimensions.

The C# build passed without warnings or errors. Godot checks measured the jump increase and verified player/grid scale, distant buried mining, range boundaries, protected bedrock, item collection, resized placement/collision, and combined combat/building. The original combat suite passed, and the rendered proportions were inspected.

## Generated background walls — 2026-10-25

Generated foreground blocks now receive a separate background wall of the same material, drawn darker and behind characters and foreground blocks. This includes the generated terrain, wood trunks, stone outcrops, and bedrock. Original open sky and cave air retain scenery with no block-wall layer.

Mining a generated foreground block reveals its background wall and leaves it intact. Background walls have no collision bodies, cannot be mined, produce no drops, and do not block movement, melee attacks, mining, or building visibility checks.

A wall in the target cell is an alternative to an adjacent foreground block when placing a block. This lets players attach an isolated foreground block to an exposed wall so it appears to float. Normal material costs, building range, visibility, and character overlap checks still apply. Player-built foreground blocks do not generate additional background walls.

To try it, excavate a chamber in the terrain, select dirt, stone, or wood, and place a block on a darker wall away from other foreground blocks. Mining that block returns its material and reveals the original wall again. In original open air, construction still needs an adjacent foreground block.

Verification passed in Godot for generation only behind original solid cells, unchanged sky/cave air, persistent walls after mining, unmineable wall-only cells, zero wall collision, walking through exposed walls, isolated wall-supported placement and material cost, normal collision on the new foreground block, and recovering that block without removing the wall. Existing sandbox and combined combat checks also passed. The C# build had zero warnings and errors, and an excavated chamber with an isolated foreground block was rendered and inspected.

## Current 24px scale and shorter reach — 2026-10-25

The developer requested halving mining reach and changing blocks to 24px with a player three blocks tall. This supersedes the earlier 16px proportions and 384px mining reach above.

- Blocks are now 24 by 24 pixels. The player drawing and collision body are 72 pixels high, exactly three blocks.
- Mining reach is 192 pixels, half the previous physical distance. This equals eight blocks on the new grid. Mining through intervening blocks remains enabled.
- The world uses 96 by 32 cells, retaining its overall 2304 by 768 pixel size. Terrain, trees, cave bounds, and surface stone are aligned to the new grid; the surface is now at y=456.
- Jump speed remains 606.3542, and the measured full jump is still approximately 123.8 pixels. Run speed, dodge settings, and combat timings are preserved.
- Building reach remains 160 pixels. Generated background walls remain non-solid, unmineable, and usable as foreground construction supports.
- Player drawing, collision, and the melee swing's vertical coverage have been adjusted for the taller body. Mining hints derive their block count from the current reach and tile size.

The C# build passed with zero warnings and errors. Godot checks passed for the exact three-block player size, world dimensions, preserved jump height, halved mining boundary, buried-block mining, material recovery, building collisions, non-solid background walls, wall-supported construction, and combat. The isolated combat suite also passed. Surface and excavated-wall previews were rendered and inspected.

## Pickaxe, axe, and sword — 2026-10-25

The developer requested separate tools for gathering and combat. The hotbar now contains six slots:

| Slot | Equipment | Action |
| --- | --- | --- |
| 1 | Pickaxe | Hold left mouse to mine dirt or stone |
| 2 | Axe | Hold left mouse to harvest wood, including placed wood blocks |
| 3 | Sword | Left click or J to swing at enemies |
| 4 | Dirt | Left or right mouse to build |
| 5 | Stone | Left or right mouse to build |
| 6 | Wood | Left or right mouse to build |

All three tools are supplied at spawn for this prototype. They do not wear out or require crafting yet. Wrong tools leave the block untouched and show the required tool in the targeting hint. The sword does not mine blocks, and mining tools do not damage enemies. Core mining checks enforce the equipped tool as well as the existing range and protection rules.

The held-item drawing changes with the selection. Pickaxe and axe swing while harvesting; materials appear as a held block. Changing equipment interrupts the previous action, preventing a hidden sword from dealing delayed damage. Sword damage, startup, active frames, and recovery retain the established combat values.

Godot checks passed for keyboard selection of slot six, wheel wrapping, dirt/stone mining with the pickaxe, rejected axe-on-stone and pickaxe-on-wood use, wood gathering with the axe, rejected sword-on-dirt use, sword attacks using both J and left click, rejected enemy damage from mining tools, equipment-switch cancellation, continued building, wall-supported wood placement/recovery, and reset to the starter pickaxe. The original combat suite passed. All three held-item selections and the expanded hotbar were rendered and inspected; C# compilation had zero warnings and errors.
