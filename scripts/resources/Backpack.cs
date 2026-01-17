using Godot;
using System.Collections.Generic;

/// <summary>
/// Player inventory for storing gathered resources.
/// Unlimited capacity - no weight or slot limits.
/// </summary>
public partial class Backpack : Node
{
    private Dictionary<ResourceType, int> _resources = new();

    public override void _Ready()
    {
        // Initialize all resource types to 0
        foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
        {
            _resources[type] = 0;
        }
    }

    /// <summary>
    /// Add resources to the backpack.
    /// </summary>
    public void Add(ResourceType type, int amount)
    {
        if (amount <= 0) return;

        _resources[type] += amount;

        SignalBus.Instance?.EmitSignal(
            SignalBus.SignalName.ResourceChanged,
            (int)type,
            _resources[type]
        );
    }

    /// <summary>
    /// Get the count of a specific resource.
    /// </summary>
    public int GetCount(ResourceType type)
    {
        return _resources.GetValueOrDefault(type, 0);
    }

    /// <summary>
    /// Check if the backpack has at least the specified amount.
    /// </summary>
    public bool Has(ResourceType type, int amount)
    {
        return GetCount(type) >= amount;
    }

    /// <summary>
    /// Check if the backpack has all required resources.
    /// </summary>
    public bool HasResources(Dictionary<ResourceType, int> required)
    {
        foreach (var (type, amount) in required)
        {
            if (!Has(type, amount)) return false;
        }
        return true;
    }

    /// <summary>
    /// Try to spend resources. Returns false if insufficient.
    /// </summary>
    public bool TrySpend(Dictionary<ResourceType, int> cost)
    {
        if (!HasResources(cost)) return false;

        foreach (var (type, amount) in cost)
        {
            _resources[type] -= amount;

            SignalBus.Instance?.EmitSignal(
                SignalBus.SignalName.ResourceChanged,
                (int)type,
                _resources[type]
            );
        }

        return true;
    }

    /// <summary>
    /// Spend a single resource type.
    /// </summary>
    public bool TrySpend(ResourceType type, int amount)
    {
        return TrySpend(new Dictionary<ResourceType, int> { { type, amount } });
    }
}
