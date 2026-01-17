---
title: Upgrade Station
status: review
modifies: systems/facilities/upgrade-station.md, systems/tools/tool-system.md, systems/tools/armor.md
priority: P1
author: AI
created: 26-01-17
---

# Upgrade Station

Facility for upgrading tools and armor.

## Summary

Implement the upgrade station where players spend resources to improve their equipment through 8 levels across 4 color tiers.

## Design

### Upgrade Station

```csharp
public partial class UpgradeStation : StaticBody3D
{
    [Signal] public delegate void UpgradeCompletedEventHandler(EquipmentType type, int newLevel);

    public void OpenUpgradeUI();
    public bool CanUpgrade(EquipmentType type);
    public Dictionary<ResourceType, int> GetUpgradeCost(EquipmentType type);
    public void PerformUpgrade(EquipmentType type);
}
```

### Equipment Type

```csharp
public enum EquipmentType
{
    Pickaxe,
    Axe,
    Sword,
    Helmet,
    Armor
}
```

### Upgrade UI

```csharp
public partial class UpgradeUI : Control
{
    [Export] public EquipmentSlot[] Slots;

    private UpgradeStation _station;

    public void Populate();
    public void OnSlotSelected(EquipmentType type);
    public void OnUpgradeConfirmed();
}

public partial class EquipmentSlot : Control
{
    public EquipmentType Type;
    public int CurrentLevel;
    public TextureRect Icon;
    public Label LevelLabel;
    public Control CostContainer;
    public Button UpgradeButton;

    public void UpdateDisplay();
    public void ShowCost(Dictionary<ResourceType, int> cost, Backpack backpack);
}
```

### Cost Calculation

```csharp
public static class UpgradeCosts
{
    public static Dictionary<ResourceType, int> Calculate(
        EquipmentType type,
        int currentLevel,
        int worldId)
    {
        int nextLevel = currentLevel + 1;
        var cost = new Dictionary<ResourceType, int>();

        // Base wood/metal varies by world
        ResourceType wood = GetWoodForWorld(worldId);
        ResourceType metal = GetMetalForWorld(worldId);

        cost[wood] = 10 * nextLevel;
        cost[metal] = 5 * nextLevel;

        // Gems for higher tiers
        if (nextLevel >= 3)
            cost[ResourceType.Ruby] = 2 * (nextLevel - 2);
        if (nextLevel >= 5)
            cost[ResourceType.Amethyst] = 2 * (nextLevel - 4);
        if (nextLevel >= 7)
            cost[ResourceType.Diamond] = nextLevel - 6;

        return cost;
    }

    private static ResourceType GetWoodForWorld(int worldId) =>
        worldId >= 7 ? ResourceType.Hardwood : ResourceType.Planks;

    private static ResourceType GetMetalForWorld(int worldId) => worldId switch
    {
        >= 9 => ResourceType.CopperIngots,
        >= 8 => ResourceType.ProcessedSandstone,
        >= 6 => ResourceType.Marble,
        _ => ResourceType.Steel
    };
}
```

### Upgrade Priority Guidance

Display recommended upgrade order in UI:
1. Sword (combat efficiency)
2. Armor (survivability)
3. Pickaxe (resource gathering)
4. Axe (wood gathering)

## Visual Design

- Station: Anvil/forge aesthetic
- Equipment displays on pedestals
- Tier colors: Red (1-2), Green (3-4), Purple (5-6), Black (7-8)
- Upgrade animation with sparks and glow
- Level indicators clearly visible

## Implementation Checklist

- [ ] Create UpgradeStation scene
- [ ] Implement UpgradeUI with equipment slots
- [ ] Create cost calculation system
- [ ] Add world-specific material requirements
- [ ] Implement tier color system
- [ ] Add upgrade animation/VFX
- [ ] Connect to player equipment
- [ ] Add sound effects
- [ ] Show affordable/unaffordable state
- [ ] Integrate with GameManager state (Upgrading)
