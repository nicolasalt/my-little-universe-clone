---
title: Boss System
status: draft
modifies: systems/combat/enemies.md, systems/worlds/dungeons.md
priority: P2
author: AI
created: 26-01-17
---

# Boss System

Powerful boss enemies with unique mechanics.

## Summary

Implement boss encounters with high health pools, special attack patterns, and valuable rewards.

## Design

### Boss Base

```csharp
public partial class Boss : Enemy
{
    [Export] public BossData BossInfo;

    private int _currentPhase;
    private BossPhase[] _phases;
    private BossHealthBar _healthBar;

    public override void _Ready()
    {
        base._Ready();
        _phases = BossInfo.Phases;
        ShowBossHealthBar();
    }

    protected override void Die()
    {
        HideBossHealthBar();
        SignalBus.EmitSignal(SignalBus.SignalName.BossDefeated, this);
        base.Die();
    }
}
```

### Boss Data

```csharp
[GlobalClass]
public partial class BossData : EnemyData
{
    [Export] public string Title;  // "Spider Queen"
    [Export] public BossPhase[] Phases;
    [Export] public PackedScene ArenaScene;
    [Export] public AudioStream BossMusic;
    [Export] public string[] MythicalDrops;  // Special item IDs
}

[Serializable]
public class BossPhase
{
    public float HealthThreshold;  // 0.75, 0.5, 0.25
    public string[] AttackPatterns;
    public float DamageMultiplier = 1f;
    public float SpeedMultiplier = 1f;
}
```

### Boss AI

```csharp
public partial class BossAI : EnemyAI
{
    private int _currentPhase;
    private float _attackTimer;
    private Queue<string> _attackQueue;

    protected override void Process(double delta)
    {
        CheckPhaseTransition();
        ProcessAttackPattern(delta);
        base.Process(delta);
    }

    private void CheckPhaseTransition()
    {
        float healthPercent = _health.Current / _health.Max;
        int newPhase = GetPhaseForHealth(healthPercent);

        if (newPhase != _currentPhase)
        {
            _currentPhase = newPhase;
            OnPhaseTransition();
        }
    }

    private void OnPhaseTransition()
    {
        // Visual/audio feedback
        PlayPhaseTransitionEffect();

        // Enrage or change patterns
        _attackQueue = new Queue<string>(
            _boss.BossInfo.Phases[_currentPhase].AttackPatterns
        );
    }
}
```

### Attack Patterns

```csharp
public abstract class BossAttack
{
    public abstract string Id { get; }
    public abstract float Cooldown { get; }
    public abstract float WindupTime { get; }

    public abstract void Execute(Boss boss, Player target);
}

public class GroundSlamAttack : BossAttack
{
    public override string Id => "ground_slam";
    public override float Cooldown => 5f;
    public override float WindupTime => 1f;

    public override void Execute(Boss boss, Player target)
    {
        // Show warning indicator
        // Delay
        // AoE damage around boss
    }
}

public class ChargeAttack : BossAttack
{
    public override string Id => "charge";
    public override float Cooldown => 8f;
    public override float WindupTime => 0.5f;

    public override void Execute(Boss boss, Player target)
    {
        // Lock onto player position
        // Rush forward
        // Damage on contact
    }
}

public class SummonMinionsAttack : BossAttack
{
    public override string Id => "summon";
    public override float Cooldown => 15f;
    public override float WindupTime => 2f;

    public override void Execute(Boss boss, Player target)
    {
        // Spawn 3-5 minions
    }
}
```

### Boss Health Bar

```csharp
public partial class BossHealthBar : Control
{
    [Export] public ProgressBar HealthBar;
    [Export] public Label NameLabel;
    [Export] public Label TitleLabel;

    public void SetBoss(Boss boss)
    {
        NameLabel.Text = boss.Data.DisplayName;
        TitleLabel.Text = boss.BossInfo.Title;
        boss.Health.HealthChanged += UpdateHealth;
    }

    private void UpdateHealth(float current, float max)
    {
        HealthBar.Value = current / max * 100;
    }
}
```

### Example Bosses

```csharp
// Trollheim - Spider Queen
new BossData {
    Id = "spider_queen",
    DisplayName = "Spider Queen",
    Title = "Matriarch of the Caves",
    MaxHealth = 500,
    Damage = 15,
    XPReward = 100,
    IgnoresPacifist = true,
    Phases = new[] {
        new BossPhase { HealthThreshold = 1f, AttackPatterns = new[] { "bite", "web_shot" } },
        new BossPhase { HealthThreshold = 0.5f, AttackPatterns = new[] { "bite", "web_shot", "summon_spiders" }, SpeedMultiplier = 1.3f }
    },
    DropTable = new[] {
        new Drop { Resource = ResourceType.Coal, MinAmount = 20, MaxAmount = 30, Chance = 1f }
    }
};

// Odysseum - Cerberus
new BossData {
    Id = "cerberus",
    DisplayName = "Cerberus",
    Title = "Guardian of the Underworld",
    MaxHealth = 50000,
    Damage = 50,
    XPReward = 500,
    IgnoresPacifist = true,
    Phases = new[] {
        new BossPhase { HealthThreshold = 1f, AttackPatterns = new[] { "triple_bite", "fire_breath" } },
        new BossPhase { HealthThreshold = 0.66f, AttackPatterns = new[] { "triple_bite", "fire_breath", "ground_slam" }, DamageMultiplier = 1.25f },
        new BossPhase { HealthThreshold = 0.33f, AttackPatterns = new[] { "berserk_combo" }, DamageMultiplier = 1.5f, SpeedMultiplier = 1.5f }
    },
    DropTable = new[] {
        new Drop { Resource = ResourceType.Leather, MinAmount = 50, MaxAmount = 100, Chance = 1f },
        new Drop { Resource = ResourceType.Coins, MinAmount = 1000, MaxAmount = 2000, Chance = 1f }
    },
    MythicalDrops = new[] { "cerberus_collar" }
};
```

## Implementation Checklist

- [ ] Create Boss base class extending Enemy
- [ ] Implement BossData resource
- [ ] Create BossAI with phase transitions
- [ ] Implement attack pattern system
- [ ] Create boss health bar UI
- [ ] Add attack telegraph/warning indicators
- [ ] Implement mythical item drops
- [ ] Create first boss (Spider Queen)
- [ ] Add boss music system
- [ ] Implement arena barriers during fight
