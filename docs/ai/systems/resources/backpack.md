# Backpack System

Unlimited inventory for collected resources.

## Implementation Status: ✅ Implemented

## Design

The backpack holds all gathered resources with no capacity limit. This is a core MLU mechanic that keeps gameplay flowing without inventory management interruptions.

## Implementation

**File:** `scripts/resources/Backpack.cs`
**Location:** Child node of Player

```csharp
public partial class Backpack : Node
{
    Dictionary<ResourceType, int> _resources;

    void Add(ResourceType type, int amount);
    int GetCount(ResourceType type);
    bool Has(ResourceType type, int amount);
    bool HasResources(Dictionary<ResourceType, int> required);
    bool TrySpend(Dictionary<ResourceType, int> cost);
    bool TrySpend(ResourceType type, int amount);
}
```

## Signals
- Emits `ResourceChanged(int resourceType, int newTotal)` via SignalBus on any change

## Not Yet Implemented
- Persistence (save/load)
- Per-world isolation
- UI integration (inventory screen)

## Changelog
- 26-01-17: Initial design for My Little Universe clone
- 26-01-17: Implemented Backpack node with add/spend/query methods
