using System.Linq;
using Godot;
using Godot.Collections;

namespace Cypherless.Weapons;
[Tool]

public abstract partial class Gun : Weapon
{
    public abstract int Ammo { get; set; }
    public abstract int MagazineSize { get; set; }
    public abstract int MagazineCount { get; set; }
    public abstract float ReloadDelay { get; set; }
    private Array<Marker3D> _muzzlePointArr ;
    private AudioStreamPlayer3D gunSound;
    protected abstract PackedScene _bulletScene { get; set; }
    protected abstract PackedScene _magazineScene { get; set; }
    public bool IsEmpty => MagazineCount == 0 && Ammo == 0;
    public override bool IsUsable => UseTimer?.TimeLeft == 0 && !IsEmpty;

    public override void _Ready()
    {
        base._Ready();
        _muzzlePointArr = new Array<Marker3D>(GetNode<Node>("Muzzles").GetChildren().OfType<Marker3D>());
        gunSound = GetNode<AudioStreamPlayer3D>("AudioStreamPlayer3D");
    }
    
    public virtual void Reload()
    {
        if (MagazineCount > 0)
        { 
            UseTimer.Start(UseDelay + ReloadDelay);
            Ammo = MagazineSize;
            MagazineCount--;
            WeaponOwner?.GunReloadCallBack();
        }
    }
    public override void UseWeapon()
    {
        /*if (WeaponOwner is Player player)*/
        {
            if (Ammo > 0)
            {
                if (Aimcursor is null)
                {
                    Rpc(nameof(Shoot),(new Vector3(0, 0, 0)));
                }
                else
                {
                    Rpc(nameof(Shoot),Aimcursor.GlobalPosition);
                }
                /*if (player.IsGamepad) Input.StartJoyVibration(player.PlayerIndex, 0.8f, 0.8f, 0.05f);*/
            }
            else
            {
                Reload();
            }
        }
    }
    
    
    [Rpc(MultiplayerApi.RpcMode.AnyPeer,CallLocal = true)]
    protected void Shoot(Vector3 cursorPos)
    {
        {
            UseTimer.Start(UseDelay);
            gunSound.Play();
            //Instance of the Bullet.
            foreach (var muzzlePoint in _muzzlePointArr)
            {
                var bulletInstance = _bulletScene.Instantiate() as Bullet;
                GetTree().Root.AddChild(bulletInstance);
                bulletInstance.Shooter = WeaponOwner;
                bulletInstance.ApplyCentralImpulse(-muzzlePoint.GlobalTransform.Basis.Z.Normalized() * 30);
                bulletInstance.Position = muzzlePoint.GlobalPosition;
           
                // Bullet Rotation 
                var basistempo = bulletInstance.Transform;
                basistempo.Basis = muzzlePoint.GlobalTransform.Basis;
                bulletInstance.Transform = basistempo;
           
                // Handle Particle 
                var particle = muzzlePoint.GetNode<CpuParticles3D>("ShootParticles");
                particle.Emitting = true;
     
            }
            
            // Recoil Animation
            //GetNode<AnimationPlayer>("AnimationPlayer").Play("Shoot");
            //EmitSignal(SignalName.HaveShoot);

            Ammo--;
        }
    }
}