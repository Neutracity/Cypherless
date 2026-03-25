using Godot;

public partial class Cardboard : Node3D
{
    public override void _Ready()
    {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        var cpuParticles = GetNode<CpuParticles3D>("RigidBody3D/POP");
        cpuParticles.Emitting = GetNode<RigidBody3D>("RigidBody3D").LinearVelocity > Vector3.One;
    }
}