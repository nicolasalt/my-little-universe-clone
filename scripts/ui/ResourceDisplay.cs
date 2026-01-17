using Godot;
using System.Collections.Generic;

/// <summary>
/// Resource display UI matching My Little Universe style.
/// Shows resource icons with counts in a horizontal row.
/// </summary>
public partial class ResourceDisplay : HBoxContainer
{
    [Export] public int IconSize = 40;
    [Export] public int Spacing = 10;

    private Dictionary<ResourceType, ResourceCounter> _counters = new();

    // Icon paths
    private static readonly Dictionary<ResourceType, string> IconPaths = new()
    {
        { ResourceType.Wood, "res://assets/icons/wood.svg" },
        { ResourceType.Stone, "res://assets/icons/stone.svg" },
        { ResourceType.IronOre, "res://assets/icons/iron_ore.svg" },
        { ResourceType.Coins, "res://assets/icons/coins.svg" },
        { ResourceType.Gems, "res://assets/icons/gems.svg" },
        { ResourceType.Planks, "res://assets/icons/planks.svg" },
        { ResourceType.Steel, "res://assets/icons/steel.svg" },
    };

    // Fallback colors if icon not found
    private static readonly Dictionary<ResourceType, Color> ResourceColors = new()
    {
        { ResourceType.Wood, new Color(0.6f, 0.4f, 0.2f) },
        { ResourceType.Stone, new Color(0.5f, 0.5f, 0.55f) },
        { ResourceType.IronOre, new Color(0.7f, 0.5f, 0.4f) },
        { ResourceType.Coins, new Color(1.0f, 0.85f, 0.2f) },
        { ResourceType.Gems, new Color(0.8f, 0.2f, 0.8f) },
        { ResourceType.Planks, new Color(0.8f, 0.6f, 0.3f) },
        { ResourceType.Steel, new Color(0.7f, 0.75f, 0.8f) },
    };

    public override void _Ready()
    {
        // Configure container
        AddThemeConstantOverride("separation", Spacing);

        // Create counters for main resources
        var resourcesToShow = new[] {
            ResourceType.Wood,
            ResourceType.Stone,
            ResourceType.IronOre,
            ResourceType.Coins
        };

        foreach (var type in resourcesToShow)
        {
            CreateCounter(type);
        }

        // Connect to signal
        if (SignalBus.Instance != null)
        {
            SignalBus.Instance.ResourceChanged += OnResourceChanged;
        }
    }

    public override void _ExitTree()
    {
        if (SignalBus.Instance != null)
        {
            SignalBus.Instance.ResourceChanged -= OnResourceChanged;
        }
    }

    private void CreateCounter(ResourceType type)
    {
        var counter = new ResourceCounter();
        var icon = LoadIcon(type);
        var fallbackColor = ResourceColors.GetValueOrDefault(type, new Color(0.5f, 0.5f, 0.5f));
        counter.Setup(type, icon, fallbackColor, IconSize);
        AddChild(counter);
        _counters[type] = counter;
    }

    private Texture2D LoadIcon(ResourceType type)
    {
        if (IconPaths.TryGetValue(type, out var path))
        {
            if (ResourceLoader.Exists(path))
            {
                return GD.Load<Texture2D>(path);
            }
        }
        return null;
    }

    private void OnResourceChanged(int resourceType, int newTotal)
    {
        var type = (ResourceType)resourceType;
        if (_counters.TryGetValue(type, out var counter))
        {
            counter.SetCount(newTotal);
        }
    }
}

/// <summary>
/// Individual resource counter with icon and count.
/// </summary>
public partial class ResourceCounter : HBoxContainer
{
    private Control _iconContainer;
    private Label _countLabel;
    private int _count;
    private ResourceType _type;

    public void Setup(ResourceType type, Texture2D icon, Color fallbackColor, int iconSize)
    {
        _type = type;

        // Icon container for animations
        _iconContainer = new Control();
        _iconContainer.CustomMinimumSize = new Vector2(iconSize, iconSize);
        _iconContainer.PivotOffset = new Vector2(iconSize / 2f, iconSize / 2f);
        AddChild(_iconContainer);

        if (icon != null)
        {
            // Use texture
            var textureRect = new TextureRect();
            textureRect.Texture = icon;
            textureRect.CustomMinimumSize = new Vector2(iconSize, iconSize);
            textureRect.ExpandMode = TextureRect.ExpandModeEnum.FitWidthProportional;
            textureRect.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
            _iconContainer.AddChild(textureRect);
        }
        else
        {
            // Fallback to colored square
            var colorRect = new ColorRect();
            colorRect.CustomMinimumSize = new Vector2(iconSize, iconSize);
            colorRect.Color = fallbackColor;
            _iconContainer.AddChild(colorRect);
        }

        // Count label
        _countLabel = new Label();
        _countLabel.Text = "0";
        _countLabel.VerticalAlignment = VerticalAlignment.Center;
        _countLabel.CustomMinimumSize = new Vector2(40, 0);
        AddChild(_countLabel);

        // Tooltip
        TooltipText = ResourceInfo.GetDisplayName(type);

        AddThemeConstantOverride("separation", 4);
    }

    public void SetCount(int count)
    {
        _count = count;
        _countLabel.Text = FormatCount(count);

        // Pulse animation on change
        var tween = CreateTween();
        tween.TweenProperty(_iconContainer, "scale", new Vector2(1.2f, 1.2f), 0.1f);
        tween.TweenProperty(_iconContainer, "scale", new Vector2(1.0f, 1.0f), 0.1f);
    }

    private string FormatCount(int count)
    {
        if (count >= 1000000)
            return $"{count / 1000000f:F1}M";
        if (count >= 1000)
            return $"{count / 1000f:F1}K";
        return count.ToString();
    }
}
