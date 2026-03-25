using System;
using Godot;
using Cypherless;
using Cypherless.Weapons;
using Cypherless.Weapons.HandGuns;

[Tool]
public partial class WeaponItem : RigidBody3D
{
    [Export]
    public PackedScene WeaponScene;

    [Export]
    public bool RefreshCollision = false; // Déclenche dans l'éditeur

    public bool IsPickable = false;
    
    private CollisionShape3D _collisionShape;
    private MeshInstance3D _meshInstance;
    private Node3D _meshHolder;

    public override void _Ready()
    {
        ContactMonitor = true;
        MaxContactsReported = 4;
        if (Engine.IsEditorHint())
        {
            SetProcess(true);
            SetPhysicsProcess(false);
        }
        else {
            SetProcess(true);
            SetPhysicsProcess(true);
            GetTree().CreateTimer(2.5f).Timeout += () =>
            {
                Freeze = true;
                IsPickable = true;
            };
        }

        if (WeaponScene == null)
        {
            WeaponScene = GD.Load<PackedScene>("res://scenes/Weapons/Guns/HandGuns/J42.tscn");
        }


        if (WeaponScene != null) 
        {
            CallDeferred(nameof(RefreshInEditor));
        }
    }

    public override void _Process(double delta)
    {
        
        if (Engine.IsEditorHint() && RefreshCollision)
        {
            RefreshCollision = false;
            RefreshInEditor();
        }
        else if (!Engine.IsEditorHint())
        {
            //GD.Print(AngularVelocity);
        }
    }

    private void RefreshInEditor()
    {
        var meshHolder = GetNodeOrNull<Node3D>("MeshHolder");
        if (meshHolder == null || WeaponScene == null)
            return;

        // Nettoyage
        foreach (var child in meshHolder.GetChildren())
            child.QueueFree();
        
        GetNodeOrNull<CollisionShape3D>("CollisionShape3D")?.QueueFree();

        var weaponInstance = WeaponScene.Instantiate<Node3D>();
        meshHolder.AddChild(weaponInstance);
        weaponInstance.Owner = Engine.IsEditorHint() ? GetTree().EditedSceneRoot : null;

        // Tentative de récupération du mesh
        var mesh = weaponInstance.GetNodeOrNull<Node3D>("WeaponMeshHolder").GetChild(0).GetChild<MeshInstance3D>(0);
        if (mesh != null)
        {
            UpdateCollisionBox(weaponInstance);
            weaponInstance.QueueFree();
        }
        else
        {
            GD.PrintErr("WeaponMesh non trouvé dans l'arme instanciée.");
        }

        meshHolder.PropagateCall("RequestReady"); // Force tous les enfants à exécuter _Ready
    }

    private void UpdateCollisionBox(Node3D weaponinstance)
    {
        var col = weaponinstance.GetNodeOrNull<CollisionShape3D>("CollisionShape3D");
        var Mesh = weaponinstance.GetNodeOrNull<Node3D>("WeaponMeshHolder").GetChild(0).GetChild<MeshInstance3D>(0);
        var MeshHolder = GetNode("MeshHolder") as Node3D;
        if (col != null)
        {
            col.Reparent(this);
            col.Owner = this;
            
        }

        if (Mesh != null)
        {
            Mesh.Reparent(MeshHolder);
            Mesh.Owner = MeshHolder;
            
        }
    }

    public void ApplyForces(Vector3 impulse, Vector3 torque)
    {
        ApplyImpulse(impulse);
        ApplyTorqueImpulse(torque);
    }

    public void OnBodyEntered(Node3D body)
    {
        GD.Print("GUN ITEM HIT");
        if ((LinearVelocity.Length() > 15 && body is IDamagable b))
        {
            b.TakeDamage(5);
        }
    }
}
