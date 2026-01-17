---
title: Enemy System
status: draft
modifies: systems/combat/enemies.md
priority: P2
author: AI
created: 26-01-17
---

# Enemy System

150+ enemy types with AI, drops, and combat behavior.

## Summary

Implement the enemy system with spawning, AI behavior, combat, and loot drops.

## Design

### Enemy Base

```csharp
public partial class Enemy : CharacterBody3D
{
    [Export] public EnemyData Data;

    private HealthComponent _health;
    private EnemyAI _ai;
    private DropHandler _dropHandler;

    public void TakeDamage(int amount);
    public void Die();
    public void OnPlayerEnterAggro();
}
```

### Enemy Data Resource

```csharp
[GlobalClass]
public partial class EnemyData : Resource
{
    [Export] public string Id;
    [Export] public string DisplayName;
    [Export] public int MaxHealth;
    [Export] public int Damage;
    [Export] public float AttackSpeed = 1f;
    [Export] public float MoveSpeed = 3f;
    [Export] public float AggroRange = 8f;
    [Export] public float AttackRange = 1.5f;
    [Export] public int XPReward;
    [Export] public bool IsBoss;
    [Export] public bool IgnoresPacifist;
    [Export] public Drop[] DropTable;
    [Export] public PackedScene Prefab;
}
```

### Drop System

```csharp
[Serializable]
public class Drop
{
    public ResourceType Resource;
    public int MinAmount;
    public int MaxAmount;
    public float Chance = 1f;  // 0-1
}

public partial class DropHandler : Node
{
    public void SpawnDrops(EnemyData data, Vector3 position)
    {
        foreach (var drop in data.DropTable)
        {
            if (GD.Randf() <= drop.Chance)
            {
                int amount = GD.RandRange(drop.MinAmount, drop.MaxAmount);
                amount = ApplyBountyHunter(amount, drop.Resource);
                SpawnPickup(drop.Resource, amount, position);
            }
        }
    }

    private int ApplyBountyHunter(int amount, ResourceType type)
    {
        if (type == ResourceType.Coins)
        {
            float bonus = boosterCards.GetStatModifier(StatType.CoinDropBonus);
            return (int)(amount * (1 + bonus));
        }
        return amount;
    }
}
```

### Enemy AI

```csharp
public enum EnemyState { Idle, Patrol, Chase, Attack, Return, Dead }

public partial class EnemyAI : Node
{
    private EnemyState _state = EnemyState.Idle;
    private Vector3 _spawnPosition;
    private Node3D _target;

    public void Process(double delta)
    {
        switch (_state)
        {
            case EnemyState.Idle:
                CheckForPlayer();
                break;
            case EnemyState.Chase:
                MoveTowardTarget();
                CheckAttackRange();
                CheckReturnDistance();
                break;
            case EnemyState.Attack:
                PerformAttack();
                break;
            case EnemyState.Return:
                MoveToSpawn();
                break;
        }
    }

    private void CheckForPlayer()
    {
        // Skip aggro if player has Pacifist and enemy not immune
        if (player.HasPacifist && !enemy.Data.IgnoresPacifist)
            return;

        float dist = GlobalPosition.DistanceTo(player.GlobalPosition);
        if (dist <= enemy.Data.AggroRange)
        {
            _target = player;
            _state = EnemyState.Chase;
        }
    }
}
```

### Spawn System

```csharp
public partial class EnemySpawner : Node3D
{
    [Export] public EnemyData[] PossibleEnemies;
    [Export] public float RespawnTime = 30f;
    [Export] public int MaxActive = 3;

    private List<Enemy> _activeEnemies = new();

    public void OnHexUnlocked();
    public void SpawnEnemy();
    public void OnEnemyDied(Enemy enemy);
}
```

### Example Enemy Definitions

```csharp
// Gaia enemies
new EnemyData {
    Id = "goblin",
    DisplayName = "Goblin",
    MaxHealth = 2,
    Damage = 5,
    XPReward = 2,
    DropTable = new[] {
        new Drop { Resource = ResourceType.Steel, MinAmount = 1, MaxAmount = 2, Chance = 0.5f },
        new Drop { Resource = ResourceType.Wood, MinAmount = 1, MaxAmount = 3, Chance = 0.7f },
        new Drop { Resource = ResourceType.Coins, MinAmount = 1, MaxAmount = 5, Chance = 1f }
    }
};

new EnemyData {
    Id = "yeti",
    DisplayName = "Yeti",
    MaxHealth = 16,
    Damage = 10,
    XPReward = 8,
    DropTable = new[] {
        new Drop { Resource = ResourceType.Meat, MinAmount = 2, MaxAmount = 4, Chance = 1f }
    }
};
```

## Visual Design

- Each enemy type has unique model/sprite
- Damage flash on hit
- Death dissolve animation
- Aggro indicator (red eyes/exclamation)

## Implementation Checklist

- [ ] Create Enemy base class
- [ ] Implement EnemyData resource
- [ ] Create EnemyAI with state machine
- [ ] Implement drop system
- [ ] Create EnemySpawner
- [ ] Add Pacifist booster check
- [ ] Define Gaia enemy set
- [ ] Create enemy models/animations
- [ ] Add damage VFX
- [ ] Implement respawn system
