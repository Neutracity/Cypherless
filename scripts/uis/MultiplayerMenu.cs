using Godot;
using System;
using Cypherless;

public partial class MultiplayerMenu : Control
{
	private ENetMultiplayerPeer peer = new(); 
	private PackedScene playerScene;
	[Export] private string ip = "localhost";
	[Export] private bool debug = false;
	private Marker3D spawnPoint;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		playerScene = GD.Load<PackedScene>("res://scenes/player.tscn");
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("ui_pause"))
			_on_back_button_pressed();
	}

	public void _on_back_button_pressed()
	{
		SoundManager.Instance.PlaySound(SoundEffect.UI_button);
		GetTree().Root.AddChild(GD.Load<PackedScene>("res://scenes/UIs/Menus/main_menu.tscn").Instantiate());
		QueueFree();
	}

	public void _on_join_button_pressed()
	{
		SoundManager.Instance.StopMusic();
		SoundManager.Instance.PlaySound(SoundEffect.UI_button);
		String t = GetNode<TextEdit>("ScrollContainer/HBoxContainer/MarginContainer/VBoxContainer/VBoxContainer/MarginContainer/ipaddr").Text;
		ip = t != "" ? t : ip;
		Error error =  peer.CreateClient(ip, 4242);
		if (error != Error.Ok)
		{
			return;
		}

		var map = GD.Load<PackedScene>(debug ?"res://scenes/Maps/debug_test_online.tscn" : "res://scenes/Maps/arene_online.tscn").Instantiate();
		spawnPoint = map.GetNode<Marker3D>("Arene/SpawnPoint");
		GetTree().Root.AddChild(map);
		Hide();
		Multiplayer.MultiplayerPeer = peer;
	}

	public void _on_host_button_pressed()
	{
		SoundManager.Instance.StopMusic();
		SoundManager.Instance.PlaySound(SoundEffect.UI_button);
		Error error = peer.CreateServer(4242);
		if (error != Error.Ok)
			return;
		Multiplayer.MultiplayerPeer = peer;
		var map = GD.Load<PackedScene>(debug ?"res://scenes/Maps/debug_test_online.tscn" : "res://scenes/Maps/arene_online.tscn").Instantiate();
		spawnPoint = map.GetNode<Marker3D>("Arene/SpawnPoint");
		GetTree().Root.AddChild(map);
		Hide();
		Multiplayer.PeerConnected += _add_player;
		_add_player(Multiplayer.GetUniqueId());


	}

	public void _on_local_button_pressed()
	{
		SoundManager.Instance.StopMusic();
		SoundManager.Instance.PlaySound(SoundEffect.UI_button);
		GetTree().ChangeSceneToPacked(GD.Load<PackedScene>("res://scenes/Maps/new_city.tscn"));
		QueueFree();
		
	}
	
	public void _add_player(long id = 1)
	{
		Player player = playerScene.Instantiate() as Player;
		GD.Print($"Peer id :{id.ToString()} has joined the game !");
		player.Name = id.ToString();
		CallDeferred("add_child", player);
		player.GlobalPosition = spawnPoint.GlobalPosition;
	}
	
}
