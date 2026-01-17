using Godot;

/// <summary>
/// Floating text popup that shows resource gain (e.g., "+2 Wood").
/// Floats upward and fades out over time.
/// </summary>
public partial class ResourcePopup : Label3D
{
    [Export] public float RiseSpeed = 0.8f;
    [Export] public float Duration = 1.5f;
    [Export] public float FadeDelay = 1.0f;

    private float _elapsed = 0f;
    private Vector3 _startPos;
    private bool _initialized = false;

    public override void _Ready()
    {
        // Configure Label3D appearance
        Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;
        NoDepthTest = true;
        FixedSize = false;
        PixelSize = 0.01f;
        FontSize = 32;
        OutlineSize = 4;

        // Self-destruct after duration
        var timer = GetTree().CreateTimer(Duration);
        timer.Timeout += QueueFree;
    }

    public override void _Process(double delta)
    {
        // Capture start position on first frame (after GlobalPosition is set)
        if (!_initialized)
        {
            _startPos = GlobalPosition;
            _initialized = true;
        }

        _elapsed += (float)delta;

        // Float upward from start position
        GlobalPosition = _startPos + new Vector3(0, _elapsed * RiseSpeed, 0);

        // Fade out after delay
        if (_elapsed > FadeDelay)
        {
            float fadeProgress = (_elapsed - FadeDelay) / (Duration - FadeDelay);
            Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, 1f - fadeProgress);
        }
    }

    /// <summary>
    /// Configure the popup text and color based on resource type.
    /// </summary>
    public void Setup(ResourceType type, int amount)
    {
        string resourceName = ResourceInfo.GetDisplayName(type);
        Text = $"+{amount} {resourceName}";

        // Set color based on resource type
        Color textColor = type switch
        {
            ResourceType.Wood => new Color(0.65f, 0.45f, 0.25f),
            ResourceType.Stone => new Color(0.6f, 0.6f, 0.6f),
            ResourceType.IronOre => new Color(0.7f, 0.5f, 0.35f),
            ResourceType.Coins => new Color(1f, 0.85f, 0.2f),
            ResourceType.Gems => new Color(0.8f, 0.3f, 0.9f),
            _ => new Color(1f, 1f, 1f)
        };

        Modulate = textColor;
        OutlineModulate = new Color(0, 0, 0, 0.8f);
    }

    /// <summary>
    /// Factory method to spawn a resource popup at a position.
    /// </summary>
    public static ResourcePopup SpawnAt(Node parent, Vector3 position, ResourceType type, int amount)
    {
        var popup = new ResourcePopup();
        popup.Setup(type, amount);
        parent.AddChild(popup);
        popup.GlobalPosition = position;
        return popup;
    }
}
