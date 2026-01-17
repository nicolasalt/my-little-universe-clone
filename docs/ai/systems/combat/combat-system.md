# Combat System

Simple melee combat with enemies and bosses.

## Basic Attack

- Tap/Click to attack
- Auto-targets nearest enemy in range
- Attack speed modified by tool level and boosters
- Damage = BaseDamage * ToolMod * BoosterMod

## Charged Ability

- Hold attack button to charge
- Visual indicator shows charge progress
- Release to trigger AoE ability
- Each tool has unique ability effect
- Cast speed modified by Sleight of Hand boosters

### Tool Abilities

| Tool | Ability Effect |
|------|----------------|
| Sword | Wide slash, high damage to all enemies in arc |
| Pickaxe | Ground pound, damages enemies and gathers resources |
| Axe | Spinning attack, 360-degree damage |

## Enemy Combat

### Enemy Behavior
- Patrol or idle until player enters aggro range
- Chase player and attack in melee range
- Return to spawn if player escapes

### Pacifist Booster
- Enemies won't initiate attacks
- Player can farm peacefully
- Does NOT affect bosses

## Damage Calculation

```csharp
int damage = (int)(BaseDamage
    * ToolLevelMod
    * MightyBoosterMod
    * SuperiorityMod);

// Mighty stacks additively: 3x Mighty = 1 + 0.15 + 0.15 + 0.15 = 1.45
// Superiority stacks: General + Tool-specific can both apply
```

## Taking Damage

- Health reduced by enemy attack damage
- Armor reduces incoming damage
- Health regenerates slowly out of combat
- At 0 health: instant respawn, lose 1 booster card

## Hazards

| Hazard | Effect | Counter |
|--------|--------|---------|
| Fire/Lava | Burning damage over time | Fireproof booster |
| Poison | Poison damage over time | Antidote booster |
| Drowning | Damage when out of breath | Surface periodically |

## Changelog
- 26-01-17: Initial design for My Little Universe clone
