# Tool System

Three primary tools for gathering and combat.

## Tools

### Pickaxe
- Primary: Mining rocks, ores, gems
- Charged ability: Area mining blast
- Priority upgrade: 3rd (after sword, armor)

### Axe
- Primary: Chopping trees, wood resources
- Charged ability: Spinning AoE attack
- Priority upgrade: 4th (lowest)

### Sword
- Primary: Combat with enemies and bosses
- Charged ability: Powerful AoE slash
- Priority upgrade: 1st (most important)

## Upgrade Tiers

Each tool has 8 upgrade levels across 4 color tiers:

| Tier | Color | Levels |
|------|-------|--------|
| 1 | Red | 1-2 |
| 2 | Green | 3-4 |
| 3 | Purple | 5-6 |
| 4 | Black | 7-8 |

## Upgrade Effects

Each level provides:
- +10% gather/attack speed
- +15% damage (sword)
- +10% yield (pickaxe/axe)
- Improved charged ability

## Upgrade Materials by World

| World | Wood Material | Metal Material |
|-------|--------------|----------------|
| Gaia, Trollheim, Dimidium | Wood/Planks | Iron Ingots |
| Factorium, Wadirum | Wood/Planks | Iron Ingots |
| Dragonora | Hardwood | Iron Ingots |
| Odysseum | Hardwood | Marble |
| Egyptium | Hardwood | Processed Sandstone |
| Asium | Hardwood | Copper Ingots |

## Data Structure

```csharp
enum ToolType { Pickaxe, Axe, Sword }

class Tool : Resource
{
    ToolType Type;
    int Level;              // 1-8
    float GatherSpeedMod;   // calculated from level
    float DamageMod;        // calculated from level
    float YieldMod;         // calculated from level
    float AbilityRangeMod;  // calculated from level
}
```

## Tool Switching

- Auto mode: Switches based on nearest interactable
- Manual mode: Press V to cycle tools
- Setting saved in options

## Changelog
- 26-01-17: Initial design for My Little Universe clone
