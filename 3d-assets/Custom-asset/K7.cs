using Godot;

public partial class K7 : Node3D
{
	private Marker3D marker;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		marker = GetParent().GetNode<Marker3D>("K7Pos");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		GlobalPosition = GlobalPosition.Lerp(marker.GlobalPosition, (float)delta * 4);
	}
}
