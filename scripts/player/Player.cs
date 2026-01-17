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

    // Animation references
    private Node3D _characterModel;
    private Node3D _head;
    private Node3D _body;
    private Node3D _armLeft;
    private Node3D _armRight;
    private Node3D _legLeft;
    private Node3D _legRight;

    // Animation state
    private float _animTime = 0f;
    private float _bobAmount = 0f;
    private bool _wasOnFloor = true;

    // Gather animation state
    private GatherController _gatherController;
    private float _gatherAnimTime = 0f;

    public float Gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

    public override void _Ready()
    {
        _cameraArm = GetNode<Node3D>("CameraArm");
        _camera = GetNode<Camera3D>("CameraArm/Camera3D");

        // Get animation references
        _characterModel = GetNodeOrNull<Node3D>("CharacterModel");
        if (_characterModel != null)
        {
            _head = _characterModel.GetNodeOrNull<Node3D>("Head");
            _body = _characterModel.GetNodeOrNull<Node3D>("Body");
            _armLeft = _characterModel.GetNodeOrNull<Node3D>("ArmLeft");
            _armRight = _characterModel.GetNodeOrNull<Node3D>("ArmRight");
            _legLeft = _characterModel.GetNodeOrNull<Node3D>("LegLeft");
            _legRight = _characterModel.GetNodeOrNull<Node3D>("LegRight");
        }

        Input.MouseMode = Input.MouseModeEnum.Captured;

        if (Stats == null)
        {
            Stats = new PlayerStats();
        }
        Stats.Initialize();

        // Get gather controller reference
        _gatherController = GetNodeOrNull<GatherController>("GatherController");

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

        // Animate character
        bool isGathering = _gatherController != null && _gatherController.IsGathering;
        AnimateCharacter((float)delta, direction != Vector3.Zero, isGathering);

        // Land squash effect
        if (IsOnFloor() && !_wasOnFloor)
        {
            SquashOnLand();
        }
        _wasOnFloor = IsOnFloor();
    }

    private void AnimateCharacter(float delta, bool isMoving, bool isGathering = false)
    {
        if (_characterModel == null) return;

        // Gathering animation takes priority when not moving
        if (isGathering && !isMoving)
        {
            AnimateGathering(delta);
            return;
        }

        float speed = _isSprinting ? 14f : 10f;
        float limbSwing = _isSprinting ? 0.5f : 0.35f;
        float bodyBob = _isSprinting ? 0.08f : 0.05f;

        if (isMoving)
        {
            _animTime += delta * speed;
            _gatherAnimTime = 0f; // Reset gather animation when moving

            // Arm swing (opposite to legs)
            if (_armLeft != null)
                _armLeft.Rotation = new Vector3(Mathf.Sin(_animTime) * limbSwing, 0, 0);
            if (_armRight != null)
                _armRight.Rotation = new Vector3(-Mathf.Sin(_animTime) * limbSwing, 0, 0);

            // Leg swing
            if (_legLeft != null)
                _legLeft.Rotation = new Vector3(-Mathf.Sin(_animTime) * limbSwing, 0, 0);
            if (_legRight != null)
                _legRight.Rotation = new Vector3(Mathf.Sin(_animTime) * limbSwing, 0, 0);

            // Body bob
            _bobAmount = Mathf.Abs(Mathf.Sin(_animTime * 2)) * bodyBob;
            if (_body != null)
                _body.Position = new Vector3(0, 0.85f + _bobAmount, 0);
            if (_head != null)
                _head.Position = new Vector3(0, 1.42f + _bobAmount, 0);
        }
        else
        {
            // Idle breathing animation
            _animTime += delta * 2f;
            _gatherAnimTime = 0f; // Reset gather animation when idle

            float idleBob = Mathf.Sin(_animTime) * 0.02f;
            if (_body != null)
                _body.Position = new Vector3(0, 0.85f + idleBob, 0);
            if (_head != null)
                _head.Position = new Vector3(0, 1.42f + idleBob, 0);

            // Reset limbs smoothly
            if (_armLeft != null)
                _armLeft.Rotation = _armLeft.Rotation.Lerp(Vector3.Zero, delta * 8f);
            if (_armRight != null)
                _armRight.Rotation = _armRight.Rotation.Lerp(Vector3.Zero, delta * 8f);
            if (_legLeft != null)
                _legLeft.Rotation = _legLeft.Rotation.Lerp(Vector3.Zero, delta * 8f);
            if (_legRight != null)
                _legRight.Rotation = _legRight.Rotation.Lerp(Vector3.Zero, delta * 8f);
        }
    }

    private void AnimateGathering(float delta)
    {
        // Rhythmic chopping/swinging motion
        float gatherSpeed = 8f; // Speed of the gather animation
        _gatherAnimTime += delta * gatherSpeed;

        // Arms swing down together in a chopping motion
        float armSwing = Mathf.Sin(_gatherAnimTime) * 0.6f;
        // Add a slight forward lean
        float forwardLean = Mathf.Abs(Mathf.Sin(_gatherAnimTime)) * 0.15f;

        if (_armLeft != null)
            _armLeft.Rotation = new Vector3(-armSwing - 0.3f, 0, 0);
        if (_armRight != null)
            _armRight.Rotation = new Vector3(-armSwing - 0.3f, 0, 0);

        // Slight body bob with gather rhythm
        float gatherBob = Mathf.Abs(Mathf.Sin(_gatherAnimTime)) * 0.04f;
        if (_body != null)
        {
            _body.Position = new Vector3(0, 0.85f + gatherBob, 0);
            _body.Rotation = new Vector3(forwardLean, 0, 0);
        }
        if (_head != null)
        {
            _head.Position = new Vector3(0, 1.42f + gatherBob, 0);
            _head.Rotation = new Vector3(forwardLean * 0.5f, 0, 0);
        }

        // Legs stay relatively still, maybe a slight stance
        if (_legLeft != null)
            _legLeft.Rotation = _legLeft.Rotation.Lerp(new Vector3(-0.1f, 0, 0), delta * 5f);
        if (_legRight != null)
            _legRight.Rotation = _legRight.Rotation.Lerp(new Vector3(0.1f, 0, 0), delta * 5f);
    }

    private void SquashOnLand()
    {
        if (_characterModel == null) return;

        // Quick squash and stretch effect using a tween
        var tween = CreateTween();
        tween.TweenProperty(_characterModel, "scale", new Vector3(1.2f, 0.8f, 1.2f), 0.05f);
        tween.TweenProperty(_characterModel, "scale", new Vector3(0.9f, 1.1f, 0.9f), 0.08f);
        tween.TweenProperty(_characterModel, "scale", Vector3.One, 0.1f);
    }
}
