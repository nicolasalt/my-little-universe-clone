---
title: Resource Gathering System
status: review
modifies: systems/resources/gathering.md, systems/player/controller.md
priority: P0
author: AI
created: 26-01-17
---

# Resource Gathering System

Core mechanic for collecting resources from the world.

## Summary

Implement automatic resource gathering when player stands near harvestable objects. This is the foundational mechanic that drives all progression.

## Design

### Resource Node Component

```csharp
public partial class ResourceNode : StaticBody3D
{
    [Export] public ResourceType Type;
    [Export] public int TotalHits = 3;
    [Export] public int YieldPerHit = 1;
    [Export] public float RespawnTime = 30f;

    private int _remainingHits;
    private bool _isDepleted;

    public void OnGathered(float yieldMultiplier);
    public void Deplete();
    public void Respawn();
}
```

### Gather Area (Player)

```csharp
// Area3D on player that detects nearby resources
public partial class GatherArea : Area3D
{
    [Export] public float GatherRadius = 1.5f;

    public ResourceNode GetNearestResource();
    public List<ResourceNode> GetResourcesInRange();
}
```

### Gather Controller

```csharp
public partial class GatherController : Node
{
    [Export] public float BaseGatherRate = 1.0f;  // gathers per second

    private ResourceNode _currentTarget;
    private float _gatherTimer;

    public void StartGathering(ResourceNode node);
    public void StopGathering();
    private void ProcessGather(double delta);
}
```

### Gather Flow

1. Player enters range of ResourceNode
2. GatherArea detects node and reports to GatherController
3. Appropriate tool selected (auto or manual mode)
4. Player rotates to face resource
5. Gather animation plays at gather rate
6. Each gather hit:
   - Reduces node hits remaining
   - Adds resources to backpack
   - Plays particle effect
7. When depleted:
   - Node becomes inactive
   - Respawn timer starts
   - Node visually shrinks/disappears
8. On respawn:
   - Node grows back
   - Becomes harvestable again

### Gather Rate Modifiers

```csharp
float effectiveRate = BaseGatherRate
    * (1 + toolLevel * 0.1f)        // +10% per tool level
    * (1 + gathererStacks * 0.1f);  // +10% per Gatherer card
```

## Visual Feedback

- Gather swing animation on player
- Hit particles on resource
- Resource counter popup (+3 Wood)
- Resource shrinks as depleted
- Growth animation on respawn

## Implementation Checklist

- [ ] Create ResourceNode scene and script
- [ ] Create ResourceType enum and resource definitions
- [ ] Add GatherArea to Player scene
- [ ] Implement GatherController
- [ ] Add gather animation to player
- [ ] Implement tool selection (auto mode)
- [ ] Add particle effects for gathering
- [ ] Implement resource respawning
- [ ] Connect to backpack system
- [ ] Add gather rate modifiers
- [ ] Create first set of resource nodes (tree, rock, ore)
