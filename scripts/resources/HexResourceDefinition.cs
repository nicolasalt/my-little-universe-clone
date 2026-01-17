using Godot;

/// <summary>
/// Collection of resource spawn points for a single hex.
/// </summary>
[GlobalClass]
public partial class HexResourceDefinition : Resource
{
    /// <summary>
    /// All spawn points within this hex.
    /// </summary>
    [Export] public ResourceSpawnPoint[] SpawnPoints = System.Array.Empty<ResourceSpawnPoint>();

    public HexResourceDefinition() { }

    public HexResourceDefinition(ResourceSpawnPoint[] spawnPoints)
    {
        SpawnPoints = spawnPoints;
    }
}
