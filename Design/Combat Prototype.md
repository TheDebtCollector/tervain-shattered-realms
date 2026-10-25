---
tags:
  - tervain
  - development
  - prototype
created: 2026-10-25
status: baseline-accepted
---

# Combat Prototype

Related: [[MVP Design]]

## Play

The project is in `C:\Tervain_Shattered_Realms\tervain--shattered-realms`. F5 now opens [[Sandbox Prototype]]. Press **F2** there to enter the combat room, or open `Scenes/CombatPlayground.tscn` and press **F6**. Press F3 in the combat room to return to a fresh sandbox. If Godot prompts to reload externally changed files, reload them.

| Input | Action |
| --- | --- |
| A / D or Left / Right | Move and face a direction |
| Space, W, or Up | Jump; release early for a shorter jump |
| Shift | Dodge in the facing direction |
| J or left mouse button | Melee attack in the facing direction |
| R | Restart the encounter at any time |

Attacks follow the direction the character faces, not the mouse cursor. Platforms can be jumped through from below. Dodge can cancel an attack, has a cooldown, and protects the player while active. Air dodging is currently allowed.

## What is implemented

- One small authored room with platforms and placeholder drawings.
- Accelerated horizontal movement, variable jump height, a short jump buffer, and coyote time.
- A melee attack with startup, a brief active window, and recovery; movement slows during an attack.
- A short dodge with invulnerability and a cooldown.
- Player health, temporary protection after taking damage, death, and restart.
- One enemy that approaches, locks its facing direction, signals an attack, strikes, and recovers.
- Enemy health and a visible status prompt. Four successful starter-weapon hits defeat it.
- A wind-up that continues when hit; recovery windows can be interrupted by attacks.
- An on-screen control guide and health/dodge indicators.

No inventory, loot, mining, realm travel, saving, healing, audio, or final art is included yet.

## Code map

| File | Responsibility |
| --- | --- |
| `Scripts/Player.cs` | Movement, jump, dodge, melee hit detection, player health, placeholder character drawing |
| `Scripts/Enemy.cs` | Enemy attack states, telegraph, damage, and placeholder enemy drawing |
| `Scripts/CombatPlayground.cs` | Room geometry, input bindings, HUD, restart, and engine-driven smoke tests |
| `Scenes/CombatPlayground.tscn` | Main scene and character nodes |
| `Tervain.csproj` | Godot .NET SDK and C# build settings |

Select **Player** or **TrainingCreature** in the main scene to edit exported settings in the Inspector. Start with move speed, jump speed, dodge duration/cooldown, enemy wind-up duration, and enemy recovery duration. Room collision shapes and HUD are constructed in code at runtime for this first prototype.

## Verification

The project compiled with zero warnings and errors. An integration smoke test ran in Godot's physics loop and passed checks for floor collision, movement input, jumping, dodge activation and cooldown, dodge invulnerability, post-hit protection, melee damage per swing, enemy defeat, enemy wind-up/strike/recovery, enemy damage, player death, and a fresh state after scene reload.

A rendered screenshot was inspected for layout. On 2026-10-25, the developer playtested the prototype, reported liking the movement, and described combat as smooth. The current movement and combat feel are accepted as the baseline. This feedback does not establish final difficulty balance.

To rerun the engine smoke test from a terminal, use your Godot executable:

```powershell
& '<path-to-Godot-.NET.exe>' --headless --path 'C:\Tervain_Shattered_Realms\tervain--shattered-realms' 'res://Scenes/CombatPlayground.tscn' -- --prototype-test
```

## First playtest questions

1. Does movement feel responsive or too slippery?
2. Is jump height easy to control?
3. Can you recognise the enemy's wind-up before its first hit?
4. Does dodging feel reliable, and is its cooldown understandable?
5. Is there enough time to counterattack during recovery?
6. Is the encounter enjoyable without loot rewards?

The first movement and combat milestone is accepted. Preserve the current feel while developing the sandbox foundation. Revisit tuning when new mechanics or playtest feedback identify a specific need.

## Original accepted baseline — 2026-10-25

Player settings: movement speed 280, jump speed 565, gravity 1550, dodge speed 650, dodge duration 0.18 seconds, dodge cooldown 0.8 seconds, and melee damage 25.

Enemy settings: movement speed 100, attack wind-up 0.65 seconds, recovery 0.85 seconds, and attack damage 20.

These are reference values from the current scripts. Keep them as the initial baseline for future work rather than retuning them without a concrete reason.

## Next milestone

Sandbox foundation: mining and placing blocks, collecting materials, a basic hotbar/inventory, and then simple crafting. The next playable target is to dig out material and use it to construct a small shelter while keeping the accepted movement and combat feel.


## Sandbox implementation update — 2026-10-25

Mining, resource drops and collection, hotbar stacks, and block placement are now implemented in [[Sandbox Prototype]]. The accepted Player script was kept unchanged, and the combat regression checks passed. Simple crafting remains the next part of the sandbox foundation milestone.

## Combined world update — 2026-10-25

At the developer's request, combat is now available directly in [[Sandbox Prototype]] alongside mining and building. J attacks from any hotbar slot; mouse clicks use the selected tool or material. The creature waits to the right of spawn until approached. Built terrain shields both sides from melee damage, and blocks cannot be placed inside a living enemy. Movement and combat timing values remain at the accepted baseline. The isolated combat room remains available for tuning and regression checks.

## Earlier 16px movement tuning — 2026-10-25

The developer subsequently requested one extra block of jump height and a player four blocks tall on a 16px grid. Current player height is 64px, and jump speed is 606.3542. Measured held-jump height increased from about 107.8px to 123.8px. Run speed, gravity, dodge, damage, and combat timing values remain as above. These explicit requests supersede the original baseline's jump speed and character proportions. See [[Sandbox Prototype]] for the full scale and mining changes.

## Current movement scale — 2026-10-25

The latest requested grid uses 24px blocks and a 72px player, exactly three blocks tall. Jump speed remains 606.3542 and full jump height remains approximately 123.8px. Run speed, gravity, dodge settings, and combat timings are preserved. Collision and melee coverage use the new player proportions. Mining reach is now 192px (eight blocks). See [[Sandbox Prototype]] for current values and validation.

## Equipped sword requirement — 2026-10-25

The main sandbox now requires the sword (slot 3) to deal melee damage. Left click and J both swing the selected sword. Pickaxe (1), axe (2), and materials (4–6) cannot attack enemies. Switching equipment cancels any pending sword swing. The isolated combat room starts with the sword equipped and keeps its original J/left-click controls. Combat timing and damage remain at the accepted values. See [[Sandbox Prototype]] for the current six-slot controls; this supersedes earlier notes about attacking from any hotbar slot.
