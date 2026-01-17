using Godot;
using System.Collections.Generic;

/// <summary>
/// State of a hex tile.
/// </summary>
public enum HexState
{
    Locked,
    Unlocking,
    Unlocked
}

/// <summary>
/// Data for a single hex tile in the grid.
/// </summary>
public partial class HexTile : Resource
{
    [Export] public Vector2I Coordinates { get; set; }
    [Export] public HexState State { get; set; } = HexState.Locked;

    // Unlock cost (resource type -> amount required)
    public Dictionary<ResourceType, int> UnlockCost { get; set; } = new();

    // Amount paid so far (resource type -> amount paid)
    public Dictionary<ResourceType, int> PaidAmount { get; set; } = new();

    public HexTile() { }

    public HexTile(Vector2I coordinates)
    {
        Coordinates = coordinates;
    }

    /// <summary>
    /// Get total cost for a specific resource.
    /// </summary>
    public int GetCost(ResourceType type)
    {
        return UnlockCost.GetValueOrDefault(type, 0);
    }

    /// <summary>
    /// Get amount paid for a specific resource.
    /// </summary>
    public int GetPaid(ResourceType type)
    {
        return PaidAmount.GetValueOrDefault(type, 0);
    }

    /// <summary>
    /// Get remaining cost for a specific resource.
    /// </summary>
    public int GetRemaining(ResourceType type)
    {
        return Mathf.Max(0, GetCost(type) - GetPaid(type));
    }

    /// <summary>
    /// Check if unlock cost is fully paid.
    /// </summary>
    public bool IsFullyPaid()
    {
        foreach (var (type, cost) in UnlockCost)
        {
            if (GetPaid(type) < cost) return false;
        }
        return true;
    }

    /// <summary>
    /// Add payment toward unlock cost.
    /// </summary>
    public void AddPayment(ResourceType type, int amount)
    {
        if (!PaidAmount.ContainsKey(type))
        {
            PaidAmount[type] = 0;
        }
        PaidAmount[type] += amount;
    }

    /// <summary>
    /// Get unlock progress as percentage (0-1).
    /// </summary>
    public float GetProgress()
    {
        int totalCost = 0;
        int totalPaid = 0;

        foreach (var (type, cost) in UnlockCost)
        {
            totalCost += cost;
            totalPaid += Mathf.Min(GetPaid(type), cost);
        }

        if (totalCost == 0) return 1f;
        return (float)totalPaid / totalCost;
    }

    /// <summary>
    /// Calculate unlock cost based on distance from origin.
    /// </summary>
    public static Dictionary<ResourceType, int> CalculateCost(int distance)
    {
        var cost = new Dictionary<ResourceType, int>();

        switch (distance)
        {
            case 0:
                // Origin is free
                break;
            case 1:
                cost[ResourceType.Wood] = 10;
                break;
            case 2:
                cost[ResourceType.Wood] = 15;
                cost[ResourceType.Stone] = 5;
                break;
            case 3:
                cost[ResourceType.Wood] = 20;
                cost[ResourceType.Stone] = 10;
                break;
            default:
                cost[ResourceType.Wood] = 25 + (distance - 4) * 5;
                cost[ResourceType.Stone] = 15 + (distance - 4) * 5;
                break;
        }

        return cost;
    }
}
