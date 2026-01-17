using Godot;

public partial class Player : CharacterBody3D
{
    [Export] public float WalkSpeed = 5.0f;
    [Export] public float SprintSpeed = 8.0f;
    [Export] public float JumpVelocity = 4.5f;
    [Export] public float MouseSensitivity = 0.003f;
    [Export] public PlayerStats Stats;

    private Node3D _cameraArm;
    private Camera3D _camera;
    private float _cameraArmRotationX = 0f;
    private bool _isSprinting = false;

    public float Gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

    public override void _Ready()
    {
        _cameraArm = GetNode<Node3D>("CameraArm");
        _camera = GetNode<Camera3D>("CameraArm/Camera3D");

        Input.MouseMode = Input.MouseModeEnum.Captured;

        if (Stats == null)
        {
            Stats = new PlayerStats();
        }
        Stats.Initialize();

        GameManager.Instance.CurrentPlayer = this;
        SignalBus.Instance?.EmitSignal(SignalBus.SignalName.PlayerSpawned, this);

        // Emit initial stats
        SignalBus.Instance?.EmitSignal(SignalBus.SignalName.HealthChanged, Stats.CurrentHealth, Stats.MaxHealth);
        SignalBus.Instance?.EmitSignal(SignalBus.SignalName.StaminaChanged, Stats.CurrentStamina, Stats.MaxStamina);
        SignalBus.Instance?.EmitSignal(SignalBus.SignalName.ManaChanged, Stats.CurrentMana, Stats.MaxMana);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured)
        {
            // Rotate player body horizontally
            RotateY(-mouseMotion.Relative.X * MouseSensitivity);

            // Rotate camera arm vertically
            _cameraArmRotationX -= mouseMotion.Relative.Y * MouseSensitivity;
            _cameraArmRotationX = Mathf.Clamp(_cameraArmRotationX, Mathf.DegToRad(-80), Mathf.DegToRad(80));
            _cameraArm.Rotation = new Vector3(_cameraArmRotationX, 0, 0);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector3 velocity = Velocity;

        // Apply gravity
        if (!IsOnFloor())
        {
            velocity.Y -= Gravity * (float)delta;
        }

        // Handle jump
        if (Input.IsActionJustPressed("jump") && IsOnFloor())
        {
            velocity.Y = JumpVelocity;
        }

        // Handle sprint
        _isSprinting = Input.IsActionPressed("sprint") && Stats.CurrentStamina > 0;

        // Get input direction
        Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_back");
        Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

        float currentSpeed = _isSprinting ? SprintSpeed : WalkSpeed;

        if (direction != Vector3.Zero)
        {
            velocity.X = direction.X * currentSpeed;
            velocity.Z = direction.Z * currentSpeed;

            // Drain stamina while sprinting and moving
            if (_isSprinting)
            {
                Stats.DrainStamina((float)delta);
            }
        }
        else
        {
            velocity.X = Mathf.MoveToward(Velocity.X, 0, currentSpeed);
            velocity.Z = Mathf.MoveToward(Velocity.Z, 0, currentSpeed);
        }

        // Regenerate stamina when not sprinting
        if (!_isSprinting)
        {
            Stats.RegenStamina((float)delta);
        }

        // Regenerate mana
        Stats.RegenMana((float)delta);

        Velocity = velocity;
        MoveAndSlide();
    }
}
