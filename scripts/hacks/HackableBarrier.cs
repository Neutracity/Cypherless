using Godot;
using System;
using Godot;
using System;
using Cypherless;
using Godot.Collections;

namespace Cypherless;

public partial class HackableBarrier : StaticBody3D, IInteractable
{
    enum HacksTypes
    { 
        Qte,
        Level,
        DigiCode,
    }
    private Dictionary<int, string> dialogue = new()
    {
        { 0, "Attention ! La barrière est électrifié, je peux t'aider à la pirater il suffit juste que tu appuie sur les boutons au bon moment ! Tu devrais appuyer sur E pour la pirater" },
        { 1, "Bonjour, je suis David, le plus gros bg de Cypherless, aujourd'hui on va faire des crêpes avec le poto K7, et je vous propose pour cela de commencer par verser la farine dans le bol, puis l'eau, et la pate est prete !"},
        { 2, "Vous n'avez pas le droit d'êtres ici !"},
        {3, "On m'as dit que le code n'était pas très sécurisé "}
    };
    private Dictionary<int, string> pnjNames = new()
    {
        { 0, "K7" },
        { 1, "David" },
        { 2, "Soldat"},
        {3,"Didier"}
    };
    
    [Export] HacksTypes _hacksType = HacksTypes.Qte;
    private bool _alreadyHacked = false;
    private MeshInstance3D _mesh;
    private ShaderMaterial _nextpass;
    private PackedScene _hackScene;
    private Node _hackNode;
    private StaticBody3D _col;
    private DialogueBox dialoguebox;
    private PackedScene dialogueboxScene = GD.Load<PackedScene>("res://scenes/UIs/dialogue.tscn");

    public override void _Ready()
    {
        _mesh = GetNode<MeshInstance3D>("pCube2_lambert2_0/pCylinder1_lambert1_0");
        _nextpass = _mesh.GetMaterialOverlay() as ShaderMaterial;
        _col = GetNode<StaticBody3D>("StaticBody3D");
        dialoguebox = dialogueboxScene.Instantiate() as DialogueBox;
        GetTree().Root.CallDeferred("add_child", dialoguebox);
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
            if (hack.Done )
            {
                _nextpass.SetShaderParameter("visible", 0);
                _hackNode.QueueFree();
                _hackNode = null;
                _col.CollisionLayer = 0;
                _col.CollisionMask = 0;
                CollisionLayer = 0;
                CollisionMask = 0;
                _alreadyHacked = true;  
                
            }
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
        dialoguebox.SpeakerName = pnjNames[0]; // ← ICI on envoie le nom
        _ = dialoguebox.ShowDialogueAsync(dialogue[0]);
        
    }

    public void NoMoreInteractable()
    {
        dialoguebox.HideDialogue();
        _nextpass.SetShaderParameter("visible", 0);
        if (_hackNode is not null)
        {
            _hackNode.QueueFree();
            _hackNode = null;
            LoadHackScene();
        }
    }
}
