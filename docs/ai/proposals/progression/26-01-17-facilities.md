---
title: Processing Facilities
status: review
modifies: systems/facilities/sawmill.md, systems/facilities/smelter.md
priority: P1
author: AI
created: 26-01-17
---

# Processing Facilities

Sawmill, Smelter, and Refinery for processing raw materials.

## Summary

Implement industrial facilities that process raw resources into refined materials needed for upgrades and construction.

## Design

### Base Facility

```csharp
public abstract partial class Facility : StaticBody3D
{
    [Export] public FacilityType Type;
    [Export] public float ProcessingTime = 2f;

    protected Queue<ProcessingJob> _jobQueue;
    protected ProcessingJob _currentJob;

    public abstract ResourceType[] AcceptedInputs { get; }
    public abstract Dictionary<ResourceType, ResourceType> OutputMap { get; }
    public abstract int InputToOutputRatio { get; }

    public void AddInput(ResourceType type, int amount);
    public bool CanAccept(ResourceType type);
    protected virtual void ProcessTick();
}
```

### Sawmill

```csharp
public partial class Sawmill : Facility
{
    public override ResourceType[] AcceptedInputs => new[] { ResourceType.Wood };
    public override Dictionary<ResourceType, ResourceType> OutputMap => new()
    {
        { ResourceType.Wood, ResourceType.Planks }
    };
    public override int InputToOutputRatio => 2;  // 2 Wood = 1 Plank
}
```

### Smelter

```csharp
public partial class Smelter : Facility
{
    public override ResourceType[] AcceptedInputs => new[]
    {
        ResourceType.IronOre,
        ResourceType.CopperOre
    };
    public override Dictionary<ResourceType, ResourceType> OutputMap => new()
    {
        { ResourceType.IronOre, ResourceType.Steel },
        { ResourceType.CopperOre, ResourceType.CopperIngots }
    };
    public override int InputToOutputRatio => 2;
}
```

### Refinery

```csharp
public partial class Refinery : Facility
{
    public override ResourceType[] AcceptedInputs => new[]
    {
        ResourceType.Amethyst,
        ResourceType.Azurite,
        ResourceType.Ruby,
        ResourceType.Sulphur,
        ResourceType.Uranium
    };
    // Maps each gem to its refined version
    public override int InputToOutputRatio => 3;
}
```

### Processing Job

```csharp
public class ProcessingJob
{
    public ResourceType Input;
    public ResourceType Output;
    public int Amount;
    public float Progress;
}
```

### Player Interaction

```csharp
// When player enters facility range:
// 1. Check backpack for accepted inputs
// 2. Automatically transfer applicable resources
// 3. Display processing status
// 4. Output items go to ground for pickup

public void OnPlayerEntered(Player player)
{
    var backpack = player.Backpack;
    foreach (var inputType in AcceptedInputs)
    {
        int available = backpack.GetCount(inputType);
        if (available > 0)
        {
            backpack.TrySpend(inputType, available);
            AddInput(inputType, available);
        }
    }
}
```

### Facility Construction

```csharp
public partial class FacilityBlueprint : Area3D
{
    [Export] public PackedScene FacilityScene;
    [Export] public Dictionary<ResourceType, int> ConstructionCost;

    private Dictionary<ResourceType, int> _paid;

    public void OnPlayerDeliverResources(Dictionary<ResourceType, int> resources);
    public void CompleteConstruction();
}
```

## Visual Design

- Input hopper (resources visibly go in)
- Processing animation (spinning saw, glowing furnace)
- Output pile (processed items appear)
- Construction scaffolding for blueprints

## Implementation Checklist

- [ ] Create base Facility class
- [ ] Implement Sawmill
- [ ] Implement Smelter
- [ ] Implement Refinery
- [ ] Create processing queue system
- [ ] Add auto-transfer from player backpack
- [ ] Create output pile mechanic
- [ ] Implement FacilityBlueprint for construction
- [ ] Add facility models with animations
- [ ] Create facility HUD (shows processing status)
- [ ] Place facilities in world hexes
