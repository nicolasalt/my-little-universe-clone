---
title: Dungeon System
status: draft
modifies: systems/worlds/dungeons.md
priority: P2
author: AI
created: 26-01-17
---

# Dungeon System

65+ dungeons with unique challenges and rewards.

## Summary

Implement the dungeon system including entrances, interior layouts, puzzles, and boss encounters.

## Design

### Dungeon Data

```csharp
[GlobalClass]
public partial class DungeonData : Resource
{
    [Export] public string Id;
    [Export] public string DisplayName;
    [Export] public int WorldId;
    [Export] public DungeonType Type;
    [Export] public int RecommendedLevel;
    [Export] public PackedScene Scene;
    [Export] public bool HasBoss;
    [Export] public bool HasSourceCore;
    [Export] public EnemyData[] EnemyPool;
    [Export] public Drop[] CompletionRewards;
}

public enum DungeonType
{
    Resource,    // High resource concentration
    Combat,      // Dense enemy spawns
    Puzzle,      // Buttons, keys, pressure plates
    Boss         // Single powerful enemy
}
```

### Dungeon Entrance

```csharp
public partial class DungeonEntrance : Area3D
{
    [Export] public DungeonData Dungeon;

    private bool _isCleared;

    public void OnPlayerEntered()
    {
        ShowEnterPrompt();
    }

    public void EnterDungeon()
    {
        dungeonManager.LoadDungeon(Dungeon);
    }
}
```

### Dungeon Manager

```csharp
public partial class DungeonManager : Node
{
    public DungeonData CurrentDungeon { get; private set; }
    public bool IsInDungeon { get; private set; }

    [Signal] public delegate void DungeonEnteredEventHandler(string dungeonId);
    [Signal] public delegate void DungeonClearedEventHandler(string dungeonId);
    [Signal] public delegate void DungeonExitedEventHandler();

    public void LoadDungeon(DungeonData data);
    public void ExitDungeon();
    public void OnAllEnemiesDefeated();
    public void OnBossDefeated();
}
```

### Puzzle Elements

```csharp
public partial class PressurePlate : Area3D
{
    [Export] public Node3D[] Targets;  // Doors, bridges, etc.
    [Export] public bool RequiresWeight;

    public void OnBodyEntered(Node body);
    public void OnBodyExited(Node body);
    private void ActivateTargets();
}

public partial class Lever : StaticBody3D
{
    [Export] public Node3D[] Targets;
    [Export] public bool IsOneTime;

    private bool _isActivated;

    public void Interact();
    private void ToggleTargets();
}

public partial class LockedDoor : StaticBody3D
{
    [Export] public KeyColor RequiredKey;

    public bool TryUnlock(Backpack backpack);
}

public enum KeyColor { Gold, Red, Green, Blue }
```

### Key System

```csharp
public partial class KeyPickup : Area3D
{
    [Export] public KeyColor Color;

    public void OnPlayerEntered()
    {
        player.Backpack.Add(GetResourceType(Color), 1);
        QueueFree();
    }

    private ResourceType GetResourceType(KeyColor color) => color switch
    {
        KeyColor.Gold => ResourceType.GoldKey,
        KeyColor.Red => ResourceType.RedKey,
        KeyColor.Green => ResourceType.GreenKey,
        KeyColor.Blue => ResourceType.BlueKey,
        _ => ResourceType.GoldKey
    };
}
```

### Hazards

```csharp
public partial class LavaArea : Area3D
{
    [Export] public float DamagePerSecond = 10f;

    public override void _Process(double delta)
    {
        foreach (var body in GetOverlappingBodies())
        {
            if (body is Player player)
            {
                if (!player.HasFireproof)
                    player.TakeDamage((float)(DamagePerSecond * delta));
            }
        }
    }
}

public partial class PoisonSwamp : Area3D
{
    [Export] public float DamagePerSecond = 5f;
    // Similar, checks Antidote
}

public partial class DeepWater : Area3D
{
    [Export] public float BreathDuration = 10f;
    // Drowning after breath runs out
}
```

### Dungeon Layout

```
Dungeon Scene Structure:
├── SpawnPoint (Node3D)
├── Rooms (Node3D)
│   ├── Room1
│   │   ├── EnemySpawners
│   │   ├── ResourceNodes
│   │   └── Hazards
│   ├── Room2
│   └── BossRoom
├── Puzzles (Node3D)
│   ├── PressurePlates
│   ├── Levers
│   └── LockedDoors
├── Collectibles (Node3D)
│   ├── Keys
│   ├── SourceCore
│   └── Treasure
└── Exit (Area3D)
```

## Implementation Checklist

- [ ] Create DungeonData resource
- [ ] Implement DungeonEntrance
- [ ] Create DungeonManager
- [ ] Implement puzzle elements (plates, levers, doors)
- [ ] Create key/lock system
- [ ] Add hazard areas (lava, poison, water)
- [ ] Implement dungeon loading/unloading
- [ ] Create Source Core pickup
- [ ] Design first dungeon (Gaia)
- [ ] Add dungeon completion tracking
