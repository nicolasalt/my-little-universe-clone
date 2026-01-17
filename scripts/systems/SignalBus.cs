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

    public override void _Ready()
    {
        Instance = this;
    }
}
