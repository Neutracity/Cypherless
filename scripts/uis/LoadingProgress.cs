using System;
using System.Net;
using Godot;

public partial class LoadingProgress : Control
{
	ProgressBar _progressBar;
	PackedScene _loadingScene;
	public int division;
	public string Path;

	public int Percentage;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_progressBar = GetNode<ProgressBar>("background/cypherless_logo/loading_progress");
		Percentage = 0;
		division = 1;
	}

	public override void _EnterTree()
	{

		ResourceLoader.LoadThreadedRequest(Path,"", true);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// To delete
		
		
		Percentage++;
		if (Percentage >= 100*division)
		{
			Percentage = 0;
			division = ((int)GD.Randi()%10) + 1;
		}
		if (division == 0)
			division = 1;
		_progressBar.Value = Percentage/division;

		if (ResourceLoader.LoadThreadedGetStatus(Path) == ResourceLoader.ThreadLoadStatus.Loaded)
		{
			Input.SetCustomMouseCursor(ResourceLoader.Load("res://2d-assets/Other/crosshair.png"), hotspot: new Vector2(21, 21));
			GetTree().ChangeSceneToPacked((PackedScene)ResourceLoader.LoadThreadedGet(Path));
			QueueFree();
		}

		if (ResourceLoader.LoadThreadedGetStatus(Path) == ResourceLoader.ThreadLoadStatus.Failed)
		{
			GD.PrintErr("Loading failed");
		}
	}
}
