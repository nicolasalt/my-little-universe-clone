---
title: Basic Combat System
status: review
modifies: systems/combat/combat-system.md, systems/player/controller.md
priority: P0
author: AI
created: 26-01-17
---

# Basic Combat System

Simple melee combat with enemies.

## Summary

Implement basic combat mechanics: attacking enemies with sword, taking damage, and charged ability attacks.

## Design

### Combat Controller

```csharp
public partial class CombatController : Node
{
    [Export] public float BaseAttackRate = 1.0f;  // attacks per second
    [Export] public float BaseDamage = 10f;
    [Export] public float BaseAbilityDamage = 25f;
    [Export] public float AbilityChargeTime = 1.0f;
    [Export] public float AbilityRange = 3f;

    private float _attackCooldown;
    private float _chargeProgress;
    private bool _isCharging;

    public void Attack();
    public void StartCharge();
    public void ReleaseCharge();
    public void CancelCharge();
}
```

### Attack Hitbox

```csharp
public partial class AttackArea : Area3D
{
    [Export] public float BaseRange = 2f;

    public List<Enemy> GetEnemiesInRange();
    public Enemy GetNearestEnemy();
}
```

### Damage Calculation

```csharp
public int CalculateDamage(bool isAbility)
{
    float baseDmg = isAbility ? BaseAbilityDamage : BaseDamage;

    // Tool modifier (+15% per level for sword)
    float toolMod = 1 + (toolLevel * 0.15f);

    // Mighty stacks additively
    float mightyMod = 1 + (mightyStacks * 0.15f);

    // Superiority (general + sword specific)
    float superiorityMod = 1 + generalSuperiority + swordSuperiority;

    return (int)(baseDmg * toolMod * mightyMod * superiorityMod);
}
```

### Health System

```csharp
public partial class HealthComponent : Node
{
    [Export] public float MaxHealth = 100f;
    [Export] public float RegenRate = 2f;  // per second, out of combat
    [Export] public float RegenDelay = 5f; // seconds after taking damage

    public float CurrentHealth { get; private set; }

    [Signal] public delegate void DamagedEventHandler(float amount);
    [Signal] public delegate void HealedEventHandler(float amount);
    [Signal] public delegate void DiedEventHandler();

    public void TakeDamage(float amount);
    public void Heal(float amount);
}
```

### Death Handling

```csharp
private void OnPlayerDied()
{
    // Instant respawn
    player.GlobalPosition = lastSafePosition;
    player.Health.Heal(player.Health.MaxHealth);

    // Lose random booster card
    boosterCardManager.RemoveRandomCard();

    // Signal for UI feedback
    SignalBus.EmitSignal(SignalBus.SignalName.PlayerDied);
}
```

### Charged Ability

```csharp
public void ProcessCharge(double delta)
{
    if (!_isCharging) return;

    _chargeProgress += (float)delta / AbilityChargeTime;
    _chargeProgress = Mathf.Min(_chargeProgress, 1f);

    // Visual feedback
    chargeIndicator.Value = _chargeProgress;
}

public void ReleaseCharge()
{
    if (_chargeProgress >= 1f)
    {
        // Full charge - execute ability
        var enemies = attackArea.GetEnemiesInRange(AbilityRange);
        int damage = CalculateDamage(isAbility: true);
        foreach (var enemy in enemies)
            enemy.TakeDamage(damage);

        PlayAbilityEffect();
    }

    _isCharging = false;
    _chargeProgress = 0f;
}
```

## Visual Feedback

- Sword swing animation
- Hit flash on enemies
- Damage numbers
- Charge glow effect
- Ability slash VFX
- Screen shake on big hits

## Implementation Checklist

- [ ] Create CombatController
- [ ] Add AttackArea to player
- [ ] Implement basic attack with cooldown
- [ ] Add HealthComponent
- [ ] Implement damage calculation with modifiers
- [ ] Add charge ability system
- [ ] Create ability VFX for sword
- [ ] Implement death and respawn
- [ ] Add damage number popups
- [ ] Connect to booster card system for modifiers
- [ ] Add attack animations
