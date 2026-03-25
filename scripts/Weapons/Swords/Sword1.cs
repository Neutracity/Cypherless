using Cypherless.Weapons;
using Godot;

namespace Cypherless.Weapons;
[Tool]

public partial class Sword1 : Weapon
{
    public override WeaponTypes WeaponType { get; set; } = WeaponTypes.Sword;
    [Export] protected override float UseDelay { get; set; } = 1f;
    [Export] public override int Damage { get; set; } = 1;
    public override Character WeaponOwner { get; set; }

    public override void UseWeapon()
    {
    }
    
    public override void _Ready()
    {
        if (GetParent().GetParent() is WeaponItem)
            return;
        UseTimer = GetNode<Timer>("UseCooldown");
        UseTimer.Start(UseDelay);
        /*InitWeapon();*/
    }

    public override void _Process(double delta)
    {
    }
}