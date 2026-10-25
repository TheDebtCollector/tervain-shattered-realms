# Tervain: Shattered Realms

An open-source 2D side-view sandbox built with **Godot .NET and C#**. Build a home, explore generated realms, and pursue better loot as a friendly beginning gives way to increasingly hostile places.

## Current prototype

- Responsive movement, variable jumping, dodging, and deliberate melee combat. A 68px-tall character explores a 24px block grid, fitting through three-block-high tunnels with 4px of clearance.
- A combat playground with a telegraphed enemy attack and recovery windows.
- One combined sandbox with mineable dirt, stone, wood, and iron ore, collectible drops, an eleven-slot equipment/material hotbar, block placement, and an enemy encounter. Terrain provides cover during combat. Mining reaches 8 blocks (192px) and can target buried blocks directly. Generated blocks leave non-solid background walls after mining; foreground blocks can be attached to those walls without neighbouring blocks.

The developer’s 24×24 pixel art now appears on terrain, background walls, pickups, held blocks, and material icons. A small iron ore deposit sits beside the underground cave.

A desert begins to the right of the forest, with sand, sandstone, cracked earth, axe-harvested cacti, and dead bushes that break under any weapon hit without drops. Unsupported sand falls and settles back into terrain, including player-placed sand. Select the new materials with 8, 9, 0, and Minus, or use the mouse wheel/hotbar.

Realm travel, procedural world generation, crafting, and save/load are planned. The current sandbox terrain is authored; prototype play progress is not saved.

## Run

1. Install Godot **4.7.2 .NET** and a compatible .NET SDK (the project targets .NET 8).
2. Import `tervain--shattered-realms/project.godot` into Godot.
3. Press **F5** to play the sandbox.

Move with A/D, jump with Space, dodge with Shift, and equip the pickaxe (1), axe (2), or sword (3) to swing with left click or J. Tool swings deal 15 damage; sword swings deal 25. Holding left click repeats tool swings, including in empty air. Approach the creature to the right of spawn to fight in the same world where you mine and build. Select pickaxe (1) to mine dirt/stone/iron ore or axe (2) to harvest wood by holding left mouse. Select dirt, stone, wood, or iron ore with 4/5/6/7 and click to place. All three tools are supplied at spawn. F2 opens the combat room; F3 returns to a fresh sandbox. R resets the current scene.

## Design and development

This repository is also an Obsidian vault: open the repository folder as a vault to browse the design notes. Local Obsidian settings are excluded from Git.

- [MVP design](Design/MVP%20Design.md)
- [Combat prototype](Design/Combat%20Prototype.md)
- [Sandbox prototype](Design/Sandbox%20Prototype.md)
- [Desert prototype](Design/Desert%20Prototype.md)
- [Block artwork and export workflow](Design/Artwork.md)
- [Version control workflow](Design/Version%20Control.md)

The working movement and combat baseline has been accepted after playtesting. The sandbox is ready for playtest feedback.

## License

[MIT](LICENSE). Third-party materials retain their respective licenses.
