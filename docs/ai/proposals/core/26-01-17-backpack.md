---
title: Backpack Inventory System
status: review
modifies: systems/resources/backpack.md
priority: P0
author: AI
created: 26-01-17
---

# Backpack Inventory System

Unlimited inventory for collected resources.

## Summary

Implement the backpack system that stores all gathered resources with no capacity limit. This keeps gameplay flowing without inventory management interruptions.

## Design

### Backpack Class

```csharp
public partial class Backpack : Node
{
    private Dictionary<ResourceType, int> _resources = new();

    [Signal] public delegate void ResourceAddedEventHandler(ResourceType type, int amount, int newTotal);
    [Signal] public delegate void ResourceRemovedEventHandler(ResourceType type, int amount, int newTotal);
    [Signal] public delegate void ResourcesChangedEventHandler();

    public void Add(ResourceType type, int amount)
    {
        _resources.TryGetValue(type, out int current);
        _resources[type] = current + amount;
        EmitSignal(SignalName.ResourceAdded, (int)type, amount, _resources[type]);
        EmitSignal(SignalName.ResourcesChanged);
    }

    public bool TrySpend(Dictionary<ResourceType, int> cost)
    {
        if (!HasResources(cost)) return false;
        foreach (var (type, amount) in cost)
        {
            _resources[type] -= amount;
            EmitSignal(SignalName.ResourceRemoved, (int)type, amount, _resources[type]);
        }
        EmitSignal(SignalName.ResourcesChanged);
        return true;
    }

    public int GetCount(ResourceType type)
    {
        return _resources.TryGetValue(type, out int count) ? count : 0;
    }

    public bool HasResources(Dictionary<ResourceType, int> required)
    {
        foreach (var (type, amount) in required)
        {
            if (GetCount(type) < amount) return false;
        }
        return true;
    }

    public Dictionary<ResourceType, int> GetAll() => new(_resources);
}
```

### Resource Type Definition

```csharp
public enum ResourceType
{
    // Universal
    Wood, Rock, Planks, Steel, Coins, Ruby,

    // Gems
    Amethyst, Azurite, Uranium, Sulphur, Diamond,

    // Trollheim
    Coal, Wheat, Leather, Wool, Bone, Lava,

    // Dimidium
    Crab, GhostStone, Meat,

    // Factorium
    Plasma, Scrolls, Gear, AlienEggShell,

    // Wadirum
    Acorn, Candy, Chocolate, Petal, ChristmasBall, Gift,

    // Odysseum
    Marble, Clay, Fish, Limestone, GreekStone,

    // Dragonora
    Bamboo, Hardwood,

    // Egyptium
    Sandstone, ProcessedSandstone,

    // Asium
    CopperOre, CopperIngots,

    // Refined
    RefinedAmethyst, RefinedAzurite, RefinedRuby, RefinedSulphur, RefinedUranium,

    // Keys
    GoldKey, RedKey, GreenKey, BlueKey,

    // Special
    SourceCore, EnergyCrystal, QuestToken
}
```

### Resource Database

```csharp
[GlobalClass]
public partial class ResourceData : Resource
{
    [Export] public ResourceType Type;
    [Export] public string DisplayName;
    [Export] public Texture2D Icon;
    [Export] public ResourceCategory Category;
    [Export] public int[] AvailableInWorlds;  // empty = all
    [Export] public bool AffectedByGemCollector;
}
```

### HUD Integration

```csharp
public partial class ResourceDisplay : Control
{
    private Dictionary<ResourceType, ResourceCounter> _counters;

    public void OnResourceChanged(ResourceType type, int newAmount);
    public void ShowResourcePopup(ResourceType type, int amount, Vector3 worldPos);
    public void SetRelevantResources(ResourceType[] types);
}
```

## Persistence

```csharp
public partial class Backpack
{
    public Dictionary<string, int> Serialize()
    {
        var data = new Dictionary<string, int>();
        foreach (var (type, amount) in _resources)
            data[type.ToString()] = amount;
        return data;
    }

    public void Deserialize(Dictionary<string, int> data)
    {
        _resources.Clear();
        foreach (var (key, amount) in data)
            if (Enum.TryParse<ResourceType>(key, out var type))
                _resources[type] = amount;
    }
}
```

## Implementation Checklist

- [ ] Create ResourceType enum
- [ ] Create ResourceData resource class
- [ ] Define all resource data files
- [ ] Implement Backpack class
- [ ] Create resource counter UI component
- [ ] Add resource popup on gather
- [ ] Implement resource display in HUD
- [ ] Add inventory screen (I key)
- [ ] Implement serialization for save/load
- [ ] Connect backpack to gathering system
- [ ] Add visual for world-unavailable resources (grayed out)
