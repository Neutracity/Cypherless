using System;
using Cypherless;
using Godot;

public partial class RedDamageFade : TextureRect
{
    public AnimationPlayer Anim;
    private Player _player;
    
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Anim = GetNode<AnimationPlayer>("AnimationPlayer");
        _player = GetParent<Node>().GetParent().GetParent<Player>();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (_player.Health <= 0)
        {
            Anim.Play("Die",1);
        }
        else
        {
            Anim.Play("Fade");
        }
    }
}