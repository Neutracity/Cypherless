using Godot;
using System.Net;
using System.Net.Sockets;

public partial class PauseMenu : Control
{
	private string GetLocalIpAddress()
	{
		string ip = "Non détectée";
		foreach (var ipAddr in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
		{
			if (ipAddr.AddressFamily == AddressFamily.InterNetwork)
			{
				ip = ipAddr.ToString();
				break;
			}
		}
		return ip;
	}
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Hide();
		GetNode<Label>("multiplayer_info/VBoxContainer/ip_adress").Text = $"{GetLocalIpAddress()}";
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("ui_pause"))
		{
			GD.Print("pause menu escape");
			if (GetTree().Paused)
			{
				Input.SetCustomMouseCursor(ResourceLoader.Load("res://2d-assets/Other/crosshair.png"), hotspot: new Vector2(21, 21));
				Hide();
				GetTree().Paused = false;
			}
			else
			{
				Input.SetCustomMouseCursor(null);
				GetTree().Paused = true;
				Show();
			}
		}
	}

	public void _on_button_resume_pressed()
	{
		Hide();
		GetTree().Paused = false;
		SoundManager.Instance.PlaySound(SoundEffect.UI_button);
	}

	public void _on_button_options_pressed()
	{
		AddChild(GD.Load<PackedScene>("res://scenes/UIs/Menus/settings_menu.tscn").Instantiate());
		SoundManager.Instance.PlaySound(SoundEffect.UI_button);
	}

	public void _on_button_save_pressed()
	{
		GetTree().Root.AddChild(GD.Load<PackedScene>("res://scenes/DEBUG_TestScene.tscn").Instantiate());
		GetParent().QueueFree();
		GetTree().Paused = false;
		SoundManager.Instance.PlaySound(SoundEffect.UI_button);
	}

	public void _on_button_main_menu_pressed()
	{
		GetTree().Root.AddChild(GD.Load<PackedScene>("res://scenes/UIs/Menus/main_menu.tscn").Instantiate());
		GetParent().QueueFree();
		GetTree().Paused = false;
		SoundManager.Instance.PlaySound(SoundEffect.UI_button);
	}

	public void _on_button_leave_game_pressed()
	{
		GetTree().Quit();
		SoundManager.Instance.PlaySound(SoundEffect.UI_button);
	}
}
