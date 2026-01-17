using Godot;

/// <summary>
/// Handles automatic hex unlocking when player is near an unlockable hex.
/// Drains resources from the player's backpack to pay for unlocks.
/// </summary>
public partial class HexPaymentController : Node
{
    [Export] public float PaymentRate = 5f; // Resources per second
    [Export] public float DetectionRadius = 6f; // How close to trigger payment (slightly larger than hex size)

    private Node3D _player;
    private Backpack _backpack;
    private Vector2I? _currentTarget;
    private float _paymentTimer;
    private HexVisual _highlightedVisual;

    public Vector2I? CurrentTarget => _currentTarget;
    public bool IsPaying => _currentTarget.HasValue;

    public override void _Ready()
    {
        _player = GetParent<Node3D>();
        _backpack = _player.GetNodeOrNull<Backpack>("Backpack");

        if (_backpack == null)
        {
            GD.PrintErr("HexPaymentController: No Backpack found on player");
        }
    }

    public override void _Process(double delta)
    {
        if (_backpack == null || HexGridManager.Instance == null) return;

        ProcessPayment(delta);
    }

    private void ProcessPayment(double delta)
    {
        var grid = HexGridManager.Instance;
        var playerHex = grid.WorldToHex(_player.GlobalPosition);

        // Find nearest payable hex within range (unlockable or already unlocking)
        Vector2I? nearestPayable = FindNearestPayable(playerHex);

        // Update target if changed
        if (nearestPayable != _currentTarget)
        {
            SetTarget(nearestPayable);
        }

        // Process payment to current target
        if (_currentTarget.HasValue)
        {
            _paymentTimer += (float)delta;

            float paymentInterval = 1f / PaymentRate;
            while (_paymentTimer >= paymentInterval)
            {
                _paymentTimer -= paymentInterval;
                TryPayResource(_currentTarget.Value);
            }
        }
    }

    private Vector2I? FindNearestPayable(Vector2I playerHex)
    {
        var grid = HexGridManager.Instance;
        Vector2I? nearest = null;
        float nearestDist = float.MaxValue;

        // Check both unlockable tiles AND tiles already being unlocked
        foreach (var coords in grid.GetPayableTiles())
        {
            var worldPos = grid.HexToWorld(coords);
            float dist = _player.GlobalPosition.DistanceTo(worldPos);

            if (dist < DetectionRadius && dist < nearestDist)
            {
                nearest = coords;
                nearestDist = dist;
            }
        }

        return nearest;
    }

    private void SetTarget(Vector2I? newTarget)
    {
        var grid = HexGridManager.Instance;

        // Clear highlight on old target and refresh its visual
        if (_highlightedVisual != null && _currentTarget.HasValue)
        {
            _highlightedVisual.SetHighlight(false);
            var oldTile = grid.GetTile(_currentTarget.Value);
            if (oldTile != null)
            {
                _highlightedVisual.UpdateState(oldTile);
            }
            _highlightedVisual = null;
        }

        _currentTarget = newTarget;
        _paymentTimer = 0f;

        // Start or continue unlocking if new target
        if (_currentTarget.HasValue)
        {
            var tile = grid.GetTile(_currentTarget.Value);

            if (tile != null && tile.State == HexState.Locked)
            {
                grid.StartUnlocking(_currentTarget.Value);
            }

            // Highlight new target
            _highlightedVisual = grid.GetVisual(_currentTarget.Value);
            if (_highlightedVisual != null)
            {
                _highlightedVisual.SetHighlight(true);
                _highlightedVisual.UpdateState(tile);
            }

            // Update UI
            HexUnlockUI.Instance?.SetTarget(_currentTarget);
        }
        else
        {
            // Clear UI when no target
            HexUnlockUI.Instance?.SetTarget(null);
        }
    }

    private void TryPayResource(Vector2I coords)
    {
        var grid = HexGridManager.Instance;
        var tile = grid.GetTile(coords);

        if (tile == null || tile.State != HexState.Unlocking) return;

        // Try to pay each resource type that still needs payment
        foreach (var (type, cost) in tile.UnlockCost)
        {
            int remaining = tile.GetRemaining(type);
            if (remaining > 0 && _backpack.Has(type, 1))
            {
                _backpack.TrySpend(type, 1);
                grid.AddPayment(coords, type, 1);
                return; // Only pay one resource per tick
            }
        }
    }
}
