using Godot;
using System;
using Cypherless;

public partial class Metro : Node3D
{
    private bool _eventstarted = false;
    private CollisionShape3D _box;
    private Node3D _box1;
    private Player _player;

    public override void _Ready()
    {
        _box = GetNode<CollisionShape3D>("NavigationRegion3D/StaticBody3D/Aaaaa");
        _box1 = GetNode<Node3D>("Node3D/AAAAA");
        foreach (Node3D child in _box1.GetChildren())
        {
            child.Hide();
        }
            
        _box.Disabled = true;
        
        
    }
    private void _on_area_3d_body_entered(Node3D body)
    {
        if (body is Player p && !_eventstarted)
        {
            _eventstarted = true;
            _player = p;
            foreach (Node3D child in _box1.GetChildren())
            {
                child.Show();
            }
            
            _box.Disabled = false;
            
            var explosionTimer= new Timer();
            explosionTimer.OneShot = true;
            explosionTimer.WaitTime = 10;
            explosionTimer.Timeout += OnExplosionTimerTimeout;
        }
    }

    private void OnExplosionTimerTimeout()
    {
       SoundManager.Instance.PlaySound(SoundEffect.explosion_building); 
       SoundManager.Instance.PlaySound(SoundEffect.tremblement_de_terre);
       GetTree().Root.AddChild(GD.Load<PackedScene>("res://scenes/EcranNoir.tscn").Instantiate());
       GetParent().CallDeferred("queue_free");
    }
    
}
