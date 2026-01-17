using Godot;

/// <summary>
/// Types of resources that can be gathered in the world.
/// </summary>
public enum ResourceType
{
    // Basic gatherable resources
    Wood,
    Stone,
    IronOre,

    // Currency
    Coins,
    Gems,

    // Processed materials (for future)
    Planks,
    Steel
}

/// <summary>
/// Defines which tool is required to gather a resource type.
/// </summary>
public enum ToolType
{
    None,
    Axe,
    Pickaxe,
    Sword
}

/// <summary>
/// Static helper for resource type information.
/// </summary>
public static class ResourceInfo
{
    public static ToolType GetRequiredTool(ResourceType type)
    {
        return type switch
        {
            ResourceType.Wood => ToolType.Axe,
            ResourceType.Stone => ToolType.Pickaxe,
            ResourceType.IronOre => ToolType.Pickaxe,
            ResourceType.Coins => ToolType.None,
            ResourceType.Gems => ToolType.Pickaxe,
            ResourceType.Planks => ToolType.None,
            ResourceType.Steel => ToolType.None,
            _ => ToolType.None
        };
    }

    public static string GetDisplayName(ResourceType type)
    {
        return type switch
        {
            ResourceType.Wood => "Wood",
            ResourceType.Stone => "Stone",
            ResourceType.IronOre => "Iron Ore",
            ResourceType.Coins => "Coins",
            ResourceType.Gems => "Gems",
            ResourceType.Planks => "Planks",
            ResourceType.Steel => "Steel",
            _ => type.ToString()
        };
    }
}
