using Godot;

public partial class Mechant : Node3D
{
    // Called when the node enters the scene tree for the first time.
    [Export] private int pv = 5;

    public override void _Ready()
    {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public void Hit(int damage = 1)
    {
        pv -= damage;
        if (pv == 0) QueueFree();
    }
}