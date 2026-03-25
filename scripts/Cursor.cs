using Godot;

public partial class Cursor : Marker3D
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public static Vector3 ScreenPointToRay()
    {
        return Vector3.Up;
    }
}