---
title: Booster Card Implementation
status: review
modifies: systems/progression/booster-cards.md
priority: P1
author: AI
created: 26-01-17
---

# Booster Card Implementation

Complete booster card data and effects.

## Summary

Define all 25+ booster cards with their effects, stacking rules, and integration with game systems.

## Design

### Booster Card Resource

```csharp
[GlobalClass]
public partial class BoosterCard : Resource
{
    [Export] public string Id;
    [Export] public string DisplayName;
    [Export] public string Description;
    [Export] public Texture2D Icon;
    [Export] public BoosterCategory Category;
    [Export] public bool CanStack;
    [Export] public bool StacksAdditively;  // vs takes highest
    [Export] public ToolType? ToolSpecific;  // null = all tools
    [Export] public Dictionary<StatType, float> StatModifiers;
    [Export] public string[] SpecialEffects;  // For unique effects
}
```

### Stat Types

```csharp
public enum StatType
{
    // Movement
    MoveSpeed,
    SwimSpeed,
    CastMoveSpeed,
    PostCastMoveSpeed,

    // Combat
    Damage,
    AttackSpeed,
    ThornsReflect,

    // Ability
    AbilityCastSpeed,
    AbilityRange,
    AbilityDamage,

    // Gathering
    GatherSpeed,
    GemGatherBonus,
    CoinDropBonus,

    // Defense
    MaxHealth,
    FireImmunity,
    PoisonImmunity,
    PassiveAggro
}
```

### Card Definitions

```csharp
public static class BoosterCardDefinitions
{
    public static BoosterCard[] All = new[]
    {
        // Movement
        new BoosterCard {
            Id = "sprinter",
            DisplayName = "Sprinter",
            Description = "+5% movement speed",
            CanStack = true,
            StacksAdditively = true,
            StatModifiers = { { StatType.MoveSpeed, 0.05f } }
        },
        new BoosterCard {
            Id = "swimmer",
            DisplayName = "Swimmer",
            Description = "+10% swimming speed",
            CanStack = true,
            StacksAdditively = true,
            StatModifiers = { { StatType.SwimSpeed, 0.10f } }
        },

        // Combat
        new BoosterCard {
            Id = "mighty",
            DisplayName = "Mighty",
            Description = "+15% damage (stacks additively)",
            CanStack = true,
            StacksAdditively = true,
            StatModifiers = { { StatType.Damage, 0.15f } }
        },
        new BoosterCard {
            Id = "berserker",
            DisplayName = "Berserker",
            Description = "+15% sword attack speed",
            CanStack = true,
            ToolSpecific = ToolType.Sword,
            StatModifiers = { { StatType.AttackSpeed, 0.15f } }
        },
        new BoosterCard {
            Id = "thorns",
            DisplayName = "Thorns",
            Description = "Reflect 50% of damage received",
            CanStack = true,
            StacksAdditively = true,
            StatModifiers = { { StatType.ThornsReflect, 0.50f } }
        },

        // Ability - General
        new BoosterCard {
            Id = "sleight_of_hand",
            DisplayName = "Sleight of Hand",
            Description = "Faster ability casting",
            CanStack = true,
            StatModifiers = { { StatType.AbilityCastSpeed, -0.25f } }
        },
        // Ability - Tool Specific (Sword)
        new BoosterCard {
            Id = "sleight_of_hand_sword",
            DisplayName = "Sleight of Hand - Sword",
            Description = "Faster sword ability casting",
            CanStack = true,
            ToolSpecific = ToolType.Sword,
            StatModifiers = { { StatType.AbilityCastSpeed, -0.25f } }
        },

        // Similar for Pickaxe, Axe...
        // Area of Effect variants...
        // Superiority variants...

        // Gathering
        new BoosterCard {
            Id = "gatherer",
            DisplayName = "Gatherer",
            Description = "+10% gather speed",
            CanStack = true,
            StacksAdditively = true,
            StatModifiers = { { StatType.GatherSpeed, 0.10f } }
        },
        new BoosterCard {
            Id = "gem_collector",
            DisplayName = "Gem Collector",
            Description = "Increased crystal gathering",
            CanStack = true,
            StatModifiers = { { StatType.GemGatherBonus, 0.25f } }
        },
        new BoosterCard {
            Id = "bounty_hunter",
            DisplayName = "Bounty Hunter",
            Description = "+30% coins from monsters",
            CanStack = true,
            StacksAdditively = true,
            StatModifiers = { { StatType.CoinDropBonus, 0.30f } }
        },

        // Defense
        new BoosterCard {
            Id = "metabolism",
            DisplayName = "Metabolism",
            Description = "+20% max HP",
            CanStack = true,
            StacksAdditively = true,
            StatModifiers = { { StatType.MaxHealth, 0.20f } }
        },
        new BoosterCard {
            Id = "fireproof",
            DisplayName = "Fireproof",
            Description = "Immunity to fire damage",
            CanStack = false,
            StatModifiers = { { StatType.FireImmunity, 1f } }
        },
        new BoosterCard {
            Id = "antidote",
            DisplayName = "Antidote",
            Description = "Immunity to poison damage",
            CanStack = false,
            StatModifiers = { { StatType.PoisonImmunity, 1f } }
        },
        new BoosterCard {
            Id = "pacifist",
            DisplayName = "Pacifist",
            Description = "Monsters won't attack first (except bosses)",
            CanStack = false,
            StatModifiers = { { StatType.PassiveAggro, 1f } }
        },

        // Special
        new BoosterCard {
            Id = "fire_starter",
            DisplayName = "Fire Starter",
            Description = "Ability ignites enemies (friendly fire!)",
            CanStack = false,
            SpecialEffects = new[] { "ignite_on_ability" }
        },
        new BoosterCard {
            Id = "venomous",
            DisplayName = "Venomous",
            Description = "Ability inflicts poison (friendly fire!)",
            CanStack = false,
            SpecialEffects = new[] { "poison_on_ability" }
        }
    };
}
```

### Special Effect Handler

```csharp
public partial class SpecialEffectHandler : Node
{
    public void ProcessAbilityHit(Node target, List<string> activeEffects)
    {
        if (activeEffects.Contains("ignite_on_ability"))
            ApplyBurn(target);
        if (activeEffects.Contains("poison_on_ability"))
            ApplyPoison(target);
    }

    private void ApplyBurn(Node target)
    {
        if (target is IFlammable flammable)
            flammable.Ignite(duration: 3f, damagePerSecond: 5f);
    }

    private void ApplyPoison(Node target)
    {
        if (target is IPoisonable poisonable)
            poisonable.Poison(duration: 5f, damagePerSecond: 3f);
    }
}
```

## Implementation Checklist

- [ ] Create BoosterCard resource class
- [ ] Define all 25+ cards
- [ ] Implement StatType enum
- [ ] Create card icon assets
- [ ] Implement stacking logic
- [ ] Add tool-specific card filtering
- [ ] Implement special effects system
- [ ] Handle Fire Starter/Venomous friendly fire
- [ ] Create card tooltip descriptions
- [ ] Test all stat modifiers
