using Godot;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Main controller for the hex grid. Manages tile creation, state, and unlocking.
/// </summary>
public partial class HexGridManager : Node3D
{
    [Export] public int GridRadius = 3; // Creates a 7x7 grid (radius 3 from center)
    [Export] public PackedScene HexVisualScene;

    private Dictionary<Vector2I, HexTile> _tiles = new();
    private Dictionary<Vector2I, HexVisual> _visuals = new();

    public static HexGridManager Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;
        GenerateGrid();
    }

    private void GenerateGrid()
    {
        // Generate hexes in a rectangular area centered on origin
        for (int q = -GridRadius; q <= GridRadius; q++)
        {
            for (int r = -GridRadius; r <= GridRadius; r++)
            {
                var coords = new Vector2I(q, r);
                CreateTile(coords);
            }
        }

        // Unlock the origin tile
        var originTile = GetTile(Vector2I.Zero);
        if (originTile != null)
        {
            originTile.State = HexState.Unlocked;
            UpdateVisual(Vector2I.Zero);
            UpdateUnlockableVisuals();
        }
    }

    /// <summary>
    /// Update visual state of all tiles that can be unlocked.
    /// </summary>
    private void UpdateUnlockableVisuals()
    {
        foreach (var (coords, visual) in _visuals)
        {
            bool canUnlock = CanUnlock(coords);
            visual.SetUnlockable(canUnlock);
            var tile = GetTile(coords);
            if (tile != null)
            {
                visual.UpdateState(tile);
            }
        }
    }

    private void CreateTile(Vector2I coords)
    {
        // Create tile data
        var tile = new HexTile(coords);
        int distance = HexCoordinates.HexDistance(coords, Vector2I.Zero);
        tile.UnlockCost = HexTile.CalculateCost(distance);
        _tiles[coords] = tile;

        // Create visual
        var visual = new HexVisual();
        visual.Initialize(coords, tile);
        visual.Position = HexCoordinates.HexToWorld(coords);
        AddChild(visual);
        _visuals[coords] = visual;
    }

    /// <summary>
    /// Get tile at coordinates.
    /// </summary>
    public HexTile GetTile(Vector2I coords)
    {
        return _tiles.GetValueOrDefault(coords);
    }

    /// <summary>
    /// Get visual at coordinates.
    /// </summary>
    public HexVisual GetVisual(Vector2I coords)
    {
        return _visuals.GetValueOrDefault(coords);
    }

    /// <summary>
    /// Check if a tile can be unlocked (is locked and adjacent to an unlocked tile).
    /// </summary>
    public bool CanUnlock(Vector2I coords)
    {
        var tile = GetTile(coords);
        if (tile == null || tile.State != HexState.Locked) return false;

        // Check if any neighbor is unlocked
        foreach (var neighbor in HexCoordinates.GetNeighbors(coords))
        {
            var neighborTile = GetTile(neighbor);
            if (neighborTile != null && neighborTile.State == HexState.Unlocked)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Start unlocking a tile.
    /// </summary>
    public void StartUnlocking(Vector2I coords)
    {
        var tile = GetTile(coords);
        if (tile == null || !CanUnlock(coords)) return;

        tile.State = HexState.Unlocking;
        UpdateVisual(coords);

        SignalBus.Instance?.EmitSignal(SignalBus.SignalName.HexUnlockStarted, coords);
    }

    /// <summary>
    /// Add payment to a tile being unlocked.
    /// </summary>
    public void AddPayment(Vector2I coords, ResourceType type, int amount)
    {
        var tile = GetTile(coords);
        if (tile == null || tile.State != HexState.Unlocking) return;

        tile.AddPayment(type, amount);
        UpdateVisual(coords);

        SignalBus.Instance?.EmitSignal(
            SignalBus.SignalName.HexUnlockProgress,
            coords,
            tile.GetProgress()
        );

        // Check if fully paid
        if (tile.IsFullyPaid())
        {
            CompleteUnlock(coords);
        }
    }

    /// <summary>
    /// Complete the unlock of a tile.
    /// </summary>
    private void CompleteUnlock(Vector2I coords)
    {
        var tile = GetTile(coords);
        if (tile == null) return;

        tile.State = HexState.Unlocked;
        UpdateVisual(coords);
        UpdateUnlockableVisuals();

        SignalBus.Instance?.EmitSignal(SignalBus.SignalName.HexUnlocked, coords);
    }

    /// <summary>
    /// Update the visual representation of a tile.
    /// </summary>
    private void UpdateVisual(Vector2I coords)
    {
        var visual = GetVisual(coords);
        var tile = GetTile(coords);
        if (visual != null && tile != null)
        {
            visual.UpdateState(tile);
        }
    }

    /// <summary>
    /// Get all tiles that can currently be unlocked.
    /// </summary>
    public IEnumerable<Vector2I> GetUnlockableTiles()
    {
        return _tiles.Keys.Where(CanUnlock);
    }

    /// <summary>
    /// Get all tiles that can be paid for (unlockable or currently unlocking).
    /// </summary>
    public IEnumerable<Vector2I> GetPayableTiles()
    {
        foreach (var (coords, tile) in _tiles)
        {
            if (tile.State == HexState.Unlocking || CanUnlock(coords))
            {
                yield return coords;
            }
        }
    }

    /// <summary>
    /// Get the hex coordinates at a world position.
    /// </summary>
    public Vector2I WorldToHex(Vector3 worldPos)
    {
        return HexCoordinates.WorldToHex(worldPos);
    }

    /// <summary>
    /// Get the world position of hex coordinates.
    /// </summary>
    public Vector3 HexToWorld(Vector2I coords)
    {
        return HexCoordinates.HexToWorld(coords);
    }
}
