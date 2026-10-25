---
tags:
  - tervain
  - development
  - biome
created: 2026-10-25
status: ready-for-playtest
---

# Desert Prototype

Related: [[Sandbox Prototype]] · [[Artwork]]

## Explore

Walk right from the forest into the desert, starting at column 68 (x=1632). The desert retains its original 28-column span (columns 68–95) and is followed by the new [[Tundra Prototype]]. These columns contain sand, sandstone, cracked-earth patches, a low dune, four cacti, and three dead bushes. A warm sky, sun, and distant dunes replace the forest backdrop in this area. Realm generation and saving remain later milestones.

All seven new desert Aseprite assets are preserved with their PNG exports: `sand-block-24`, `sand-background-24`, `sandstone-block-24`, `sandstone-background-24`, `cracked-earth-block-24`, `cactus-24`, and `dry-bush-24`.

## Gathering and plants

| Material | Tool | Harvest time | Drop |
| --- | --- | --- | --- |
| Sand | Pickaxe | 0.25 s | One sand block |
| Sandstone | Pickaxe | 0.65 s | One sandstone block |
| Cracked earth (hotbar: Dry earth) | Pickaxe | 0.35 s | One cracked-earth block |
| Cactus | Axe | 0.4 s | One cactus block |
| Dead bush | Any pickaxe, axe, or sword hit | One connected swing | Nothing |

Cacti use the existing solid-block harvesting and placement rules; a pickaxe or sword cannot harvest them. Cactus resources have their own stack, can be placed, and can be recovered with the axe. No new contact damage is introduced.

Dead bushes are non-solid scenery with a real melee hit response. Player attacks and enemy strikes clear bushes inside their hit bounds, subject to solid-terrain obstruction. Falling sand also destroys bushes it touches. Removing their ground support or placing a block in their cell clears them without drops. They are not inventory items and cannot be harvested repeatedly.

## Falling sand

Sand loses support when there is no foreground block immediately below it. Background walls do not support it. The world releases unsupported sand columns into full-size moving terrain bodies; both generated and player-placed sand fall.

- Falling sand accelerates at 900 pixels/s², capped at 600 pixels/s, and collides with terrain, other falling sand, and characters. It causes no contact or crushing damage in this milestone.
- A small horizontal collider clearance prevents sand catching on seams inside one-block-wide shafts.
- On reaching a supported, unoccupied grid cell, sand returns to ordinary solid, mineable terrain. Sand resting on a character waits until a safe terrain cell becomes available; it never settles inside the player or a living creature.
- Sand remains mineable while falling. Mining it removes the moving body exactly once, preventing a second block from appearing when it would have landed.
- Placement cannot overlap a moving sand body. Falling and settlement do not spend inventory, create extra drops, or add background walls. Mined sand still produces an ordinary small gravity-affected resource pickup.
- Other terrain keeps its current support behaviour; sandstone and cracked earth do not fall.

Generated sand uses the new sand wall texture; sandstone and cracked earth use the sandstone wall texture. Mining and sand movement preserve original generated walls. Original air and vegetation cells do not create new background walls.

## Material controls

Existing slots 1–7 keep their meanings. The hotbar now fits eleven compact slots on one row.

| Key | Material |
| --- | --- |
| 8 | Sand |
| 9 | Sandstone |
| 0 | Cracked earth (Dry earth) |
| Minus (-) | Cactus |

The mouse wheel and hotbar buttons also select every slot. Use left or right mouse to build, the pickaxe to mine terrain, and the axe to harvest cacti.

## Code and validation

`BlockWorld.cs` owns desert terrain, scenery, bush removal, unsupported sand detection, and settlement. `FallingSand.cs` handles full-block movement and collisions. `Player.cs` and `Enemy.cs` deliver actual swing/strike hit bounds to bushes. `Equipment.cs`, `SandboxInventory.cs`, `BlockArt.cs`, and `SandboxHud.cs` include the new materials, artwork, and controls.

Focused checks run with `-- --desert-test`; the complete sandbox suite includes the same desert checks. They cover terrain composition, tool restrictions, cactus collection and placement costs, real pickaxe/axe/sword bush hits with no drops, unsupported sand columns, block conservation, wall preservation, player-placed sand, settled-sand recovery, and mining moving sand without duplication. The C# build, full sandbox suite, and original combat suite are checked before the checkpoint is published. A desert preview can be captured with `-- --desert-preview`.
