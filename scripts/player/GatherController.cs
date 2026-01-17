using Godot;

/// <summary>
/// Handles automatic resource gathering when player is near resource nodes.
/// </summary>
public partial class GatherController : Node
{
    [Export] public float BaseGatherRate = 1.0f; // Gathers per second
    [Export] public float YieldMultiplier = 1.0f;
    [Export] public bool AutoGather = true;

    private GatherArea _gatherArea;
    private Backpack _backpack;
    private ResourceNode _currentTarget;
    private float _gatherTimer;
    private Node3D _player;

    public ResourceNode CurrentTarget => _currentTarget;
    public bool IsGathering => _currentTarget != null;

    public override void _Ready()
    {
        _player = GetParent<Node3D>();
        _gatherArea = _player.GetNodeOrNull<GatherArea>("GatherArea");
        _backpack = _player.GetNodeOrNull<Backpack>("Backpack");

        if (_gatherArea == null)
        {
            GD.PrintErr("GatherController: No GatherArea found on player");
        }
        if (_backpack == null)
        {
            GD.PrintErr("GatherController: No Backpack found on player");
        }
    }

    public override void _Process(double delta)
    {
        if (!AutoGather || _gatherArea == null || _backpack == null) return;

        ProcessGathering(delta);
    }

    private void ProcessGathering(double delta)
    {
        // Find a target if we don't have one
        if (_currentTarget == null || _currentTarget.IsDepleted)
        {
            var newTarget = _gatherArea.GetNearestResource();

            if (newTarget != _currentTarget)
            {
                SetTarget(newTarget);
            }
        }

        // Gather from current target
        if (_currentTarget != null && !_currentTarget.IsDepleted)
        {
            _gatherTimer += (float)delta;

            float gatherInterval = 1f / GetEffectiveGatherRate();

            while (_gatherTimer >= gatherInterval)
            {
                _gatherTimer -= gatherInterval;
                PerformGather();
            }
        }
    }

    private void SetTarget(ResourceNode target)
    {
        if (_currentTarget != null)
        {
            SignalBus.Instance?.EmitSignal(SignalBus.SignalName.GatheringStopped);
        }

        _currentTarget = target;
        _gatherTimer = 0f;

        if (_currentTarget != null)
        {
            SignalBus.Instance?.EmitSignal(
                SignalBus.SignalName.GatheringStarted,
                _currentTarget
            );
        }
    }

    private void PerformGather()
    {
        if (_currentTarget == null || _currentTarget.IsDepleted) return;

        int yield = _currentTarget.OnGathered(YieldMultiplier);

        if (yield > 0)
        {
            _backpack.Add(_currentTarget.Type, yield);

            SignalBus.Instance?.EmitSignal(
                SignalBus.SignalName.ResourceGathered,
                (int)_currentTarget.Type,
                yield
            );

            // TODO: Trigger gather animation and particles
        }

        // Check if depleted
        if (_currentTarget.IsDepleted)
        {
            SetTarget(_gatherArea.GetNearestResource());
        }
    }

    /// <summary>
    /// Calculate the effective gather rate with all modifiers.
    /// </summary>
    public float GetEffectiveGatherRate()
    {
        // Base formula: BaseGatherRate * (1 + toolLevel * 0.1) * (1 + gathererStacks * 0.1)
        // For now, just return base rate. Tool and card bonuses will be added later.
        return BaseGatherRate;
    }

    /// <summary>
    /// Manually start gathering from a specific node.
    /// </summary>
    public void StartGathering(ResourceNode node)
    {
        SetTarget(node);
    }

    /// <summary>
    /// Stop gathering the current target.
    /// </summary>
    public void StopGathering()
    {
        SetTarget(null);
    }
}
