---
title: World 1 - Gaia
status: draft
modifies: systems/worlds/world-list.md
priority: P2
author: AI
created: 26-01-17
---

# World 1 - Gaia (Earth)

First playable world after tutorial, introduces core mechanics.

## Summary

Design and implement Gaia, the starting world that teaches players all core mechanics while providing a gentle difficulty curve.

## Design

### World Overview

| Property | Value |
|----------|-------|
| World ID | 1 |
| Theme | Natural Earth |
| Biomes | Forest, Grassland, Rocky Hills, Cave |
| Recommended Tool Level | 1-3 |
| Portal Destination | Trollheim |

### Hex Layout

```
Starting Area:
- 7 hex "safe zone" (no enemies)
- Tutorial prompts
- First sawmill, smelter
- Upgrade station

Expansion Zones (by distance):
- Ring 1: Basic resources, weak enemies
- Ring 2: Medium resources, stronger enemies, first dungeon
- Ring 3: Advanced resources, dungeon with Source Core
- Ring 4: Portal construction site
```

### Resources

| Resource | Location | Abundance |
|----------|----------|-----------|
| Wood | Everywhere | Very High |
| Rock | Everywhere | High |
| Iron Ore | Ring 2+ | Medium |
| Ruby | Ring 3+ | Low |
| Amethyst | Dungeons | Low |
| Azurite | Caves | Low |

### Enemies

| Enemy | Health | Location | Drops |
|-------|--------|----------|-------|
| Goblin | 2 | Ring 1 | Steel, Wood, Coins |
| Log Monster | 4 | Forest | Plank, Wood |
| Red Soldier | 6 | Ring 2 | Rock, Coins |
| Master Soldier | 8 | Ring 2+ | Amethyst, Coins |
| Fungus Man | 12 | Caves | Shroom |
| Yeti | 16 | Ring 3 | Meat |
| Skeleton Pirate | 20 | Dungeons | Ruby, Coins |

### Structures

```csharp
// Structure placements in Gaia
var structures = new[] {
    // Starting area
    new Structure { Type = "Sawmill", HexCoord = (1, 0) },
    new Structure { Type = "Smelter", HexCoord = (0, 1) },
    new Structure { Type = "UpgradeStation", HexCoord = (-1, 0) },

    // Ring 2
    new Structure { Type = "DungeonEntrance", HexCoord = (3, 2), DungeonId = "gaia_cave_1" },

    // Ring 3
    new Structure { Type = "DungeonEntrance", HexCoord = (-4, 3), DungeonId = "gaia_dungeon_main" },

    // Ring 4
    new Structure { Type = "PortalSite", HexCoord = (5, 0) }
};
```

### Dungeons

#### Cave Dungeon 1
- Type: Resource
- Enemies: Goblins, Fungus Men
- Rewards: Iron Ore, Azurite
- No boss

#### Main Dungeon
- Type: Combat + Resource
- Enemies: All Gaia types
- Mini-boss: Master Soldier (x3)
- Contains: Source Core
- Rewards: Ruby, Amethyst, Steel

### Portal Requirements

```csharp
var gaiaPortalCost = new Dictionary<ResourceType, int>
{
    { ResourceType.Wood, 100 },
    { ResourceType.Planks, 50 },
    { ResourceType.Steel, 50 },
    { ResourceType.Ruby, 10 },
    { ResourceType.SourceCore, 2 }
};
```

### Progression Flow

1. **Tutorial Zone** (starting hexes)
   - Learn movement
   - Learn gathering
   - Build sawmill
   - Build smelter

2. **First Expansion**
   - Fight first enemies (Goblins)
   - Upgrade sword to level 2
   - Find upgrade station

3. **Mid Game**
   - Clear first dungeon
   - Gather iron ore
   - Upgrade tools to level 3-4

4. **Portal Push**
   - Clear main dungeon for Source Cores
   - Gather portal resources
   - Build portal to Trollheim

### Environment Art

- **Terrain**: Green grass, dirt paths, rocky outcrops
- **Trees**: Oak-style with thick trunks
- **Rocks**: Gray boulders, ore veins have color tint
- **Lighting**: Warm daylight, slight golden hour feel
- **Skybox**: Blue sky with fluffy clouds

## Implementation Checklist

- [ ] Create Gaia world scene
- [ ] Design hex layout (50-80 hexes)
- [ ] Place resource nodes
- [ ] Set up enemy spawners
- [ ] Place facility construction sites
- [ ] Create cave dungeon
- [ ] Create main dungeon
- [ ] Set up portal site
- [ ] Add environmental art
- [ ] Create loading zones between areas
- [ ] Test full progression flow
