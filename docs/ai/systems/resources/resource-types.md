# Resource Types

All gatherable resources in the game (70+ types).

## Resource Categories

### Universal Resources
Available in all worlds.

| Resource | Source | Used For |
|----------|--------|----------|
| Wood | Trees | Planks, construction |
| Rock/Stone | Rock deposits | Construction |
| Planks | Sawmill (from Wood) | Upgrades, construction |
| Steel/Iron | Smelter (from Iron Ore) | Tool/armor upgrades |
| Coins | Enemies, deposits | Purchases, upgrades |
| Ruby | Gem deposits | Tool/armor upgrades |

### Gems and Crystals
Affected by Gem Collector booster.

| Resource | Rarity |
|----------|--------|
| Amethyst | Common |
| Azurite | Common |
| Ruby | Uncommon |
| Sulphur | Uncommon |
| Uranium | Rare |
| Diamond | Very Rare |

### Refined Resources
Created at refinery stations.

| Refined | Input | Facility |
|---------|-------|----------|
| Refined Amethyst | Amethyst | Refinery |
| Refined Azurite | Azurite | Refinery |
| Refined Ruby | Ruby | Refinery |
| Refined Sulphur | Sulphur | Refinery |
| Refined Uranium | Uranium | Refinery |

### World-Specific Resources

| World | Unique Resources |
|-------|-----------------|
| Gaia | Basic resources only |
| Trollheim | Coal, Wheat, Leather, Wool, Bone, Lava |
| Dimidium | Crab, Ghost Stone, Meat |
| Factorium | Plasma, Scrolls, Gear, Alien Egg Shell |
| Wadirum | Acorn, Candy, Chocolate, Petal, Christmas Ball, Gift |
| Odysseum | Marble, Clay, Fish, Limestone, Greek Stone |
| Dragonora | Bamboo, Hardwood |
| Egyptium | Sandstone, Processed Sandstone |
| Asium | Copper Ore, Copper Ingots |

### Key Items

| Item | Purpose |
|------|---------|
| Gold Key | Unlock gold gates in dungeons |
| Red Key | Unlock red gates in dungeons |
| Green Key | Unlock green gates in dungeons |
| Blue Key | Unlock blue gates in dungeons |
| Source Core | Portal construction |
| Energy Crystal | Special currency |

## Data Structure

```csharp
enum ResourceCategory { Universal, Gem, Refined, WorldSpecific, KeyItem }

class ResourceType : Resource
{
    string Id;
    string DisplayName;
    Texture2D Icon;
    ResourceCategory Category;
    int[] AvailableInWorlds;  // empty = all worlds
    bool AffectedByGemCollector;
}
```

## Implementation Status: ✅ Basic types implemented

**File:** `scripts/resources/ResourceType.cs`

Currently implemented as simple enum with helper class:
```csharp
public enum ResourceType { Wood, Stone, IronOre, Coins, Gems, Planks, Steel }
public enum ToolType { None, Axe, Pickaxe, Sword }

public static class ResourceInfo
{
    ToolType GetRequiredTool(ResourceType type);
    string GetDisplayName(ResourceType type);
}
```

Additional resource types will be added as worlds are implemented.

## Changelog
- 26-01-17: Initial design for My Little Universe clone
- 26-01-17: Implemented basic ResourceType enum with 7 types and helper class
