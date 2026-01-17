using Godot;
using System.Collections.Generic;

/// <summary>
/// Area3D attached to player that detects nearby harvestable resources.
/// </summary>
public partial class GatherArea : Area3D
{
    [Export] public float GatherRadius = 2.0f;

    private List<ResourceNode> _nodesInRange = new();

    public override void _Ready()
    {
        // Configure collision shape
        var shape = new SphereShape3D();
        shape.Radius = GatherRadius;

        var collision = new CollisionShape3D();
        collision.Shape = shape;
        AddChild(collision);

        // Connect signals
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;

        // Set collision mask to detect resource nodes (layer 2)
        CollisionMask = 2;
        CollisionLayer = 0; // Don't collide with anything
    }

    private void OnBodyEntered(Node3D body)
    {
        if (body is ResourceNode node && !node.IsDepleted)
        {
            _nodesInRange.Add(node);
        }
    }

    private void OnBodyExited(Node3D body)
    {
        if (body is ResourceNode node)
        {
            _nodesInRange.Remove(node);
        }
    }

    /// <summary>
    /// Get the nearest harvestable resource node.
    /// </summary>
    public ResourceNode GetNearestResource()
    {
        ResourceNode nearest = null;
        float nearestDist = float.MaxValue;

        // Clean up depleted nodes and find nearest
        for (int i = _nodesInRange.Count - 1; i >= 0; i--)
        {
            var node = _nodesInRange[i];

            if (!IsInstanceValid(node) || node.IsDepleted)
            {
                _nodesInRange.RemoveAt(i);
                continue;
            }

            float dist = GlobalPosition.DistanceTo(node.GlobalPosition);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = node;
            }
        }

        return nearest;
    }

    /// <summary>
    /// Get all harvestable resources currently in range.
    /// </summary>
    public List<ResourceNode> GetResourcesInRange()
    {
        // Clean up invalid/depleted nodes
        _nodesInRange.RemoveAll(node =>
            !IsInstanceValid(node) || node.IsDepleted);

        return new List<ResourceNode>(_nodesInRange);
    }

    /// <summary>
    /// Check if there are any harvestable resources in range.
    /// </summary>
    public bool HasResourcesInRange()
    {
        return GetNearestResource() != null;
    }
}
