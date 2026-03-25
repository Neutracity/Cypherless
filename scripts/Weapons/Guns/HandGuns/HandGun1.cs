using Godot;

namespace Cypherless.Weapons.HandGuns;
[Tool]
public partial class HandGun1 : Gun
{
    public override WeaponTypes WeaponType { get; set; } = WeaponTypes.HandGun;
    [Export] protected override float UseDelay { get; set; } = 0.1f;
    [Export] public override int Damage { get; set; } = 1;
    [Export] public override int Ammo { get; set; } = 20;
    [Export] public override int MagazineSize { get; set; } = 20;
    [Export] public override int MagazineCount { get; set; } = 1;
    [Export] public override float ReloadDelay { get; set; } = 1.0f;
    [Export] protected override PackedScene _bulletScene { get; set; } = ResourceLoader.Load<PackedScene>("res://scenes/Weapons/Guns/Bullets/bullet.tscn");
    [Export] protected override PackedScene _magazineScene { get; set; }
    public override Character WeaponOwner { get; set; }

    public override void _Ready()
    {
        base._Ready();
    }
}