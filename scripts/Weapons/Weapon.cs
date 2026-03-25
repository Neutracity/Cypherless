using Cypherless.Weapons;
using Godot;

namespace Cypherless;
[Tool]
public abstract partial class Weapon : Node3D
{
    public abstract WeaponTypes WeaponType { get; set; }
    protected abstract float UseDelay { get; set; }
    public abstract int Damage { get; set; }
    protected Timer UseTimer;
    public Marker3D Aimcursor;
    public PackedScene WeaponItem = GD.Load<PackedScene>("res://scenes/Props/WeaponItem.tscn");
    public virtual bool IsUsable => UseTimer?.TimeLeft == 0;
    public abstract Character WeaponOwner { get; set; }

    /*[Export]
    public Resource WeaponResources
    {
        get => _weaponResources; 
        set
        {
            _weaponResources = value;
            InitWeapon();
        }
    }

    private Resource _weaponResources;*/
    public abstract void UseWeapon();
    
    public override void _Ready()
    {
        if (GetParent().GetParent() is WeaponItem)
            return;
        UseTimer = GetNode<Timer>("UseCooldown");
        UseTimer.Start(UseDelay);
        /*InitWeapon();*/
    }

    /*public void InitWeapon()
    {
        if (_weaponResources is WeaponResources weaponResources)
        {
            _weaponMesh.Mesh = weaponResources.HighRes;
        }
    }*/

    public override void _Process(double delta)
    {
        if (GetParent().GetParent() is WeaponItem)
            return;
        if(Aimcursor != null)
            LookAt(Aimcursor.GlobalPosition);
    }
}