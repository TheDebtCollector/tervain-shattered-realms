---
tags:
  - tervain
  - game-design
  - mvp
created: 2026-10-25
status: working-draft
---

# Tervain: Shattered Realms — MVP Design

## Game vision

A 2D side-view sandbox where players customise a character, establish a home in a generated world, and explore a network of persistent realms for better loot and progression.

The beginning feels relatively friendly. As players progress into more dangerous realms, the experience becomes increasingly hellish. Building and returning home give exploration a lasting purpose.

## Agreed decisions

- **Perspective:** side-view, with horizontal movement and vertical exploration through jumping, climbing, and digging.
- **Genre:** sandbox with character customisation, building, and generated worlds.
- **Replayability:** generated terrain and realm arrangements should create different experiences between saves.
- **World structure:** multiple realms within one save, connected by gates. The final vision includes many realms.
- **Persistence:** visited realms and player changes persist when returning.
- **Progression:** better loot and equipment motivate exploration of increasingly dangerous realms.
- **Tone:** a welcoming beginning that becomes progressively more hostile and frightening.
- **Combat direction:** deliberate action combat, with dodging, positioning, and timing becoming essential as difficulty increases.
- **Technology:** Godot .NET with C#. The developer already has C# experience.
- **Documentation:** useful design notes, decisions, and development milestones belong in this Obsidian vault.

## Distinctive direction

Realm travel is central to progression. A substantial home realm connects to other places with different environments, threats, resources, and rewards.

To distinguish the game from Terraria, realms must eventually offer meaningful differences in how players explore, prepare, fight, or build. Separate maps alone are insufficient.

The long-term proposal is branching progression: similarly difficult realms can offer different equipment or play styles. Realm identities, terrain layouts, and connections can vary between saves.

## Core loop

Build a home → discover a gate → prepare → explore another realm → return with loot → craft or upgrade → attempt a harder challenge.

## Proposed MVP boundary

This scope is a working proposal derived from the discussion, rather than a fully approved feature specification.

### Home realm

- One small, finite, generated starting realm with a safe spawn area.
- Basic character appearance choices and a name.
- Walking, jumping, and basic vertical exploration.
- Gather wood and stone; mine and place blocks.
- A hotbar and a small inventory with stackable materials.
- A workbench, starter equipment, torches, and enough building pieces for a simple shelter.

### Second realm

- One persistent gate connecting the home realm to one foreign realm.
- A visibly different, more dangerous environment.
- A useful material for an equipment upgrade.
- Two basic enemy types and one guarded reward encounter, subject to prototype findings.
- A stronger craftable weapon and one special loot item whose effect remains to be chosen.

### Combat

- One melee weapon with a clear attack and recovery period.
- A dodge available from the beginning, with brief invulnerability and a cooldown.
- Enemies signal attacks and expose recovery windows.
- Health, damage, temporary protection after a hit, and a way to restart after defeat.
- Limited healing is proposed; implementation details remain open.

### Persistence

- One save slot contains the character, inventory, realm identities, gate connections, and changes made in each realm.
- Generate a realm when first needed, then preserve its state.
- Fully simulate only the active realm initially.
- Exact file format and save layout remain implementation decisions. One save does not require one physical file.

## First playable journey

1. Create a character and enter a friendly generated realm.
2. Gather materials and build a small home.
3. Craft basic equipment and discover a gate.
4. Enter the second realm and learn to survive its threats.
5. Bring materials home and craft an upgrade.
6. Return, overcome the guarded encounter, and claim worthwhile loot.
7. Save, quit, and return with progress intact.

## Development milestones

### 1. Movement and combat playground

Build a small authored room with placeholder graphics, platforms, a player, and one enemy. Implement movement, jumping, melee attacks, dodging, health, and restarting.

**Completion check:** fighting one enemy is readable and enjoyable before loot rewards or procedural generation are added.

### 2. Sandbox foundation

Implement block mining and placement, item pickup, a hotbar, a basic inventory, and a small set of crafting recipes.

**Completion check:** gather materials, craft an item, and build a basic shelter.

### 3. Realm travel and persistence

Connect two small authored realms through a gate. Preserve their changes, player inventory, and return locations across travel and save/load.

**Completion check:** change both realms, travel between them, restart the game, and verify that the changes remain.

### 4. World generation and progression

Replace test terrain with small generated realms. Add the second realm's material, equipment upgrade, enemies, and guarded reward.

**Completion check:** complete the first playable journey; another seed changes exploration decisions while preserving a valid route to progression.

## Deferred features

- Many realm types and a large branching realm network.
- Environmental effects, plants, or creatures spreading through gates.
- Multiplayer.
- Large equipment trees, extensive weapon classes, and magic systems.
- Complex NPC relationships, farming, and hunger systems.
- Large boss rosters and endgame content.

Gate consequences are a promising future identity feature. They are not required for the initial loot-driven MVP.

## Open design questions

- Visual style, character proportions, and tile scale.
- Target platforms; desktop is the current working assumption.
- Second realm theme and its signature reward.
- Gate discovery and activation requirements.
- Death consequences and recovery of carried loot.
- Healing rules, dodge behaviour, and whether stamina adds value.
- Whether combat aiming is directional or follows the cursor.
- World dimensions, generation constraints, and realm difficulty signals.

Resolve movement and combat questions through the prototype before committing to a large content scope.


## Development status — 2026-10-25

- Movement and combat playground: implemented and accepted after developer playtesting. See [[Combat Prototype]].
- Sandbox foundation: mining, collectible materials, hotbar stacks, and block placement implemented; awaiting playtest feedback. Simple crafting remains. See [[Sandbox Prototype]].
- Realm travel, persistence, and procedural world generation: not started.
