using Godot;
using System.Collections.Generic;

/// <summary>
/// Manages spawning resources when hexes are unlocked.
/// Singleton that listens to HexUnlocked signal.
/// </summary>
public partial class ResourceSpawnManager : Node
{
    public static ResourceSpawnManager Instance { get; private set; }

    [Export] public PackedScene TreeScene;
    [Export] public PackedScene RockScene;

    /// <summary>
    /// Maps hex coordinates to their resource definitions.
    /// </summary>
    private Dictionary<Vector2I, HexResourceDefinition> _spawnDefinitions = new();

    /// <summary>
    /// Tracks spawned resource nodes per hex for save/load.
    /// </summary>
    private Dictionary<Vector2I, List<Node3D>> _spawnedResources = new();

    public override void _Ready()
    {
        Instance = this;

        // Initialize spawn definitions
        InitializeSpawnDefinitions();

        // Connect to HexUnlocked signal
        if (SignalBus.Instance != null)
        {
            SignalBus.Instance.HexUnlocked += OnHexUnlocked;
        }
        else
        {
            // SignalBus might not be ready yet, defer connection
            CallDeferred(nameof(ConnectSignals));
        }

        // Spawn resources for origin hex (already unlocked at start)
        CallDeferred(nameof(SpawnOriginResources));
    }

    /// <summary>
    /// Configure spawn definitions for each hex.
    /// </summary>
    private void InitializeSpawnDefinitions()
    {
        // Hex (0,0) - Origin: 2 trees + 1 rock
        _spawnDefinitions[new Vector2I(0, 0)] = new HexResourceDefinition(new[]
        {
            new ResourceSpawnPoint(TreeScene, new Vector2(3, -3)),
            new ResourceSpawnPoint(TreeScene, new Vector2(-3, -3)),
            new ResourceSpawnPoint(RockScene, new Vector2(-3, 2))
        });

        // Hex (1,0) - East: 1 tree + 1 rock
        _spawnDefinitions[new Vector2I(1, 0)] = new HexResourceDefinition(new[]
        {
            new ResourceSpawnPoint(TreeScene, new Vector2(1, 2)),
            new ResourceSpawnPoint(RockScene, new Vector2(-1, -1))
        });

        // Hex (0,1) - North: rock cluster
        _spawnDefinitions[new Vector2I(0, 1)] = new HexResourceDefinition(new[]
        {
            new ResourceSpawnPoint(RockScene, new Vector2(2, 0)),
            new ResourceSpawnPoint(RockScene, new Vector2(2.5f, 1))
        });
    }

    private void ConnectSignals()
    {
        if (SignalBus.Instance != null)
        {
            SignalBus.Instance.HexUnlocked += OnHexUnlocked;
        }
    }

    /// <summary>
    /// Spawn resources for the origin hex which is unlocked at game start.
    /// </summary>
    private void SpawnOriginResources()
    {
        SpawnResourcesForHex(Vector2I.Zero);
    }

    /// <summary>
    /// Called when a hex is unlocked via signal.
    /// </summary>
    private void OnHexUnlocked(Vector2I coords)
    {
        SpawnResourcesForHex(coords);
    }

    /// <summary>
    /// Spawn all resources defined for the given hex.
    /// </summary>
    public void SpawnResourcesForHex(Vector2I coords)
    {
        // Check if we have a definition for this hex
        if (!_spawnDefinitions.TryGetValue(coords, out var definition))
        {
            return;
        }

        // Check if already spawned
        if (_spawnedResources.ContainsKey(coords))
        {
            return;
        }

        // Get hex center in world space
        Vector3 hexCenter = HexCoordinates.HexToWorld(coords);

        // Initialize tracking list
        _spawnedResources[coords] = new List<Node3D>();

        // Spawn each resource
        foreach (var spawnPoint in definition.SpawnPoints)
        {
            if (spawnPoint?.ResourceScene == null) continue;

            var resource = spawnPoint.ResourceScene.Instantiate<Node3D>();
            if (resource == null) continue;

            // Position: hex center + local offset (LocalOffset.X = world X, LocalOffset.Y = world Z)
            resource.Position = hexCenter + new Vector3(
                spawnPoint.LocalOffset.X,
                0f,
                spawnPoint.LocalOffset.Y
            );

            // Rotation
            resource.RotationDegrees = new Vector3(0f, spawnPoint.RotationY, 0f);

            // Add to scene tree (as sibling of this manager)
            GetParent().AddChild(resource);

            // Track for save/load
            _spawnedResources[coords].Add(resource);
        }
    }

    /// <summary>
    /// Get all spawned resources for a hex. Used for save/load.
    /// </summary>
    public List<Node3D> GetResourcesForHex(Vector2I coords)
    {
        return _spawnedResources.GetValueOrDefault(coords);
    }

    /// <summary>
    /// Check if resources have been spawned for a hex.
    /// </summary>
    public bool HasSpawnedResources(Vector2I coords)
    {
        return _spawnedResources.ContainsKey(coords);
    }

    public override void _ExitTree()
    {
        if (SignalBus.Instance != null)
        {
            SignalBus.Instance.HexUnlocked -= OnHexUnlocked;
        }
    }
}
