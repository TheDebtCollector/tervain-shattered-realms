# Tervain: Shattered Realms

An open-source 2D side-view sandbox built with **Godot .NET and C#**. Build a home, explore generated realms, and pursue better loot as a friendly beginning gives way to increasingly hostile places.

## Current prototype

- Responsive movement, variable jumping, dodging, and deliberate melee combat.
- A combat playground with a telegraphed enemy attack and recovery windows.
- A separate sandbox with mineable dirt, stone, and wood, collectible drops, a four-slot hotbar, and block placement.

Realm travel, procedural world generation, crafting, and save/load are planned. The current sandbox terrain is authored; prototype play progress is not saved.

## Run

1. Install Godot **4.7.2 .NET** and a compatible .NET SDK (the project targets .NET 8).
2. Import `tervain--shattered-realms/project.godot` into Godot.
3. Press **F5** to play the sandbox.

Move with A/D, jump with Space, dodge with Shift, and attack with J. Select the pickaxe with 1 and hold left mouse to mine. Select dirt, stone, or wood with 2/3/4 and click to place. F2 opens the combat room; F3 returns to a fresh sandbox. R resets the current scene.

## Design and development

This repository is also an Obsidian vault: open the repository folder as a vault to browse the design notes. Local Obsidian settings are excluded from Git.

- [MVP design](Design/MVP%20Design.md)
- [Combat prototype](Design/Combat%20Prototype.md)
- [Sandbox prototype](Design/Sandbox%20Prototype.md)
- [Version control workflow](Design/Version%20Control.md)

The working movement and combat baseline has been accepted after playtesting. The sandbox is ready for playtest feedback.

## License

[MIT](LICENSE). Third-party materials retain their respective licenses.
