using Godot;

public partial class SignalBus : Node
{
    public static SignalBus Instance { get; private set; }

    [Signal]
    public delegate void PlayerSpawnedEventHandler(Node3D player);

    [Signal]
    public delegate void PlayerDiedEventHandler();

    [Signal]
    public delegate void HealthChangedEventHandler(float current, float max);

    [Signal]
    public delegate void StaminaChangedEventHandler(float current, float max);

    [Signal]
    public delegate void ManaChangedEventHandler(float current, float max);

    [Signal]
    public delegate void ItemPickedUpEventHandler(string itemName);

    [Signal]
    public delegate void InteractionPromptEventHandler(string promptText, bool show);

    [Signal]
    public delegate void ResourceGatheredEventHandler(int resourceType, int amount);

    [Signal]
    public delegate void ResourceChangedEventHandler(int resourceType, int newTotal);

    [Signal]
    public delegate void GatheringStartedEventHandler(Node3D target);

    [Signal]
    public delegate void GatheringStoppedEventHandler();

    // Hex territory signals
    [Signal]
    public delegate void HexUnlockStartedEventHandler(Vector2I coords);

    [Signal]
    public delegate void HexUnlockProgressEventHandler(Vector2I coords, float progress);

    [Signal]
    public delegate void HexUnlockedEventHandler(Vector2I coords);

    public override void _Ready()
    {
        Instance = this;
    }
}
