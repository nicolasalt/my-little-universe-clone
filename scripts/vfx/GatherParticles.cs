using Godot;

/// <summary>
/// Particle effect spawned when a resource is hit during gathering.
/// Self-destructs after particles finish.
/// </summary>
public partial class GatherParticles : GpuParticles3D
{
    [Export] public float ParticleLifetime = 0.8f;
    [Export] public int ParticleCount = 8;

    private static readonly Color WoodColor = new Color(0.55f, 0.35f, 0.2f);
    private static readonly Color StoneColor = new Color(0.5f, 0.5f, 0.5f);
    private static readonly Color OreColor = new Color(0.6f, 0.45f, 0.3f);
    private static readonly Color DefaultColor = new Color(0.8f, 0.8f, 0.8f);

    public override void _Ready()
    {
        Emitting = true;
        OneShot = true;
        Amount = ParticleCount;
        Lifetime = ParticleLifetime;

        // Self-destruct after particles finish
        var timer = GetTree().CreateTimer(ParticleLifetime + 0.5f);
        timer.Timeout += QueueFree;
    }

    /// <summary>
    /// Configure particles for a specific resource type.
    /// </summary>
    public void SetResourceType(ResourceType type)
    {
        Color color = type switch
        {
            ResourceType.Wood => WoodColor,
            ResourceType.Stone => StoneColor,
            ResourceType.IronOre => OreColor,
            _ => DefaultColor
        };

        // Create and configure the particle material
        var material = new ParticleProcessMaterial();
        material.EmissionShape = ParticleProcessMaterial.EmissionShapeEnum.Sphere;
        material.EmissionSphereRadius = 0.3f;
        material.Direction = new Vector3(0, 1, 0);
        material.Spread = 45f;
        material.InitialVelocityMin = 1.5f;
        material.InitialVelocityMax = 3f;
        material.Gravity = new Vector3(0, -8f, 0);
        material.ScaleMin = 0.05f;
        material.ScaleMax = 0.12f;
        material.Color = color;

        ProcessMaterial = material;

        // Create a simple mesh for particles
        var mesh = new BoxMesh();
        mesh.Size = new Vector3(0.1f, 0.1f, 0.1f);
        DrawPass1 = mesh;
    }

    /// <summary>
    /// Factory method to create and spawn gather particles at a position.
    /// </summary>
    public static GatherParticles SpawnAt(Node parent, Vector3 position, ResourceType type)
    {
        var particles = new GatherParticles();
        particles.SetResourceType(type);
        parent.AddChild(particles);
        particles.GlobalPosition = position;
        return particles;
    }
}
