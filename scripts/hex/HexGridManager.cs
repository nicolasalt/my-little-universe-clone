using Godot;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Main controller for the hex grid. Manages tile creation, state, and unlocking.
/// </summary>
public partial class HexGridManager : Node3D
{
    private const string MapFilePath = "res://data/maps/hex_map.tres";

    [Export] public int GridRadius = 3; // Creates a 7x7 grid (radius 3 from center)
    [Export] public PackedScene HexVisualScene;

    private Dictionary<Vector2I, HexTile> _tiles = new();
    private Dictionary<Vector2I, HexVisual> _visuals = new();

    /// <summary>
    /// Spawn data loaded from map file, keyed by hex coordinates.
    /// Populated by LoadFromData() for ResourceSpawnManager to consume.
    /// </summary>
    private Dictionary<Vector2I, Godot.Collections.Array<ResourceSpawnPoint>> _spawnData = new();

    public static HexGridManager Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;

        // Check for map file first, fall back to procedural generation
        if (ResourceLoader.Exists(MapFilePath))
        {
            var mapData = GD.Load<HexMapData>(MapFilePath);
            if (mapData != null)
            {
                LoadFromData(mapData);
                return;
            }
        }

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
                bool isOrigin = coords == Vector2I.Zero;
                CreateTile(coords, startVisible: isOrigin);
            }
        }

        // Unlock the origin tile and reveal adjacent hexes
        var originTile = GetTile(Vector2I.Zero);
        if (originTile != null)
        {
            originTile.State = HexState.Unlocked;
            UpdateVisual(Vector2I.Zero);
            RevealAdjacentHexes(Vector2I.Zero);
        }
    }

    /// <summary>
    /// Reveal hexes adjacent to a newly unlocked hex with staggered animation.
    /// </summary>
    private void RevealAdjacentHexes(Vector2I unlockedCoords)
    {
        float delay = 0f;
        const float staggerDelay = 0.05f;

        foreach (var neighbor in HexCoordinates.GetNeighbors(unlockedCoords))
        {
            var visual = GetVisual(neighbor);
            var tile = GetTile(neighbor);

            if (visual != null && tile != null && tile.State == HexState.Locked)
            {
                if (!visual.IsCurrentlyVisible)
                {
                    visual.SetUnlockable(true);
                    visual.UpdateState(tile);
                    visual.AnimateAppear(delay);
                    delay += staggerDelay;
                }
            }
        }
    }

    private void CreateTile(Vector2I coords, bool startVisible = false)
    {
        // Create tile data
        var tile = new HexTile(coords);
        int distance = HexCoordinates.HexDistance(coords, Vector2I.Zero);
        tile.UnlockCost = HexTile.CalculateCost(distance);
        _tiles[coords] = tile;

        // Create visual
        var visual = new HexVisual();
        visual.Initialize(coords, tile, startVisible);
        visual.Position = HexCoordinates.HexToWorld(coords);
        AddChild(visual);
        _visuals[coords] = visual;
    }

    /// <summary>
    /// Load hex grid from map data file.
    /// </summary>
    public void LoadFromData(HexMapData data)
    {
        ClearGrid();

        // Track which hexes start unlocked so we can reveal their neighbors
        var unlockedHexes = new List<Vector2I>();

        foreach (var hexData in data.Hexes)
        {
            var tile = hexData.ToRuntimeTile();
            bool startVisible = tile.State == HexState.Unlocked && !hexData.StartHidden;

            CreateTileFromData(hexData.Coordinates, tile, hexData.StartHidden, startVisible);

            // Store spawn data for ResourceSpawnManager
            if (hexData.Spawns != null && hexData.Spawns.Count > 0)
            {
                _spawnData[hexData.Coordinates] = hexData.Spawns;
            }

            if (tile.State == HexState.Unlocked)
            {
                unlockedHexes.Add(hexData.Coordinates);
            }
        }

        // Reveal hexes adjacent to unlocked hexes
        foreach (var coords in unlockedHexes)
        {
            RevealAdjacentHexes(coords);
        }
    }

    /// <summary>
    /// Create a tile from loaded save data.
    /// </summary>
    private void CreateTileFromData(Vector2I coords, HexTile tile, bool startHidden, bool startVisible)
    {
        _tiles[coords] = tile;

        // Create visual
        var visual = new HexVisual();
        visual.Initialize(coords, tile, startVisible, startHidden);
        visual.Position = HexCoordinates.HexToWorld(coords);
        AddChild(visual);
        _visuals[coords] = visual;
    }

    /// <summary>
    /// Add a single hex tile at runtime. Used by the editor.
    /// </summary>
    public void AddTile(Vector2I coords, HexTile tile, bool startVisible = true)
    {
        if (_tiles.ContainsKey(coords)) return;
        CreateTileFromData(coords, tile, false, startVisible);
    }

    /// <summary>
    /// Remove a single hex tile at runtime. Used by the editor.
    /// </summary>
    public void RemoveTile(Vector2I coords)
    {
        if (_visuals.TryGetValue(coords, out var visual))
        {
            visual.QueueFree();
            _visuals.Remove(coords);
        }
        _tiles.Remove(coords);
        _spawnData.Remove(coords);
    }

    /// <summary>
    /// Clear all tiles and visuals from the grid.
    /// </summary>
    public void ClearGrid()
    {
        foreach (var visual in _visuals.Values)
        {
            visual.QueueFree();
        }
        _tiles.Clear();
        _visuals.Clear();
        _spawnData.Clear();
    }

    /// <summary>
    /// Get spawn data for a hex (for ResourceSpawnManager).
    /// Returns null if no spawn data exists for the hex.
    /// </summary>
    public Godot.Collections.Array<ResourceSpawnPoint> GetSpawnData(Vector2I coords)
    {
        return _spawnData.GetValueOrDefault(coords);
    }

    /// <summary>
    /// Check if spawn data was loaded from a map file.
    /// </summary>
    public bool HasLoadedSpawnData => _spawnData.Count > 0;

    /// <summary>
    /// Get all hex coordinates that have spawn data defined.
    /// </summary>
    public IEnumerable<Vector2I> GetHexesWithSpawnData()
    {
        return _spawnData.Keys;
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
    /// Get all tile coordinates and their tiles.
    /// </summary>
    public IEnumerable<(Vector2I coords, HexTile tile, HexVisual visual)> GetAllHexes()
    {
        foreach (var (coords, tile) in _tiles)
        {
            _visuals.TryGetValue(coords, out var visual);
            yield return (coords, tile, visual);
        }
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

        // Play unlock animation
        var visual = GetVisual(coords);
        visual?.AnimateUnlock();

        // Reveal adjacent locked hexes
        RevealAdjacentHexes(coords);

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
