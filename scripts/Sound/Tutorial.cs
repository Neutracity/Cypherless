using Godot;
using System;

public partial class Tutorial : Node
{
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
{
	SoundManager sound = (SoundManager)GetNode("/root/SoundManager");
	sound.StopMusic(); // ou sound.PlayMusic();
}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
