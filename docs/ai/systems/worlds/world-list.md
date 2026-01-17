# World List

10 unique mythical worlds with distinct themes.

## Worlds

| # | Name | Theme | Special Features |
|---|------|-------|------------------|
| 0 | Hades | Tutorial/Fiery | Cannot return after completion |
| 1 | Gaia | Earth | Starting world, core mechanics |
| 2 | Trollheim | Norse | Trolls, spiders, lava areas |
| 3 | Dimidium | Ocean/Islands | Underwater dungeons, pirates, Kraken |
| 4 | Factorium | Mechanical/Factory | Robots, puzzles, Alien Queen |
| 5 | Wadirum | Winter/Fairy Tale | Christmas themes, daily tasks unlock |
| 6 | Odysseum | Greek Mythology | Poseidon, Medusa, Minotaur, Cerberus |
| 7 | Dragonora | Asian/Dragons | Bamboo, dragon enemies |
| 8 | Egyptium | Egyptian | Pyramids, mummies |
| 9 | Asium | Volcanic | Copper mining (grindy) |

## World Properties

```csharp
class WorldData : Resource
{
    int WorldId;
    string Name;
    string Theme;
    List<ResourceType> AvailableResources;
    List<EnemyType> EnemyTypes;
    Dictionary<ResourceType, int> PortalCost;
    int RecommendedToolLevel;
    bool CanReturn;  // false only for Hades
}
```

## Unique Resources by World

See `resource-types.md` for complete list.

## Boss Progression

| World | Notable Bosses |
|-------|---------------|
| Trollheim | Spider Queen |
| Dimidium | Kraken, Cyclop |
| Factorium | Alien Queen |
| Odysseum | Medusa, Minotaur, Poseidon, Cerberus |

## Dungeon Count

65+ total dungeons spread across all worlds.

## Changelog
- 26-01-17: Initial design for My Little Universe clone
