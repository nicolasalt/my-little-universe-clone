using Godot;

/// <summary>
/// Visual representation of a single hex tile.
/// Generates procedural hex mesh and updates color based on state.
/// </summary>
public partial class HexVisual : Node3D
{
    private static readonly Color UnlockedColor = new(0.3f, 0.6f, 0.25f, 1f); // Bright green
    private static readonly Color LockedColor = new(0.1f, 0.1f, 0.12f, 1f); // Dark gray
    private static readonly Color UnlockableColor = new(0.2f, 0.25f, 0.4f, 1f); // Noticeable blue-ish
    private static readonly Color UnlockingColor = new(0.6f, 0.5f, 0.2f, 1f); // Yellow-orange

    private MeshInstance3D _meshInstance;
    private StaticBody3D _collisionBody;
    private StandardMaterial3D _material;
    private Vector2I _coordinates;
    private bool _isUnlockable;

    public Vector2I Coordinates => _coordinates;

    public void Initialize(Vector2I coords, HexTile tile)
    {
        _coordinates = coords;
        CreateMesh();
        CreateCollision();
        UpdateState(tile);
    }

    private void CreateMesh()
    {
        _meshInstance = new MeshInstance3D();
        AddChild(_meshInstance);

        var mesh = new ArrayMesh();
        var arrays = new Godot.Collections.Array();
        arrays.Resize((int)Mesh.ArrayType.Max);

        // Get hex vertices and indices
        var vertices = HexCoordinates.GetHexVertices(HexCoordinates.HexSize * 0.95f); // Slightly smaller for gap
        var indices = HexCoordinates.GetHexIndices();

        // Generate normals (all pointing up)
        var normals = new Vector3[vertices.Length];
        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = Vector3.Up;
        }

        arrays[(int)Mesh.ArrayType.Vertex] = vertices;
        arrays[(int)Mesh.ArrayType.Normal] = normals;
        arrays[(int)Mesh.ArrayType.Index] = indices;

        mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
        _meshInstance.Mesh = mesh;

        // Create material
        _material = new StandardMaterial3D();
        _material.AlbedoColor = LockedColor;
        _meshInstance.MaterialOverride = _material;
    }

    private void CreateCollision()
    {
        _collisionBody = new StaticBody3D();
        AddChild(_collisionBody);

        var collisionShape = new CollisionShape3D();
        var shape = new CylinderShape3D();
        shape.Radius = HexCoordinates.HexSize * 1.05f; // Slightly larger to overlap with neighbors
        shape.Height = 0.5f;
        collisionShape.Shape = shape;
        collisionShape.Position = new Vector3(0, -0.25f, 0);
        _collisionBody.AddChild(collisionShape);
    }

    public void UpdateState(HexTile tile)
    {
        if (_material == null) return;

        switch (tile.State)
        {
            case HexState.Unlocked:
                _material.AlbedoColor = UnlockedColor;
                _material.EmissionEnabled = false;
                break;
            case HexState.Unlocking:
                // Lerp between unlockable and unlocking colors based on progress
                _material.AlbedoColor = UnlockableColor.Lerp(UnlockingColor, tile.GetProgress());
                _material.Emission = new Color(0.2f, 0.2f, 0.05f);
                _material.EmissionEnabled = true;
                _material.EmissionEnergyMultiplier = 0.5f + tile.GetProgress() * 0.5f;
                break;
            case HexState.Locked:
            default:
                _material.AlbedoColor = _isUnlockable ? UnlockableColor : LockedColor;
                if (_isUnlockable)
                {
                    _material.Emission = new Color(0.1f, 0.1f, 0.2f);
                    _material.EmissionEnabled = true;
                    _material.EmissionEnergyMultiplier = 0.3f;
                }
                else
                {
                    _material.EmissionEnabled = false;
                }
                break;
        }
    }

    public void SetUnlockable(bool unlockable)
    {
        _isUnlockable = unlockable;
    }

    public void SetHighlight(bool highlighted)
    {
        if (_material == null) return;

        if (highlighted)
        {
            _material.Emission = new Color(0.4f, 0.4f, 0.15f);
            _material.EmissionEnabled = true;
            _material.EmissionEnergyMultiplier = 1.0f;
        }
        else if (!_isUnlockable)
        {
            _material.EmissionEnabled = false;
        }
    }
}
