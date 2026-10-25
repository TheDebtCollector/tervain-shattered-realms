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
5. Build walls and a roof. The player is four blocks tall; leave a five-block-high entrance for comfortable headroom.
6. Try jumping onto your construction, mining underfoot, and recovering placed blocks.
7. Approach the creature to the right of spawn. Read its wind-up, dodge with Shift, and attack with J. Try building a barrier, mining it away, fighting, and then continuing construction in the same world.

The outline is pale green when an action is allowed and red when it is blocked. The mining meter appears inside the selected block.

## Implemented rules

- Small authored terrain: 144 by 48 cells, with 16-pixel blocks, trees, surface stone, and an underground cavity. Physical world dimensions remain 2304 by 768 pixels.
- Dirt, stone, and wood are mineable, collectible, and placeable. Each has its own stack in the hotbar.
- Pickaxe is a permanent starter tool. Material stacks start empty.
- Dirt takes 0.28 seconds to mine, wood 0.4 seconds, and stone 0.6 seconds.
- Changing the target or hotbar slot interrupts unfinished mining.
- Mining reaches 384 pixels (24 blocks) from the player body centre and ignores intervening blocks. Only distance, world bounds, player state, and protected bedrock determine whether a target can be mined. Building reaches 160 pixels (10 blocks) and still requires a clear path.
- Placed blocks need an adjacent foreground block or a background wall in the target cell. They cannot overlap the player or a living enemy.
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
| `Scripts/BlockWorld.cs` | Foreground terrain, generated background walls, collision, reach/visibility checks, mining, placement, target rendering |
| `Scripts/SandboxInventory.cs` | Material stack counts and spending |
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

## Grid, jump, and mining update — 2026-10-25

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
