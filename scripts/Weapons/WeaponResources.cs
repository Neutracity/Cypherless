/*namespace Cypherless.Weapons;
using Godot;
using System;

[GlobalClass]
public partial class WeaponResources 
{
    [Export] public String Name;
    [Export] public WeaponTypes WeaponType = WeaponTypes.Sword;
    [Export] public bool TwoHanded = false;
    
    [ExportGroup("Stats")] 
    [Export] public float UseDelay = 1f;
    [Export] public int DamagePerAmmo = 1;
    [ExportSubgroup("Gun Stats")] 
    [Export] public int Ammo = 0;
    [Export] public int MagazineSize = 0;
    [Export] public int MagazineCount = 0;
    [Export] public float ReloadDelay = 1f;
    [Export] PackedScene _bulletScene { get; set; }
    [Export] PackedScene _magazineScene { get; set; }
    public bool IsEmpty => MagazineCount == 0 && Ammo == 0;
    
    [ExportGroup("Meshs")]
    [Export] public Mesh LowRes;
    [Export] public Mesh HighRes;
    [Export] public PackedScene WeaponItem { get; set; }
    
    
    public WeaponResources() : this("",WeaponTypes.Sword,false,1f,1,0,0,0,1f,null,null,null,null,null) { }

    public WeaponResources(String name, WeaponTypes weaponTypes, Boolean twoHanded, float useDelay, int damagePerAmmo,
        int ammo, int magazineSize, int magazineCount, float reloadDelay, PackedScene bulletScene, PackedScene magazineScene, Mesh lowRes, Mesh highRes,PackedScene weaponItem)
    {
        Name = name;
        WeaponType = weaponTypes;
        TwoHanded = twoHanded;
        UseDelay = useDelay;
        Ammo = ammo;
        DamagePerAmmo = damagePerAmmo;
        MagazineSize = magazineSize;
        MagazineCount = magazineCount;
        ReloadDelay = reloadDelay;
        _bulletScene = bulletScene;
        _magazineScene = magazineScene;
        LowRes = lowRes;
        HighRes = highRes;
        WeaponItem = weaponItem;
    }
    
}*/