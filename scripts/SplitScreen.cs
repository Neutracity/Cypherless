using Godot;

public partial class SplitScreen : Node
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        var World = GetNode<SubViewport>("HBoxContainer/SubViewportContainer/SubViewport").FindWorld3D();
        GetNode<SubViewport>("HBoxContainer/SubViewportContainer2/SubViewport").World3D = World;
    }
}