# Portal System

Travel between worlds via constructed portals.

## Portal Construction

Each world ends with constructing a portal to the next world:

1. Gather specific resources (varies by world)
2. Collect Source Cores from dungeons
3. Deliver to portal construction site
4. Portal activates when complete

## Portal Properties

```csharp
class Portal : Area3D
{
    int DestinationWorldId;
    Dictionary<ResourceType, int> ConstructionCost;
    Dictionary<ResourceType, int> PaidSoFar;
    bool IsComplete;
    bool IsActive;
}
```

## World Travel

After completing a portal:
- Can freely travel to any previously visited world
- Use portal or world map (M key)
- Resources stay in origin world (can't transfer)
- Tool/armor upgrades are global

## Portal UI

When approaching incomplete portal:
- Shows required resources
- Shows current progress
- Highlights missing materials

## Progression Rules

- Must complete worlds in order
- Cannot skip ahead
- Can return to farm resources anytime
- Some resources only available in specific worlds

## Changelog
- 26-01-17: Initial design for My Little Universe clone
