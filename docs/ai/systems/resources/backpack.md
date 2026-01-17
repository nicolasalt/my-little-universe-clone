# Backpack System

Unlimited inventory for collected resources.

## Design

The backpack holds all gathered resources with no capacity limit. This is a core MLU mechanic that keeps gameplay flowing without inventory management interruptions.

## Data Structure

```csharp
class Backpack
{
    Dictionary<ResourceType, int> Resources;

    void Add(ResourceType type, int amount);
    bool TrySpend(Dictionary<ResourceType, int> cost);
    int GetCount(ResourceType type);
    bool HasResources(Dictionary<ResourceType, int> required);
}
```

## Persistence
- Saved per-world (resources can't transfer between worlds)
- Auto-saved on resource changes

## UI Integration
- HUD shows relevant resources for current area
- Full inventory viewable via I key
- Resources grayed out if not available in current world

## Changelog
- 26-01-17: Initial design for My Little Universe clone
