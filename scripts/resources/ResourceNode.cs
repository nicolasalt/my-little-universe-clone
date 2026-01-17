using Godot;

/// <summary>
/// A harvestable resource in the world (tree, rock, ore vein, etc).
/// Auto-gathered when player is in range with appropriate tool.
/// </summary>
public partial class ResourceNode : StaticBody3D
{
    [Export] public ResourceType Type = ResourceType.Wood;
    [Export] public int TotalHits = 3;
    [Export] public int YieldPerHit = 1;
    [Export] public float RespawnTime = 30f;

    private int _remainingHits;
    private bool _isDepleted;
    private Node3D _visual;
    private Vector3 _originalScale;

    public bool IsDepleted => _isDepleted;
    public int RemainingHits => _remainingHits;

    public override void _Ready()
    {
        _remainingHits = TotalHits;
        _visual = GetNodeOrNull<Node3D>("Visual");
        if (_visual != null)
        {
            _originalScale = _visual.Scale;
        }
    }

    /// <summary>
    /// Called when the player gathers from this node.
    /// Returns the amount of resources yielded.
    /// </summary>
    public int OnGathered(float yieldMultiplier = 1f)
    {
        if (_isDepleted) return 0;

        _remainingHits--;

        int yield = Mathf.CeilToInt(YieldPerHit * yieldMultiplier);

        // Visual feedback - shrink based on remaining hits
        if (_visual != null)
        {
            float scale = (float)_remainingHits / TotalHits;
            scale = Mathf.Max(scale, 0.1f); // Minimum visible scale
            _visual.Scale = _originalScale * scale;
        }

        // Spawn hit particles
        SpawnHitParticles();

        // Shake effect on hit
        PlayHitShake();

        // Check if depleted
        if (_remainingHits <= 0)
        {
            Deplete();
        }

        return yield;
    }

    /// <summary>
    /// Spawn particle effects when hit.
    /// </summary>
    private void SpawnHitParticles()
    {
        // Get spawn position (center of visual or node position)
        Vector3 spawnPos = _visual != null ? _visual.GlobalPosition : GlobalPosition;
        spawnPos.Y += 0.5f; // Offset upward slightly

        GatherParticles.SpawnAt(GetTree().Root, spawnPos, Type);
    }

    /// <summary>
    /// Quick shake effect when hit.
    /// </summary>
    private void PlayHitShake()
    {
        if (_visual == null) return;

        var originalPos = _visual.Position;
        var tween = CreateTween();
        tween.TweenProperty(_visual, "position", originalPos + new Vector3(0.05f, 0, 0), 0.03f);
        tween.TweenProperty(_visual, "position", originalPos + new Vector3(-0.05f, 0, 0), 0.03f);
        tween.TweenProperty(_visual, "position", originalPos, 0.03f);
    }

    /// <summary>
    /// Mark the resource as depleted and start respawn timer.
    /// </summary>
    public void Deplete()
    {
        _isDepleted = true;

        // Hide visual
        if (_visual != null)
        {
            _visual.Visible = false;
        }

        // Start respawn timer
        var timer = GetTree().CreateTimer(RespawnTime);
        timer.Timeout += Respawn;
    }

    /// <summary>
    /// Restore the resource node to full.
    /// </summary>
    public void Respawn()
    {
        _isDepleted = false;
        _remainingHits = TotalHits;

        // Restore visual
        if (_visual != null)
        {
            _visual.Visible = true;
            _visual.Scale = _originalScale;
        }
    }

    /// <summary>
    /// Get the tool required to harvest this resource.
    /// </summary>
    public ToolType GetRequiredTool()
    {
        return ResourceInfo.GetRequiredTool(Type);
    }
}
