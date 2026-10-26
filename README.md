# Tervain: Shattered Realms

An open-source 2D side-view sandbox built with **Godot .NET and C#**. Build a home, explore generated realms, and pursue better loot as a friendly beginning gives way to increasingly hostile places.

## Current prototype

- Responsive movement, variable jumping, dodging, and deliberate melee combat. A 68px-tall character explores a 24px block grid, fitting through three-block-high tunnels with 4px of clearance.
- One authored sandbox connects all **twenty biomes**, with matching terrain, background walls, vegetation/formations, scenery, and visual pools. Use the header selector or Page Up/Page Down to travel between regions.
- Mine, collect, and build with the original materials and 51 distinct added material stacks. The scrolling hotbar shows tools, collected materials, and the selected stack. Mining reaches 8 blocks (192px); background walls support construction, and unsupported sand falls.
- The fifteen added biome art sets include 180 editable native 24×24 ASE/PNG pairs. Art sources, prompts, export tools, validation, and twenty reviewed in-game captures are versioned.
- A standalone combat playground and automated sandbox, combat, and twenty-biome regression checks are included.

Seed-based world creation, realm travel, crafting, liquid mechanics, and save/load are planned. The current sandbox terrain is authored; prototype play progress is not saved. The future generated-world spawn is always (0, 0) in plains.

## Run

1. Install Godot **4.7.2 .NET** and a compatible .NET SDK (the project targets .NET 8).
2. Import `tervain--shattered-realms/project.godot` into Godot.
3. Press **F5** to play the sandbox.

Move with A/D, jump with Space, dodge with Shift, and equip the pickaxe (1), axe (2), or sword (3) to swing with left click or J. Tool swings deal 15 damage; sword swings deal 25. Holding left click repeats tool swings, including in empty air. Approach the creature to the right of spawn to fight in the same world where you mine and build. Hold Ctrl with the pickaxe or axe to mine the closest block ahead without mouse aiming. Select pickaxe (1) to mine dirt/stone/iron ore or axe (2) to harvest wood by holding left mouse. Select dirt, stone, wood, or iron ore with 4/5/6/7 and click to place. All three tools are supplied at spawn. F2 opens the combat room; F3 returns to a fresh sandbox. R resets the current scene.

## Design and development

This repository is also an Obsidian vault: open the repository folder as a vault to browse the design notes. Local Obsidian settings are excluded from Git.

- [Continue on another device](Design/Continue%20on%20Another%20Device.md)
- [World generation rules](Design/World%20Generation.md)
- [Twenty-biome integration and verification](Design/Biome%20Integration.md)
- [Biome artwork](Design/Biome%20Artwork.md)
- [Art workshop archive](Design/Art%20Workshop.md)
- [MVP design](Design/MVP%20Design.md)
- [Combat prototype](Design/Combat%20Prototype.md)
- [Sandbox prototype](Design/Sandbox%20Prototype.md)
- [Desert prototype](Design/Desert%20Prototype.md)
- [Snow and tundra prototype](Design/Tundra%20Prototype.md)
- [Block artwork and export workflow](Design/Artwork.md)
- [Version control workflow](Design/Version%20Control.md)

The working movement and combat baseline has been accepted after playtesting. The sandbox is ready for playtest feedback.

## License

[MIT](LICENSE). Third-party materials retain their respective licenses.
