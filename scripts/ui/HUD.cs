using Godot;

public partial class HUD : CanvasLayer
{
    private ProgressBar _healthBar;
    private ProgressBar _staminaBar;
    private ProgressBar _manaBar;
    private Label _interactionPrompt;

    public override void _Ready()
    {
        _healthBar = GetNode<ProgressBar>("MarginContainer/VBoxContainer/HealthBar");
        _staminaBar = GetNode<ProgressBar>("MarginContainer/VBoxContainer/StaminaBar");
        _manaBar = GetNode<ProgressBar>("MarginContainer/VBoxContainer/ManaBar");
        _interactionPrompt = GetNode<Label>("InteractionPrompt");

        _interactionPrompt.Visible = false;

        // Connect to signals
        SignalBus.Instance.HealthChanged += OnHealthChanged;
        SignalBus.Instance.StaminaChanged += OnStaminaChanged;
        SignalBus.Instance.ManaChanged += OnManaChanged;
        SignalBus.Instance.InteractionPrompt += OnInteractionPrompt;
    }

    public override void _ExitTree()
    {
        if (SignalBus.Instance != null)
        {
            SignalBus.Instance.HealthChanged -= OnHealthChanged;
            SignalBus.Instance.StaminaChanged -= OnStaminaChanged;
            SignalBus.Instance.ManaChanged -= OnManaChanged;
            SignalBus.Instance.InteractionPrompt -= OnInteractionPrompt;
        }
    }

    private void OnHealthChanged(float current, float max)
    {
        _healthBar.MaxValue = max;
        _healthBar.Value = current;
    }

    private void OnStaminaChanged(float current, float max)
    {
        _staminaBar.MaxValue = max;
        _staminaBar.Value = current;
    }

    private void OnManaChanged(float current, float max)
    {
        _manaBar.MaxValue = max;
        _manaBar.Value = current;
    }

    private void OnInteractionPrompt(string promptText, bool show)
    {
        _interactionPrompt.Text = promptText;
        _interactionPrompt.Visible = show;
    }
}
