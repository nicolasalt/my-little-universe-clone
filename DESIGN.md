# 3D RPG Game Design Document

## Overview
A 3D third-person RPG built in Godot 4.5 with C#. This document outlines the core architecture and systems.

---

## Technical Stack
- **Engine**: Godot 4.5
- **Language**: C# (.NET)
- **Renderer**: Forward Plus (optimized for 3D)
- **Target**: PC (Windows/Linux/Mac)

---

## Architecture

### Folder Structure
```
res://
├── scenes/
│   ├── main/              # Main.tscn - entry point
│   ├── characters/        # Player.tscn, enemies, NPCs
│   ├── levels/            # World scenes
│   └── ui/                # HUD.tscn, menus
├── scripts/
│   ├── player/            # Player.cs, PlayerCamera.cs
│   ├── systems/           # GameManager.cs, SignalBus.cs
│   ├── resources/         # PlayerStats.cs
│   └── ui/                # HUD.cs
├── resources/
│   └── stats/             # Default stat configurations
└── assets/
    ├── models/
    ├── textures/
    └── audio/
```

### Autoloads (Singletons)
| Name | Script | Purpose |
|------|--------|---------|
| GameManager | GameManager.cs | Game state, pause, scene management |
| SignalBus | SignalBus.cs | Global event system |

---

## Core Systems

### 1. Player Controller
**Scene**: `scenes/characters/Player.tscn`
**Root Node**: CharacterBody3D

**Components**:
- Player.cs - Movement, input handling, state
- Camera3D (child) - Third-person follow camera
- CollisionShape3D - Capsule collider
- MeshInstance3D - Placeholder capsule mesh

**Movement Specs**:
- Walk speed: 5 m/s
- Sprint speed: 8 m/s
- Jump velocity: 4.5 m/s
- Gravity: Project default (9.8)
- Mouse sensitivity: 0.3

**Input Actions**:
| Action | Default Binding |
|--------|-----------------|
| move_forward | W |
| move_back | S |
| move_left | A |
| move_right | D |
| jump | Space |
| sprint | Shift |
| interact | E |

### 2. Player Stats
**Resource**: `PlayerStats.cs` (extends Resource)

**Properties**:
```csharp
float MaxHealth = 100
float CurrentHealth = 100
float MaxStamina = 100
float CurrentStamina = 100
float MaxMana = 50
float CurrentMana = 50
float StaminaRegenRate = 10  // per second
float ManaRegenRate = 5      // per second
```

**Signals**:
- HealthChanged(float current, float max)
- StaminaChanged(float current, float max)
- ManaChanged(float current, float max)
- PlayerDied()

### 3. Game Manager
**Autoload**: `GameManager.cs`

**Responsibilities**:
- Track game state (Playing, Paused, Menu)
- Handle pause/unpause
- Manage scene transitions
- Store reference to current player

**States**:
```csharp
enum GameState { MainMenu, Playing, Paused, Dialogue, Inventory }
```

### 4. Signal Bus
**Autoload**: `SignalBus.cs`

**Global Signals**:
- PlayerSpawned(Player player)
- PlayerDied()
- EnemyDefeated(Enemy enemy)
- ItemPickedUp(Item item)
- QuestUpdated(Quest quest)

### 5. HUD System
**Scene**: `scenes/ui/HUD.tscn`
**Root Node**: CanvasLayer

**Elements**:
- Health bar (ProgressBar)
- Stamina bar (ProgressBar)
- Interaction prompt (Label)
- Crosshair (TextureRect, optional)

---

## Scene Hierarchy

### Main.tscn
```
Main (Node3D)
├── WorldEnvironment
├── DirectionalLight3D
├── Ground (StaticBody3D)
│   ├── MeshInstance3D (PlaneMesh 50x50)
│   └── CollisionShape3D
├── Player (instance of Player.tscn)
└── HUD (instance of HUD.tscn)
```

### Player.tscn
```
Player (CharacterBody3D)
├── CollisionShape3D (CapsuleShape3D)
├── MeshInstance3D (CapsuleMesh - placeholder)
├── CameraArm (Node3D) - pivot point
│   └── Camera3D
└── InteractionRayCast (RayCast3D)
```

---

## Input Configuration
Added to project.godot under [input]:

```
move_forward = Key W
move_back = Key S
move_left = Key A
move_right = Key D
jump = Key Space
sprint = Key Shift
interact = Key E
pause = Key Escape
```

---

## Future Expansion

### Phase 2 - Combat
- Melee attack system
- Damage calculation
- Enemy AI with state machine
- Hit detection with hitboxes

### Phase 3 - Inventory
- Item base class
- Inventory container
- Equipment slots
- Item pickups in world

### Phase 4 - NPCs & Dialogue
- NPC base class
- Dialogue system with choices
- Quest givers

### Phase 5 - Quests
- Quest resource definition
- Quest tracker
- Objectives system

### Phase 6 - Save/Load
- Serialization system
- Save slots
- Auto-save
