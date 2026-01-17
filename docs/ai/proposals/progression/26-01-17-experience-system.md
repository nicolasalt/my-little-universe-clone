---
title: Experience and Leveling
status: review
modifies: systems/progression/experience.md, systems/progression/booster-cards.md
priority: P1
author: AI
created: 26-01-17
---

# Experience and Leveling

XP from actions leads to booster card choices.

## Summary

Implement the experience system where all player actions grant XP. Leveling up offers a choice of 3 random booster cards.

## Design

### Experience Manager

```csharp
public partial class ExperienceManager : Node
{
    [Export] public int MaxLevel = 9;
    [Export] public float BaseXPToLevel = 100f;
    [Export] public float LevelScaling = 1.5f;

    public int CurrentLevel { get; private set; }
    public float CurrentXP { get; private set; }
    public float XPToNextLevel => GetXPForLevel(CurrentLevel + 1);

    [Signal] public delegate void XPGainedEventHandler(float amount, float total, float required);
    [Signal] public delegate void LeveledUpEventHandler(int newLevel);

    public void AddXP(float amount);
    private void CheckLevelUp();
    private float GetXPForLevel(int level);
}
```

### XP Sources

```csharp
public static class XPValues
{
    public const float GatherResource = 1f;
    public const float DefeatEnemy = 5f;
    public const float DefeatBoss = 50f;
    public const float UnlockHex = 10f;
    public const float CompleteDungeon = 25f;

    public static float GetWorldMultiplier(int worldId) => 1 + (worldId * 0.2f);
}
```

### Level Up Flow

```csharp
private void OnLevelUp()
{
    CurrentLevel = (CurrentLevel + 1) % (MaxLevel + 1);  // Cycles 0-9
    CurrentXP = 0;

    // Offer card selection
    var cardChoices = boosterCardManager.GetRandomCards(3);
    EmitSignal(SignalName.LeveledUp, CurrentLevel);
    ShowCardSelection(cardChoices);
}
```

### Card Selection UI

```csharp
public partial class CardSelectionUI : Control
{
    [Export] public CardDisplay[] CardSlots;  // 3 slots

    private BoosterCard[] _choices;

    public void ShowSelection(BoosterCard[] cards);
    public void OnCardSelected(int index);
    public void OnCardHovered(int index);
}

public partial class CardDisplay : Control
{
    public Texture2D Icon;
    public string Name;
    public string Description;
    public bool CanStack;
    public int CurrentStacks;

    public void Populate(BoosterCard card);
    public void ShowTooltip();
}
```

### Booster Card Manager

```csharp
public partial class BoosterCardManager : Node
{
    [Export] public int MaxCards = 10;

    private List<BoosterCard> _activeCards = new();
    private BoosterCard[] _allCards;  // All possible cards

    public List<BoosterCard> ActiveCards => _activeCards;

    public BoosterCard[] GetRandomCards(int count);
    public void AddCard(BoosterCard card);
    public void RemoveRandomCard();
    public void DiscardCard(int index);
    public float GetStatModifier(StatType stat);
    public bool HasCard(string cardId);
    public int GetStackCount(string cardId);
}
```

### Stat Calculation with Cards

```csharp
public float GetStatModifier(StatType stat)
{
    float modifier = 0f;

    foreach (var card in _activeCards)
    {
        if (card.StatModifiers.TryGetValue(stat, out float mod))
        {
            if (card.StacksAdditively)
                modifier += mod;  // Mighty: +0.15 per stack
            else
                modifier = Mathf.Max(modifier, mod);  // Take highest
        }
    }

    return modifier;
}
```

## XP Bar Visual

- Circular or bar indicator
- Fills as XP gained
- Flash/pulse on level up
- Level number displayed

## Implementation Checklist

- [ ] Create ExperienceManager
- [ ] Define XP values for actions
- [ ] Implement level cycling (0-9)
- [ ] Create card selection UI
- [ ] Implement BoosterCardManager
- [ ] Define all booster cards
- [ ] Add card tooltips
- [ ] Implement stat modifier calculations
- [ ] Connect XP gain to all actions
- [ ] Add level-up VFX
- [ ] Create XP bar HUD element
