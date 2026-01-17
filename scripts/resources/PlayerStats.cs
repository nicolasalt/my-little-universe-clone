using Godot;

[GlobalClass]
public partial class PlayerStats : Resource
{
    [Export] public float MaxHealth { get; set; } = 100f;
    [Export] public float MaxStamina { get; set; } = 100f;
    [Export] public float MaxMana { get; set; } = 50f;
    [Export] public float StaminaRegenRate { get; set; } = 15f;
    [Export] public float ManaRegenRate { get; set; } = 5f;
    [Export] public float StaminaDrainRate { get; set; } = 20f;

    public float CurrentHealth { get; private set; }
    public float CurrentStamina { get; private set; }
    public float CurrentMana { get; private set; }

    public void Initialize()
    {
        CurrentHealth = MaxHealth;
        CurrentStamina = MaxStamina;
        CurrentMana = MaxMana;
    }

    public void TakeDamage(float amount)
    {
        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        SignalBus.Instance?.EmitSignal(SignalBus.SignalName.HealthChanged, CurrentHealth, MaxHealth);

        if (CurrentHealth <= 0)
        {
            SignalBus.Instance?.EmitSignal(SignalBus.SignalName.PlayerDied);
        }
    }

    public void Heal(float amount)
    {
        CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
        SignalBus.Instance?.EmitSignal(SignalBus.SignalName.HealthChanged, CurrentHealth, MaxHealth);
    }

    public bool UseStamina(float amount)
    {
        if (CurrentStamina >= amount)
        {
            CurrentStamina -= amount;
            SignalBus.Instance?.EmitSignal(SignalBus.SignalName.StaminaChanged, CurrentStamina, MaxStamina);
            return true;
        }
        return false;
    }

    public void DrainStamina(float delta)
    {
        CurrentStamina = Mathf.Max(0, CurrentStamina - StaminaDrainRate * delta);
        SignalBus.Instance?.EmitSignal(SignalBus.SignalName.StaminaChanged, CurrentStamina, MaxStamina);
    }

    public void RegenStamina(float delta)
    {
        if (CurrentStamina < MaxStamina)
        {
            CurrentStamina = Mathf.Min(MaxStamina, CurrentStamina + StaminaRegenRate * delta);
            SignalBus.Instance?.EmitSignal(SignalBus.SignalName.StaminaChanged, CurrentStamina, MaxStamina);
        }
    }

    public bool UseMana(float amount)
    {
        if (CurrentMana >= amount)
        {
            CurrentMana -= amount;
            SignalBus.Instance?.EmitSignal(SignalBus.SignalName.ManaChanged, CurrentMana, MaxMana);
            return true;
        }
        return false;
    }

    public void RegenMana(float delta)
    {
        if (CurrentMana < MaxMana)
        {
            CurrentMana = Mathf.Min(MaxMana, CurrentMana + ManaRegenRate * delta);
            SignalBus.Instance?.EmitSignal(SignalBus.SignalName.ManaChanged, CurrentMana, MaxMana);
        }
    }
}
