# Player Stats

Resource-based stat system for health, stamina, mana.

## Resource
`PlayerStats.cs` (extends Resource)

## Properties

```csharp
float MaxHealth = 100
float CurrentHealth = 100
float MaxStamina = 100
float CurrentStamina = 100
float MaxMana = 50
float CurrentMana = 50
float StaminaRegenRate = 10  // per second
float ManaRegenRate = 5      // per second
```

## Signals

- HealthChanged(float current, float max)
- StaminaChanged(float current, float max)
- ManaChanged(float current, float max)
- PlayerDied()

## Changelog
- 26-01-17: Initial design
