# Gathering System

Automatic resource collection when near harvestable objects.

## Behavior

1. Player enters gather range of a resource node
2. Appropriate tool is selected (auto or manual)
3. Gathering animation plays
4. Resources added to backpack at gather rate
5. Node depletes and respawns after timer

## Gather Range
- Detected via Area3D on player
- Default radius: 1.5m
- Player faces nearest resource while gathering

## Gather Speed
Base gather speed modified by:
- Tool level (each level +10% speed)
- Gatherer booster (+10% per stack)
- Resource type difficulty

## Tool Selection

| Resource Type | Tool |
|--------------|------|
| Trees, Wood | Axe |
| Rocks, Ore, Gems | Pickaxe |
| Enemies | Sword |
| Other (coins, drops) | Any (auto-pickup) |

## Resource Nodes

```csharp
class ResourceNode : StaticBody3D
{
    ResourceType Type;
    int Quantity;           // hits to deplete
    int YieldPerHit;        // base amount per gather
    float RespawnTime;      // seconds until regrow
    bool IsDepleted;
}
```

## Resource Respawning
- Trees and rocks respawn over time
- Respawn timer starts when depleted
- Visual: grows back from small to full size

## Charged Gathering
- Hold attack to charge area gather
- Releases AoE that gathers all resources in range
- More efficient for clustered resources

## Changelog
- 26-01-17: Initial design for My Little Universe clone
