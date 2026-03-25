using Godot;
using System;

public partial class DeathScreen : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetTree().Paused = true;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("ui_pause"))
			GetTree().Paused = true;
	}

	public void _on_resume_button_pressed()
	{
		SoundManager.Instance.PlaySound(SoundEffect.UI_button);
		Node loading = GD.Load<PackedScene>("res://scenes/UIs/Menus/loading_screen.tscn").Instantiate();
		((LoadingProgress)loading).Path = "res://scenes/tutorial.tscn";
		QueueFree();
		GetTree().Root.AddChild(loading);
	}

	public void _on_main_menu_button_pressed()
	{
		SoundManager.Instance.PlaySound(SoundEffect.UI_button);
		GetTree().Root.AddChild(GD.Load<PackedScene>("res://scenes/UIs/Menus/main_menu.tscn").Instantiate());
		GetParent().QueueFree();
		GetTree().Paused = false;
	}
}
