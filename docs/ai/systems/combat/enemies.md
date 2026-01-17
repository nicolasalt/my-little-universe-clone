# Enemy System

150+ enemy types across all worlds.

## Enemy Base Properties

```csharp
class Enemy : CharacterBody3D
{
    string Id;
    string DisplayName;
    int MaxHealth;
    int Damage;
    float AttackSpeed;
    float MoveSpeed;
    float AggroRange;
    float AttackRange;
    List<Drop> DropTable;
    int XPReward;
    bool IsBoss;
}
```

## Drop System

```csharp
class Drop
{
    ResourceType Resource;
    int MinAmount;
    int MaxAmount;
    float DropChance;  // 0.0 - 1.0
}
```

## Enemies by World

### Gaia (World 1)

| Enemy | Health | Drops |
|-------|--------|-------|
| Goblin | 2 | Steel, Wood, Rock |
| Log Monster | 4 | Plank, Wood |
| Red Soldier | 6 | Rock, Coins |
| Master Soldier | 8 | Amethyst, Coins |
| Fungus Man | 12 | Shroom |
| Yeti | 16 | Meat |
| Skeleton Pirate | 20 | Ruby, Coins |

### Trollheim (World 2)

| Enemy | Health | Notable Drops |
|-------|--------|---------------|
| Scarecrow | 30 | Wheat, Plank |
| Bear | 50-70 | Leather |
| Spider | 120 | Coal |
| Mountain Troll | 400-1000 | Wool, Lava |
| **Spider Queen** (Boss) | 500 | Coal |

### Dimidium (World 3)

| Enemy | Health | Notable Drops |
|-------|--------|---------------|
| Crab | 75-100 | Crab |
| Ghost Pirate | 100-150 | Ghost Stone, Uranium |
| Shark | 150-200 | Meat |
| **Kraken** (Boss) | 300x5 | Special |

### Later Worlds
Enemies scale up significantly in health (1000+) and drops become more valuable.

## Boss Enemies

- Much higher health pools (500 - 50,000+)
- Unique attack patterns
- Ignore Pacifist booster
- Drop rare resources and quest items
- Often guard dungeon exits or portal components

## AI States

```csharp
enum EnemyState { Idle, Patrol, Chase, Attack, Return, Dead }
```

## Spawn System

- Enemies spawn from spawn points in unlocked hexes
- Respawn after timer when defeated
- Spawn rate and type varies by hex

## Changelog
- 26-01-17: Initial design for My Little Universe clone
