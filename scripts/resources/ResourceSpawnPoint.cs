using Godot;

/// <summary>
/// Defines a single resource spawn point within a hex.
/// </summary>
[GlobalClass]
public partial class ResourceSpawnPoint : Resource
{
    /// <summary>
    /// The scene to instantiate (Tree.tscn, Rock.tscn, etc).
    /// </summary>
    [Export] public PackedScene ResourceScene;

    /// <summary>
    /// Offset from hex center (X = world X, Y = world Z).
    /// </summary>
    [Export] public Vector2 LocalOffset;

    /// <summary>
    /// Y rotation in degrees.
    /// </summary>
    [Export] public float RotationY;

    public ResourceSpawnPoint() { }

    public ResourceSpawnPoint(PackedScene scene, Vector2 offset, float rotation = 0f)
    {
        ResourceScene = scene;
        LocalOffset = offset;
        RotationY = rotation;
    }
}
