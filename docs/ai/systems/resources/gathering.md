# Gathering System

Automatic resource collection when near harvestable objects.

## Implementation Status: ✅ Implemented

## Behavior

1. Player enters gather range of a resource node
2. GatherController detects nearest resource via GatherArea
3. Player rotates to face the resource
4. Auto-gathering occurs at BaseGatherRate (1 gather/second)
5. Resources added to backpack, signals emitted
6. Node depletes and respawns after timer

## Components

### GatherArea (`scripts/player/GatherArea.cs`)
- Area3D on Player detecting nearby resources
- Default radius: 2.0m
- Collision mask: layer 2 (resource nodes)
- Methods: `GetNearestResource()`, `GetResourcesInRange()`, `HasResourcesInRange()`

### GatherController (`scripts/player/GatherController.cs`)
- Node on Player handling auto-gathering logic
- `BaseGatherRate`: 1.0 gathers/second (configurable)
- `YieldMultiplier`: 1.0 (for tool/booster bonuses)
- `AutoGather`: true by default
- Player faces target while gathering

### ResourceNode (`scripts/resources/ResourceNode.cs`)
- StaticBody3D placed in world
- Collision layer: 2 (for GatherArea detection)
- Visual node shrinks as hits decrease
- Respawns after RespawnTime

## Gather Speed Formula
```
effectiveRate = BaseGatherRate * (1 + toolLevel * 0.1) * (1 + gathererStacks * 0.1)
```
(Tool and booster bonuses not yet implemented)

## Tool Selection

| Resource Type | Required Tool |
|--------------|---------------|
| Wood | Axe |
| Stone | Pickaxe |
| IronOre | Pickaxe |
| Gems | Pickaxe |
| Coins | None |

## Resource Nodes

```csharp
public partial class ResourceNode : StaticBody3D
{
    [Export] public ResourceType Type;
    [Export] public int TotalHits = 3;
    [Export] public int YieldPerHit = 1;
    [Export] public float RespawnTime = 30f;

    public int OnGathered(float yieldMultiplier);
    public void Deplete();
    public void Respawn();
}
```

## Signals (SignalBus)
- `ResourceGathered(int resourceType, int amount)` - fired on each gather
- `ResourceChanged(int resourceType, int newTotal)` - fired when backpack updates
- `GatheringStarted(Node3D target)` - fired when targeting a new node
- `GatheringStopped()` - fired when no longer gathering

## Test Scenes
- `scenes/resources/Tree.tscn` - Wood resource (5 hits, 2 yield)
- `scenes/resources/Rock.tscn` - Stone resource (3 hits, 1 yield)
- Main scene has 3 trees and 3 rocks for testing

## Not Yet Implemented
- Gather animation on player
- Tool selection logic (currently gathers regardless of tool)
- Particle effects on hit
- Resource popup text (+3 Wood)
- Charged/AoE gathering

## Changelog
- 26-01-17: Initial design for My Little Universe clone
- 26-01-17: Implemented core gathering system (GatherArea, GatherController, ResourceNode, Backpack)
