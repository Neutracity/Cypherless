using Godot;
using System;
using Cypherless;
using Godot.Collections;

namespace Cypherless;

public partial class HackableMetro : StaticBody3D, IInteractable
{
    enum HacksTypes
    { 
        Qte,
        Level,
        DigiCode,
    }
    
    [Export] HacksTypes _hacksType = HacksTypes.Qte;
    private bool _alreadyHacked = false;
    private MeshInstance3D _mesh;
    private ShaderMaterial _nextpass;
    private PackedScene _hackScene;
    private Node _hackNode;
    private StaticBody3D _col;

    public override void _Ready()
    {
        _mesh = GetNode<MeshInstance3D>("frontDoor1");
        _nextpass = _mesh.GetMaterialOverlay() as ShaderMaterial;
        _col = GetNode<StaticBody3D>("StaticBody3D");
        LoadHackScene();
    }

    public void LoadHackScene()
    {
        switch (_hacksType)
        {
            case HacksTypes.Qte:
                _hackScene = GD.Load<PackedScene>("res://scenes/UIs/Hacks/hack_qte.tscn");
                break;
            case HacksTypes.Level:
                _hackScene = GD.Load<PackedScene>("res://scenes/UIs/Hacks/hack_levels.tscn");
                break;
            case HacksTypes.DigiCode:
                _hackScene = GD.Load<PackedScene>("res://scenes/UIs/Hacks/hack_digicode.tscn");
                break;
        }
        
    }

    public override void _Process(double delta)
    {
        if (_hackNode != null && _hackNode is IHack hack)
        {
            if (hack.Done)
            {
                _hackNode.QueueFree();
                _hackNode = null;
                _col.QueueFree();
                CollisionLayer = 0;
                CollisionMask = 0;
                _alreadyHacked = true;  
            }
        }
        else
        {
            //GD.Print("Not IHACK");
        }
    }

    public void Interact()
    {
        if (!_alreadyHacked && _hackNode is null)
        {
            _hackNode = _hackScene.Instantiate();
            GetTree().Root.CallDeferred("add_child",_hackNode);
        }
        else
        {
            
            _hackNode?.QueueFree();
            _hackNode = null;
            LoadHackScene();
        }
    }

    public void IsNowInteractable()
    {
        _nextpass.SetShaderParameter("visible", 1);
    }

    public void NoMoreInteractable()
    {
        _nextpass.SetShaderParameter("visible", 0);
        if (_hackNode is not null)
        {
            _hackNode.QueueFree();
            _hackNode = null;
            LoadHackScene();
        }
    }
}