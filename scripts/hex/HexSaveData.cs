using Godot;

/// <summary>
/// Initial state for a hex when loading from map data.
/// </summary>
public enum HexInitialState
{
    Locked,
    Unlocked
}

/// <summary>
/// Serialization-friendly hex configuration for saving/loading maps.
/// Separate from runtime HexTile because:
/// - Uses explicit cost fields instead of Dictionary (Godot serializes these properly)
/// - Stores initial state, not runtime state (no Unlocking state)
/// - References spawn data directly
/// </summary>
[GlobalClass]
public partial class HexSaveData : Resource
{
    /// <summary>
    /// Hex coordinates in offset (odd-q) format.
    /// </summary>
    [Export] public Vector2I Coordinates { get; set; }

    /// <summary>
    /// Initial state when the game starts.
    /// </summary>
    [Export] public HexInitialState InitialState { get; set; } = HexInitialState.Locked;

    /// <summary>
    /// Wood cost to unlock this hex.
    /// </summary>
    [Export] public int UnlockCostWood { get; set; }

    /// <summary>
    /// Stone cost to unlock this hex.
    /// </summary>
    [Export] public int UnlockCostStone { get; set; }

    /// <summary>
    /// Override normal visibility rules - hex stays hidden even when adjacent to unlocked.
    /// Used for secrets or late-game areas.
    /// </summary>
    [Export] public bool StartHidden { get; set; }

    /// <summary>
    /// Resource spawn points within this hex.
    /// </summary>
    [Export] public Godot.Collections.Array<ResourceSpawnPoint> Spawns { get; set; } = new();

    public HexSaveData() { }

    public HexSaveData(Vector2I coordinates)
    {
        Coordinates = coordinates;
    }

    /// <summary>
    /// Convert to runtime HexTile with populated UnlockCost dictionary.
    /// </summary>
    public HexTile ToRuntimeTile()
    {
        var tile = new HexTile(Coordinates);
        tile.State = InitialState == HexInitialState.Unlocked ? HexState.Unlocked : HexState.Locked;

        if (UnlockCostWood > 0)
            tile.UnlockCost[ResourceType.Wood] = UnlockCostWood;
        if (UnlockCostStone > 0)
            tile.UnlockCost[ResourceType.Stone] = UnlockCostStone;

        return tile;
    }
}
