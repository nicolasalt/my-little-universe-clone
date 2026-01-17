using Godot;
using System.Text;

/// <summary>
/// World-space UI that shows unlock cost and progress above a hex.
/// </summary>
public partial class HexUnlockUI : Node3D
{
    private Label3D _costLabel;
    private Label3D _progressLabel;
    private Vector2I _targetCoords;
    private bool _isActive;

    public static HexUnlockUI Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;

        // Create cost label
        _costLabel = new Label3D();
        _costLabel.FontSize = 64;
        _costLabel.OutlineSize = 8;
        _costLabel.Modulate = new Color(1, 1, 1, 1);
        _costLabel.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;
        _costLabel.NoDepthTest = true;
        _costLabel.Position = new Vector3(0, 2.5f, 0);
        AddChild(_costLabel);

        // Create progress label
        _progressLabel = new Label3D();
        _progressLabel.FontSize = 48;
        _progressLabel.OutlineSize = 6;
        _progressLabel.Modulate = new Color(0.8f, 0.9f, 0.5f, 1);
        _progressLabel.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;
        _progressLabel.NoDepthTest = true;
        _progressLabel.Position = new Vector3(0, 1.8f, 0);
        AddChild(_progressLabel);

        Hide();

        // Connect to signals
        if (SignalBus.Instance != null)
        {
            SignalBus.Instance.HexUnlockStarted += OnUnlockStarted;
            SignalBus.Instance.HexUnlockProgress += OnUnlockProgress;
            SignalBus.Instance.HexUnlocked += OnUnlocked;
        }
    }

    public override void _ExitTree()
    {
        if (SignalBus.Instance != null)
        {
            SignalBus.Instance.HexUnlockStarted -= OnUnlockStarted;
            SignalBus.Instance.HexUnlockProgress -= OnUnlockProgress;
            SignalBus.Instance.HexUnlocked -= OnUnlocked;
        }
    }

    private void OnUnlockStarted(Vector2I coords)
    {
        _targetCoords = coords;
        _isActive = true;

        // Position above the hex
        if (HexGridManager.Instance != null)
        {
            GlobalPosition = HexGridManager.Instance.HexToWorld(coords);
        }

        UpdateDisplay();
        Show();
    }

    private void OnUnlockProgress(Vector2I coords, float progress)
    {
        if (coords != _targetCoords) return;

        UpdateDisplay();
    }

    private void OnUnlocked(Vector2I coords)
    {
        if (coords != _targetCoords) return;

        _isActive = false;
        Hide();
    }

    private void UpdateDisplay()
    {
        if (HexGridManager.Instance == null) return;

        var tile = HexGridManager.Instance.GetTile(_targetCoords);
        if (tile == null) return;

        // Build cost string
        var costBuilder = new StringBuilder();
        foreach (var (type, cost) in tile.UnlockCost)
        {
            int remaining = tile.GetRemaining(type);
            if (remaining > 0)
            {
                if (costBuilder.Length > 0) costBuilder.Append("  ");
                costBuilder.Append($"{ResourceInfo.GetDisplayName(type)}: {remaining}");
            }
        }
        _costLabel.Text = costBuilder.ToString();

        // Show progress percentage
        float progress = tile.GetProgress();
        _progressLabel.Text = $"{Mathf.RoundToInt(progress * 100)}%";
    }

    public void SetTarget(Vector2I? coords)
    {
        if (!coords.HasValue)
        {
            _isActive = false;
            Hide();
            return;
        }

        var tile = HexGridManager.Instance?.GetTile(coords.Value);
        if (tile == null || tile.State == HexState.Unlocked)
        {
            _isActive = false;
            Hide();
            return;
        }

        _targetCoords = coords.Value;
        _isActive = true;
        GlobalPosition = HexGridManager.Instance.HexToWorld(coords.Value);
        UpdateDisplay();
        Show();
    }
}
