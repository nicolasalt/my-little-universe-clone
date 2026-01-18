using Godot;

/// <summary>
/// Visual representation of a single hex tile.
/// Generates procedural hex mesh and updates color based on state.
/// Hidden until unlockable, with animations for appearing and unlocking.
/// </summary>
public partial class HexVisual : Node3D
{
    private static readonly Color UnlockedColor = new(0.3f, 0.6f, 0.25f, 1f); // Bright green
    private static readonly Color UnlockableColor = new(0.2f, 0.25f, 0.4f, 1f); // Noticeable blue-ish
    private static readonly Color UnlockingColor = new(0.6f, 0.5f, 0.2f, 1f); // Yellow-orange

    private const float AppearDuration = 0.4f;
    private const float UnlockDuration = 0.5f;

    private MeshInstance3D _meshInstance;
    private StaticBody3D _collisionBody;
    private StandardMaterial3D _material;
    private Vector2I _coordinates;
    private bool _isUnlockable;
    private bool _isVisible;
    private bool _startHidden;
    private Tween _currentTween;
    private Node3D _costDisplay;

    // Icon paths
    private const string WoodIconPath = "res://assets/icons/wood.svg";
    private const string StoneIconPath = "res://assets/icons/stone.svg";

    public Vector2I Coordinates => _coordinates;
    public bool IsCurrentlyVisible => _isVisible;

    /// <summary>
    /// Whether this hex should remain hidden even when adjacent to unlocked hexes.
    /// Used for secrets or late-game areas.
    /// </summary>
    public bool StartHidden => _startHidden;

    public void Initialize(Vector2I coords, HexTile tile, bool startVisible = false, bool startHidden = false)
    {
        _coordinates = coords;
        _startHidden = startHidden;
        CreateMesh();
        CreateCollision();

        // Start hidden unless explicitly visible (e.g., origin tile)
        // Also hide if startHidden is true (overrides visibility)
        if (startVisible && !startHidden)
        {
            _isVisible = true;
            Scale = Vector3.One;
        }
        else
        {
            _isVisible = false;
            Scale = Vector3.Zero;
        }

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
        _material.AlbedoColor = UnlockableColor;
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
                // Locked but unlockable hexes show with a subtle glow
                _material.AlbedoColor = UnlockableColor;
                _material.Emission = new Color(0.1f, 0.1f, 0.2f);
                _material.EmissionEnabled = true;
                _material.EmissionEnergyMultiplier = 0.3f;
                break;
        }
    }

    /// <summary>
    /// Animate the hex appearing (scale from 0 to 1 with bounce).
    /// Will not appear if StartHidden is true.
    /// </summary>
    public void AnimateAppear(float delay = 0f)
    {
        if (_isVisible || _startHidden) return;

        _isVisible = true;
        _currentTween?.Kill();
        _currentTween = CreateTween();

        if (delay > 0f)
        {
            _currentTween.TweenInterval(delay);
        }

        // Scale up with elastic ease for a bouncy pop-in effect
        _currentTween.TweenProperty(this, "scale", Vector3.One, AppearDuration)
            .From(Vector3.Zero)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Back);
    }

    /// <summary>
    /// Animate the hex being unlocked (pulse and color change).
    /// </summary>
    public void AnimateUnlock()
    {
        _currentTween?.Kill();
        _currentTween = CreateTween();

        // Punch scale effect (grow then shrink back)
        _currentTween.TweenProperty(this, "scale", Vector3.One * 1.15f, UnlockDuration * 0.3f)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Quad);
        _currentTween.TweenProperty(this, "scale", Vector3.One, UnlockDuration * 0.7f)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Elastic);

        // Flash emission then fade out
        if (_material != null)
        {
            _material.EmissionEnabled = true;
            _material.Emission = new Color(0.5f, 0.6f, 0.3f);
            _material.EmissionEnergyMultiplier = 2.0f;

            var colorTween = CreateTween();
            colorTween.TweenProperty(_material, "emission_energy_multiplier", 0f, UnlockDuration)
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Quad);
            colorTween.TweenCallback(Callable.From(() => _material.EmissionEnabled = false));
        }
    }

    /// <summary>
    /// Immediately show the hex without animation (for initial/loaded state).
    /// </summary>
    public void ShowImmediate()
    {
        _isVisible = true;
        Scale = Vector3.One;
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

    /// <summary>
    /// Show or hide the cost display for editor mode.
    /// </summary>
    public void ShowCostLabel(bool show)
    {
        if (_costDisplay != null)
        {
            _costDisplay.Visible = show;
        }
    }

    /// <summary>
    /// Update the cost display with current tile costs using icons.
    /// </summary>
    public void UpdateCostLabel(HexTile tile)
    {
        // Clear existing display
        if (_costDisplay != null)
        {
            _costDisplay.QueueFree();
            _costDisplay = null;
        }

        if (tile == null) return;

        _costDisplay = new Node3D();
        _costDisplay.Position = new Vector3(0, 0.5f, 0);
        AddChild(_costDisplay);

        if (tile.State == HexState.Unlocked)
        {
            // Show checkmark for unlocked
            var label = CreateLabel("✓", new Color(0.4f, 0.9f, 0.4f));
            label.Position = Vector3.Zero;
            _costDisplay.AddChild(label);
            return;
        }

        int wood = tile.GetCost(ResourceType.Wood);
        int stone = tile.GetCost(ResourceType.Stone);

        if (wood == 0 && stone == 0)
        {
            // Free hex
            var label = CreateLabel("✓", new Color(0.4f, 0.9f, 0.4f));
            label.Position = Vector3.Zero;
            _costDisplay.AddChild(label);
            return;
        }

        // Calculate layout
        float spacing = 1.2f;
        float startX = 0f;
        int itemCount = (wood > 0 ? 1 : 0) + (stone > 0 ? 1 : 0);
        if (itemCount == 2) startX = -spacing / 2f;

        float xPos = startX;

        if (wood > 0)
        {
            AddCostItem(WoodIconPath, wood, new Vector3(xPos, 0, 0));
            xPos += spacing;
        }

        if (stone > 0)
        {
            AddCostItem(StoneIconPath, stone, new Vector3(xPos, 0, 0));
        }

        _costDisplay.Visible = false; // Start hidden, ShowCostLabel will reveal
    }

    private void AddCostItem(string iconPath, int amount, Vector3 position)
    {
        // Create sprite for icon
        var sprite = new Sprite3D();
        sprite.Texture = GD.Load<Texture2D>(iconPath);
        sprite.PixelSize = 0.015f;
        sprite.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;
        sprite.NoDepthTest = true;
        sprite.Position = position + new Vector3(-0.3f, 0, 0);
        _costDisplay.AddChild(sprite);

        // Create label for amount
        var label = CreateLabel(amount.ToString(), new Color(1f, 1f, 1f));
        label.Position = position + new Vector3(0.3f, 0, 0);
        _costDisplay.AddChild(label);
    }

    private Label3D CreateLabel(string text, Color color)
    {
        var label = new Label3D();
        label.Text = text;
        label.FontSize = 32;
        label.OutlineSize = 6;
        label.Modulate = color;
        label.OutlineModulate = new Color(0f, 0f, 0f);
        label.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;
        label.NoDepthTest = true;
        label.FixedSize = false;
        label.PixelSize = 0.015f;
        label.HorizontalAlignment = HorizontalAlignment.Center;
        label.VerticalAlignment = VerticalAlignment.Center;
        return label;
    }
}
