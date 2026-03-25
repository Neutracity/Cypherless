using Godot;

public partial class PeerToPeer : Node2D
{
    [Export] private string ip = "localhost";
    private ENetMultiplayerPeer peer = new();

    [Export] private PackedScene playerScene;

    private Node2D ui;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        ui = GetNode<Node2D>("Ui");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    private void _on_host_pressed()
    {
        peer.CreateServer(4242);
        Multiplayer.MultiplayerPeer = peer;
        GetTree().Root.AddChild(GD.Load<PackedScene>("res://scenes/Maps/arcade.tscn").Instantiate());
        ui.Hide();
        Multiplayer.PeerConnected += _add_player;
        _add_player();
    }

    public void _add_player(long id = 1)
    {
        var player = playerScene.Instantiate();
        player.Name = id.ToString();
        CallDeferred("add_child", player);
    }

    public void _on_join_pressed()
    {
        peer.CreateClient(ip, 4242);
        GetTree().Root.AddChild(GD.Load<PackedScene>("res://scenes/Maps/arcade.tscn").Instantiate());
        ui.Hide();
        Multiplayer.MultiplayerPeer = peer;
    }
}