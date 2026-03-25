using Cypherless;
using Godot;

public partial class Bullet : RigidBody3D
{
    public int PlayerIndex = 0;
    public string Sname = "0";
    public Node Shooter { get; set; } = null;
    private int Speed { get; set; } = 1;
    public int Damage { get; set; } = 1;
    private int Bounces { get; set; } = 0;
    private CpuParticles3D _bounceParticles;
    private CpuParticles3D _bloodParticles;

    public override void _Ready()
    {
        ContactMonitor = true;
        MaxContactsReported = 4;
        SetPhysicsProcess(true);
        _bounceParticles = GetNode<CpuParticles3D>("BounceParticles");
        _bloodParticles = GetNode<CpuParticles3D>("BloodParticles");
    }

    public void OnBodyEntered(Node3D body)
    {
        Bounces += 1;
        GD.Print($"Bounces = {Bounces}");
        _bounceParticles.Emitting = true;
        if (Bounces == 2)
        {
            QueueFree();
        }
    }

    public void _on_area_3d_body_entered(Node3D body)
    {
        if (!(body is Player p && p == Shooter ) && body is Character victim )
        {
            _bloodParticles.Emitting = true;
            victim.TakeDamage(Damage);
            GD.Print("Hit");
            QueueFree();
        }
    }
}