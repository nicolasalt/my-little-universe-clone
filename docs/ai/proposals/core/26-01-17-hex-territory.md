---
title: Hex Territory System
status: merged
modifies: systems/territory/hex-grid.md
priority: P0
author: AI
created: 26-01-17
---

# Hex Territory System

World divided into hexagonal tiles that players unlock by spending resources.

## Summary

Implement the hex-based territory expansion system. Players start in a small revealed area and spend gathered resources to unlock adjacent hexes, revealing new content.

## Design

### Hex Tile Data

```csharp
public enum HexState { Locked, Unlocking, Unlocked }

[GlobalClass]
public partial class HexTile : Resource
{
    public Vector2I Coordinates;
    public HexState State = HexState.Locked;
    public Dictionary<ResourceType, int> UnlockCost;
    public Dictionary<ResourceType, int> PaidAmount;
    public PackedScene[] ContentPrefabs;  // What spawns when unlocked
}
```

### Hex Grid Manager

```csharp
public partial class HexGridManager : Node3D
{
    [Export] public float HexSize = 5f;  // Radius in meters

    private Dictionary<Vector2I, HexTile> _tiles;

    public Vector2I WorldToHex(Vector3 worldPos);
    public Vector3 HexToWorld(Vector2I hexCoord);
    public List<Vector2I> GetNeighbors(Vector2I hexCoord);
    public List<Vector2I> GetUnlockableHexes();

    public void UnlockHex(Vector2I coord);
    public void PayTowardsHex(Vector2I coord, Dictionary<ResourceType, int> payment);
    public bool IsUnlocked(Vector2I coord);
}
```

### Hex Visual

```csharp
public partial class HexVisual : Node3D
{
    [Export] public HexTile TileData;

    // Visual states
    private MeshInstance3D _fogMesh;      // Dark overlay for locked
    private Node3D _contentContainer;      // Spawned content
    private Control _unlockUI;             // Cost display

    public void UpdateVisualState();
    public void ShowUnlockCost();
    public void PlayUnlockAnimation();
    public void SpawnContent();
}
```

### Hex Coordinate System

Using offset coordinates (odd-q):

```csharp
public static Vector2I[] GetNeighborOffsets(int col)
{
    // Odd columns have different neighbor offsets than even
    if (col % 2 == 1) // odd
        return new[] {
            new Vector2I(1, 0), new Vector2I(1, -1), new Vector2I(0, -1),
            new Vector2I(-1, -1), new Vector2I(-1, 0), new Vector2I(0, 1)
        };
    else // even
        return new[] {
            new Vector2I(1, 1), new Vector2I(1, 0), new Vector2I(0, -1),
            new Vector2I(-1, 0), new Vector2I(-1, 1), new Vector2I(0, 1)
        };
}
```

### Unlock Flow

1. Player approaches locked hex adjacent to unlocked area
2. UI shows unlock cost
3. Player delivers resources (automatic when in range)
4. Progress bar fills
5. When complete:
   - Fog clears with animation
   - Content spawns (resources, enemies, structures)
   - Adjacent locked hexes become visible

### Cost Scaling

```csharp
// Base cost increases with distance from origin
int distanceFromStart = HexDistance(coord, Vector2I.Zero);
int scaledCost = baseCost * (1 + distanceFromStart / 5);
```

## Visual Design

- Locked: Dark fog/shadow covering hex
- Unlocking: Fog with progress indicator
- Unlocked: Clear, fully visible content
- Border: Subtle hex outline on ground

## Implementation Checklist

- [x] Create HexTile resource class
- [x] Implement HexGridManager with coordinate math
- [x] Create hex mesh for ground tiles
- [x] Implement fog of war visual (color-based MVP)
- [x] Create unlock cost UI overlay
- [x] Implement payment system
- [x] Add unlock animation
- [x] Implement content spawning on unlock
- [x] Create hex editor tool for level design
- [x] Add adjacency check for unlocking
- [ ] Save/load hex state → Deferred to Phase 2 (Save System)
