using Godot;

namespace Cypherless.Weapons.HandGuns;
[Tool]
public partial class J42 : Gun
{
    public override WeaponTypes WeaponType { get; set; } = WeaponTypes.HandGun;
    protected override float UseDelay { get; set; } = 0.1f;
    public override int Ammo { get; set; } = 20;
    public override int Damage { get; set; } = 1;
    public override int MagazineSize { get; set; } = 20;
    public override int MagazineCount { get; set; } = 0;
    public override float ReloadDelay { get; set; } = 0.5f;
    protected override PackedScene _bulletScene { get; set; } = ResourceLoader.Load<PackedScene>("res://scenes/Weapons/Guns/Bullets/bullet.tscn");
    protected override PackedScene _magazineScene { get; set; }
    public override Character WeaponOwner { get; set; }

    public override void _Ready()
    {
        base._Ready();
    }
}