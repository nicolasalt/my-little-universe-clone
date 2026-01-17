# Booster Card System

Temporary buffs earned by leveling up.

## Core Mechanics

- Maximum 10 active booster card slots
- Earned by choosing from 3 random options on level up
- Death removes 1 random card
- Cards can be manually discarded
- Some cards stack, others don't

## Card Selection

On level up:
1. 3 random cards offered
2. Player picks one
3. Card immediately activates
4. Lower levels (0-6) grant cards faster than higher levels (7-9)

## Complete Card List

### Movement Cards

| Card | Effect | Stacks |
|------|--------|--------|
| Sprinter | +5% movement speed | Yes |
| Swimmer | +10% swimming speed | Yes |
| Agile Caster | Faster movement during ability cast | No |
| No Breaks | Faster movement after ability cast | No |

### Combat Cards

| Card | Effect | Stacks |
|------|--------|--------|
| Mighty | +15% damage (additive) | Yes |
| Berserker | +15% sword attack speed | Yes |
| Thorns | Reflect 50% of received damage | Yes |
| Fire Starter | Ability ignites enemies (friendly fire!) | No |
| Venomous | Ability inflicts poison (friendly fire!) | No |

### Ability Cards

| Card | Effect | Stacks |
|------|--------|--------|
| Sleight of Hand | Faster ability casting (all tools) | Yes* |
| Sleight of Hand - Axe | Faster axe ability casting | Yes |
| Sleight of Hand - Pickaxe | Faster pickaxe ability casting | Yes |
| Sleight of Hand - Sword | Faster sword ability casting | Yes |
| Area of Effect | Increases ability range | Yes* |
| Area of Effect - Axe | Increases axe ability range | Yes |
| Area of Effect - Pickaxe | Increases pickaxe ability range | Yes |
| Area of Effect - Sword | Increases sword ability range | Yes |
| Superiority | Boosts ability damage | Yes* |
| Superiority - Axe | Boosts axe ability damage | Yes |
| Superiority - Pickaxe | Boosts pickaxe ability damage | Yes |
| Superiority - Sword | Boosts sword ability damage | Yes |

*General + tool-specific versions stack together

### Gathering Cards

| Card | Effect | Stacks |
|------|--------|--------|
| Gatherer | +10% gather speed | Yes |
| Gem Collector | Increased crystal gathering | Yes |
| Bounty Hunter | +30% coins from monsters | Yes |

### Defensive Cards

| Card | Effect | Stacks |
|------|--------|--------|
| Metabolism | +20% max HP | Yes |
| Fireproof | Immunity to fire damage | No |
| Antidote | Immunity to poison damage | No |
| Pacifist | Monsters won't initiate attacks (not bosses) | No |

## Recommended Builds

### Farming Build
1. Pacifist
2. Gatherer (stack)
3. Gem Collector
4. Sleight of Hand (General + Sword)

### Boss Fighting Build
1. Sleight of Hand (General + Sword)
2. Superiority (General + Sword)
3. Metabolism (stack)
4. Fireproof / Antidote

## Data Structure

```csharp
class BoosterCard : Resource
{
    string Id;
    string DisplayName;
    Texture2D Icon;
    string Description;
    bool CanStack;
    BoosterCategory Category;
    Dictionary<string, float> StatModifiers;
}

class BoosterCardManager
{
    List<BoosterCard> ActiveCards;  // max 10
    void AddCard(BoosterCard card);
    void RemoveRandomCard();
    void DiscardCard(int index);
    float GetStatModifier(string statName);
}
```

## Changelog
- 26-01-17: Initial design for My Little Universe clone
