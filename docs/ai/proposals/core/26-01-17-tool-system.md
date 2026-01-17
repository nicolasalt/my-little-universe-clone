---
title: Tool System
status: review
modifies: systems/tools/tool-system.md
priority: P0
author: AI
created: 26-01-17
---

# Tool System

Three primary tools for gathering and combat.

## Summary

Implement the tool system with Pickaxe, Axe, and Sword. Each tool has levels, affects gather/combat stats, and has unique charged abilities.

## Design

### Tool Data

```csharp
public enum ToolType { Pickaxe, Axe, Sword }

[GlobalClass]
public partial class ToolData : Resource
{
    [Export] public ToolType Type;
    [Export] public int Level = 1;  // 1-8
    [Export] public Texture2D Icon;
    [Export] public PackedScene ModelPrefab;
    [Export] public PackedScene AbilityVFX;

    public int Tier => (Level - 1) / 2;  // 0-3 (Red, Green, Purple, Black)

    public float GatherSpeedMod => 1 + (Level * 0.1f);
    public float DamageMod => 1 + (Level * 0.15f);
    public float YieldMod => 1 + (Level * 0.1f);
    public float AbilityRangeMod => 1 + (Level * 0.05f);
}
```

### Tool Manager

```csharp
public partial class ToolManager : Node
{
    [Export] public ToolData Pickaxe;
    [Export] public ToolData Axe;
    [Export] public ToolData Sword;

    public ToolData CurrentTool { get; private set; }
    public bool AutoSwitch = true;

    [Signal] public delegate void ToolChangedEventHandler(ToolType newTool);
    [Signal] public delegate void ToolUpgradedEventHandler(ToolType tool, int newLevel);

    public void SelectTool(ToolType type);
    public void CycleTool();
    public void AutoSelectTool(InteractionType nearestInteraction);
    public void UpgradeTool(ToolType type);
}
```

### Auto-Selection Logic

```csharp
public void AutoSelectTool(InteractionType nearest)
{
    if (!AutoSwitch) return;

    ToolType appropriate = nearest switch
    {
        InteractionType.Tree => ToolType.Axe,
        InteractionType.Rock => ToolType.Pickaxe,
        InteractionType.Ore => ToolType.Pickaxe,
        InteractionType.Gem => ToolType.Pickaxe,
        InteractionType.Enemy => ToolType.Sword,
        _ => CurrentTool.Type
    };

    if (appropriate != CurrentTool.Type)
        SelectTool(appropriate);
}
```

### Tool Abilities

```csharp
public abstract class ToolAbility
{
    public abstract float ChargeTime { get; }
    public abstract float Range { get; }
    public abstract void Execute(Vector3 position, float rangeMod);
}

public class SwordAbility : ToolAbility
{
    // Wide arc slash, high damage
    public override float ChargeTime => 1.0f;
    public override float Range => 3f;
}

public class PickaxeAbility : ToolAbility
{
    // Ground pound, damages + gathers
    public override float ChargeTime => 1.2f;
    public override float Range => 2.5f;
}

public class AxeAbility : ToolAbility
{
    // Spinning attack, 360 degrees
    public override float ChargeTime => 1.0f;
    public override float Range => 2f;
}
```

### Upgrade Requirements

```csharp
public Dictionary<ResourceType, int> GetUpgradeCost(ToolType type, int currentLevel)
{
    int nextLevel = currentLevel + 1;
    var cost = new Dictionary<ResourceType, int>();

    // Base materials scale with level
    cost[GetWoodType()] = 10 * nextLevel;
    cost[GetMetalType()] = 5 * nextLevel;

    // Higher tiers require gems
    if (nextLevel >= 3) cost[ResourceType.Ruby] = nextLevel;
    if (nextLevel >= 5) cost[ResourceType.Amethyst] = nextLevel;
    if (nextLevel >= 7) cost[ResourceType.Diamond] = nextLevel / 2;

    return cost;
}
```

## Visual Representation

- Tool model visible in player hand
- Color changes with tier (Red/Green/Purple/Black)
- Glow effect during charge
- Unique ability VFX per tool

## Implementation Checklist

- [ ] Create ToolData resource class
- [ ] Create ToolManager
- [ ] Implement tool switching (manual V key)
- [ ] Implement auto-switch based on context
- [ ] Add tool setting to options menu
- [ ] Create tool models for each tier
- [ ] Implement charged abilities per tool
- [ ] Add ability VFX
- [ ] Connect to upgrade station
- [ ] Calculate upgrade costs by world
- [ ] Update HUD with current tool display
