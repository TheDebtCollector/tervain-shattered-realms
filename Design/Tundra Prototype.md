---
tags:
  - tervain
  - development
  - biome
created: 2026-10-25
status: ready-for-playtest
---

# Snow and Tundra Prototype

Related: [[Sandbox Prototype]] · [[Desert Prototype]] · [[Artwork]]

## Explore

Continue right through the desert to enter the tundra at column 96 (x=2304). The authored world now spans 128×32 cells, or 3072×768 pixels. The forest and desert retain their original terrain and positions. The camera and world boundary extend to the new right edge.

The tundra has snow-covered ground, frozen earth beneath it, underground ice, a frozen surface pond, a low snowdrift, four snow pines, and four tundra shrubs. A cold sky, pale sun, snowy mountains, and distant conifers establish the biome's scenery.

## Resources and plants

| Material | Tool | Harvest time | Drop |
| --- | --- | --- | --- |
| Snow | Pickaxe | 0.25 s | One snow block |
| Ice | Pickaxe | 0.45 s | One ice block |
| Frozen earth | Pickaxe | 0.55 s | One frozen-earth block |
| Tundra pine wood | Axe | 0.4 s per trunk segment | One tundra-wood block |
| Tundra shrub | Any connected tool or weapon hit | One hit | Nothing |

The four resources have separate inventory stacks and can be placed and recovered with the appropriate tool. Dropped resources use the existing gravity and pickup attraction. Ctrl mining includes the new blocks and preserves their tool restrictions.

Snow, ice, and frozen earth use ordinary solid-terrain collision and preserve the accepted movement and jump feel. Snow does not inherit sand gravity. Ice has no added sliding or melting mechanic in this milestone.

Each pine has two solid tundra-wood trunk cells. Its snow-pine sprite is drawn with nearest filtering at an integer scale, cropped to remove transparent padding so it sits on the surface. Harvesting the trunk leaves darkened wooden background cells and a dim pine silhouette, with no collision or extra harvestable wood. Generated background walls support foreground building as before.

Shrubs share the dead-bush hit logic: pickaxe, axe, sword, enemy strike, or falling-sand contact can clear them. Mining their support or building in their cell also removes them. They never create resource drops or background-wall silhouettes.

## Artwork and walls

All eight new Aseprite sources and matching PNG exports are versioned:

- `snow-block-24` and `snow-background-24`
- `ice-block-24` and `ice-background-24`
- `frozen-earth-block-24`
- `tundra-wood-block-24`
- `snow-pine-24`
- `tundra-shrub-24`

Snow and frozen earth reveal the snow background tile. Ice reveals its own ice background. Pine trunks use the tundra-wood tile at 42% brightness. Original air cells remain without block walls, and player-built blocks create no new walls.

## Selecting materials

The hotbar now contains fifteen equipment/material slots. Existing number-key shortcuts remain unchanged. Scroll the mouse wheel through the added snow, ice, frozen-earth, and tundra-wood slots, or drag the horizontal hotbar scrollbar and click their icons. The hotbar follows the selected slot automatically, keeping it on screen. Keys 1 and 2 still return directly to pickaxe and axe.

## Validation

The focused biome checks run with `-- --tundra-test`; they are also included in `-- --sandbox-test`. They cover biome terrain and the frozen pond, camera limits, tool requirements, collecting/placing/recovering the new resources, complete pine harvesting, preserved non-solid pine backgrounds, and shrub hits without drops. The full sandbox suite also checks movement, three-block tunnel clearance, mining, building, sand gravity, combat, Ctrl mining, reset, and fifteen-slot wheel wrapping.

Visual previews use `-- --tundra-preview` and `-- --tundra-wall-preview`, covering the surface scenery, exposed snow/ice walls, and the hotbar scrolled to the final material.
