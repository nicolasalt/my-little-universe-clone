---
title: Home World
status: draft
modifies: null
priority: P3
author: AI
created: 26-01-17
---

# Home World

Player-created custom world with idle resource generation.

## Summary

Implement the Home World feature where players can build and customize their own world with placeable decorations and workers for passive resource income.

## Design

### Home World Manager

```csharp
public partial class HomeWorld : Node3D
{
    [Export] public Vector2I GridSize = new(20, 20);

    private Dictionary<Vector2I, PlacedTile> _tiles;
    private List<Worker> _workers;

    public void PlaceTile(Vector2I coord, TileType type);
    public void PlaceDecoration(Vector2I coord, DecorationData decoration);
    public void AssignWorker(ResourceNodePlacement node);
    public Dictionary<ResourceType, float> GetPassiveIncome();
}
```

### Tile System

```csharp
public enum TileType
{
    Grass,
    Stone,
    Wood,
    Water,
    Sand,
    Snow
}

public partial class PlacedTile : Node3D
{
    public TileType Type;
    public Color Tint = Colors.White;
    public DecorationData Decoration;
}
```

### Decorations

```csharp
[GlobalClass]
public partial class DecorationData : Resource
{
    [Export] public string Id;
    [Export] public string DisplayName;
    [Export] public Texture2D Icon;
    [Export] public PackedScene Prefab;
    [Export] public DecorationCategory Category;
    [Export] public int UnlockCost;  // Stars required
    [Export] public bool CanHaveWorker;
    [Export] public ResourceType? GeneratesResource;
    [Export] public float BaseGenerationRate;  // per hour
}

public enum DecorationCategory
{
    Nature,      // Trees, flowers
    Structure,   // Buildings, fences
    Resource,    // Mines, farms (can have workers)
    Cosmetic     // Statues, fountains
}
```

### Worker System

```csharp
public partial class Worker : Node3D
{
    [Export] public WorkerType Type;

    public ResourceNodePlacement AssignedNode;
    public float EfficiencyMultiplier = 1f;

    public float GetOutputPerHour()
    {
        if (AssignedNode == null) return 0;
        return AssignedNode.Decoration.BaseGenerationRate * EfficiencyMultiplier;
    }
}

public partial class ResourceNodePlacement : Node3D
{
    [Export] public DecorationData Decoration;

    public Worker AssignedWorker;
    public int UpgradeLevel = 1;

    public float GetGenerationRate()
    {
        float baseRate = Decoration.BaseGenerationRate;
        float levelMod = 1 + (UpgradeLevel - 1) * 0.25f;
        float workerMod = AssignedWorker?.EfficiencyMultiplier ?? 0;
        return baseRate * levelMod * workerMod;
    }
}
```

### Idle Income

```csharp
public partial class IdleIncomeManager : Node
{
    private DateTime _lastCollectionTime;

    public void CalculateOfflineEarnings()
    {
        var elapsed = DateTime.Now - _lastCollectionTime;
        var hours = (float)elapsed.TotalHours;

        // Cap at 24 hours
        hours = Mathf.Min(hours, 24f);

        var income = homeWorld.GetPassiveIncome();
        foreach (var (resource, perHour) in income)
        {
            int earned = (int)(perHour * hours);
            if (earned > 0)
                player.Backpack.Add(resource, earned);
        }

        _lastCollectionTime = DateTime.Now;
    }
}
```

### Build Mode UI

```csharp
public partial class BuildModeUI : Control
{
    [Export] public TabContainer CategoryTabs;
    [Export] public GridContainer ItemGrid;
    [Export] public ColorPicker TileColorPicker;

    private DecorationData _selectedDecoration;
    private TileType _selectedTile;

    public void OnCategorySelected(DecorationCategory category);
    public void OnItemSelected(DecorationData decoration);
    public void OnPlaceConfirmed(Vector2I coord);
    public void OnTileColorChanged(Color color);
}
```

### Star Unlock System

```csharp
public partial class StarManager : Node
{
    public int TotalStars { get; private set; }
    public int SpentStars { get; private set; }
    public int AvailableStars => TotalStars - SpentStars;

    public void CollectStar();  // From world exploration
    public bool TrySpendStars(int amount);
}
```

## Visual Design

- Isometric grid view
- Colorable tiles with RGB picker
- Decorations snap to grid
- Workers visibly working at nodes
- Day/night cycle (cosmetic)

## Implementation Checklist

- [ ] Create HomeWorld scene
- [ ] Implement tile placement system
- [ ] Create decoration catalog
- [ ] Implement worker assignment
- [ ] Add idle income calculation
- [ ] Create build mode UI
- [ ] Implement star unlock system
- [ ] Add offline earnings popup
- [ ] Create tutorial for home world
- [ ] Save/load home world state
