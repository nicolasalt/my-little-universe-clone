using Godot;

/// <summary>
/// Root resource containing the entire hex map configuration.
/// Used for saving/loading hex maps from the editor.
/// </summary>
[GlobalClass]
public partial class HexMapData : Resource
{
    /// <summary>
    /// All hexes in the map.
    /// </summary>
    [Export] public Godot.Collections.Array<HexSaveData> Hexes { get; set; } = new();
}
