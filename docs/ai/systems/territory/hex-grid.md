# Hex Grid Territory System

World divided into hexagonal tiles for exploration and expansion.

## Core Concept

Each world is composed of hex tiles:
- Start with small unlocked area
- Spend resources to unlock adjacent hexes
- Unlocking reveals new content (resources, enemies, structures)

## Hex States

```csharp
enum HexState { Locked, Unlocking, Unlocked }
```

### Locked
- Covered in darkness/fog
- Shows silhouette hints of content
- Displays unlock cost when player approaches

### Unlocking
- Player has started spending resources
- Shows progress bar
- Completes when all resources delivered

### Unlocked
- Fully revealed and accessible
- Content active (enemies spawn, resources gatherable)

## Unlock Costs

Costs scale with distance from start and world progression:

| World | Base Cost Range |
|-------|----------------|
| Gaia | 5-50 resources |
| Trollheim | 20-100 resources |
| Dimidium | 50-200 resources |
| Later worlds | 100-500+ resources |

Multiple resource types may be required per hex.

## Data Structure

```csharp
class HexTile
{
    Vector2I Coordinates;
    HexState State;
    Dictionary<ResourceType, int> UnlockCost;
    Dictionary<ResourceType, int> PaidSoFar;
    List<Node3D> Contents;  // enemies, resources, structures
    bool HasBeenRevealed;
}
```

## Hex Content Types

- Resource nodes (trees, rocks, ore deposits)
- Enemy spawn points
- Structures (facilities, upgrade stations)
- Dungeon entrances
- Portal components
- Decorative elements

## Adjacency
Hexes use offset coordinates. A hex has 6 neighbors.

## Changelog
- 26-01-17: Initial design for My Little Universe clone
