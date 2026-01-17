# Dungeon System

65+ dungeons across all worlds with unique challenges.

## Dungeon Types

### Resource Dungeons
- High concentration of specific resources
- Light enemy presence
- Good for farming

### Combat Dungeons
- Dense enemy spawns
- Mini-boss encounters
- Combat-focused rewards

### Puzzle Dungeons
- Button/lever mechanics
- Pressure plates
- Key-locked passages
- Introduced in Factorium

### Boss Dungeons
- Single powerful boss
- Unique arena
- Required for progression

## Dungeon Features

| Feature | Description |
|---------|-------------|
| Source Cores | Essential for portal construction |
| Colored Keys | Unlock matching gates within dungeon |
| Treasure Rooms | Bonus loot behind locked doors |
| Hazards | Fire, poison, water sections |

## Key System

| Key Color | Gate Color |
|-----------|------------|
| Gold | Gold |
| Red | Red |
| Green | Green |
| Blue | Blue |

Keys are dungeon-specific and don't carry over.

## Dungeon Data

```csharp
class Dungeon
{
    string Id;
    string DisplayName;
    int WorldId;
    DungeonType Type;
    int RecommendedLevel;
    List<Vector2I> HexLocations;
    List<EnemySpawn> Enemies;
    List<LootDrop> Rewards;
    bool HasBoss;
    bool HasSourceCore;
}
```

## Notable Dungeons by World

| World | Dungeons |
|-------|----------|
| Trollheim | Azurite Cavern, Spider Caves, Lava Islands |
| Dimidium | Underwater caves, Pirate hideouts, Kraken zone |
| Factorium | Mystery Keep, Engine Room, Alien Hive |
| Wadirum | Crunchy's Lair, Dead Forest, Chess areas |
| Odysseum | Poseidon's realm, Minotaur labyrinth |

## Timed Challenges

- Each world has timed challenges with objectives
- Cooldown of ~3 hours between attempts
- Reward Quest Tokens on completion
- Difficulty scales with progression

## Changelog
- 26-01-17: Initial design for My Little Universe clone
