# Player Stats

Resource-based stat system for player health and combat.

## Resource
`PlayerStats.cs` (extends Resource)

## Properties

```csharp
// Health (base, modified by Metabolism booster and armor)
float MaxHealth = 100;
float CurrentHealth = 100;

// Combat stats (modified by tool level and boosters)
float BaseDamage = 10;
float AttackSpeed = 1.0f;        // attacks per second
float AbilityDamage = 25;
float AbilityCastSpeed = 1.0f;   // seconds to charge
float AbilityRange = 3.0f;       // meters

// Gathering stats (modified by tool level and boosters)
float GatherSpeed = 1.0f;        // gathers per second
float GatherYield = 1.0f;        // multiplier

// Movement stats (modified by boosters)
float MoveSpeed = 5.0f;
float SwimSpeed = 3.0f;

// Experience
int CurrentLevel = 0;
float CurrentXP = 0;
float XPToNextLevel = 100;
```

## Signals

- `HealthChanged(float current, float max)`
- `PlayerDied()`
- `XPGained(float amount)`
- `LeveledUp(int newLevel)`
- `StatModified(string statName, float newValue)`

## Death Behavior
- Instant respawn at last safe location
- Keep all resources and upgrades
- Lose one random Booster Card

## Changelog
- 26-01-17: Updated for My Little Universe design - removed stamina/mana, added combat/gather stats
- 26-01-17: Initial design
