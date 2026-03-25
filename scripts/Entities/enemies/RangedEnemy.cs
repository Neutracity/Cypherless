using Cypherless;
using Cypherless.Weapons;
using Godot;

[Tool]
public partial class RangedEnemy : Enemy
{
	// Paramètres spécifiques aux ennemis à distance
	[Export] public float OptimalRange = 6.0f;
	[Export] public int BurstCount = 3;
	[Export] public float BurstInterval = 0.2f;
	
	// Timer pour la rafale
	private Timer _burstTimer;
	private int _remainingBurstShots;
	private Marker3D _aimcursor;
	
	public override void _Ready()
	{
		// Initialiser les paramètres spécifiques au RangedEnemy
		_actualMesh = 8; // Soldat avec casque par défaut
		_weaponScene = GD.Load<PackedScene>("res://scenes/Weapons/Guns/HandGuns/J42.tscn");
		_weaponBoneName = "mixamorig_RightHand";
		_weaponScale = new Vector3(1, 1, 1) * 100;
		_weaponRotationOffset = new Vector3(90, 90, 0);
		_weaponPositionOffset = new Vector3(10, 0, 0);
		AttackRange = 10.0f;
		_aimcursor = GetNode<Marker3D>("CursorPosition");
		
		base._Ready();
		
		if (!Engine.IsEditorHint())
		{
			// Créer le timer pour les tirs en rafale
			_burstTimer = new Timer();
			_burstTimer.OneShot = true;
			_burstTimer.Timeout += FireNextBurstShot;
			AddChild(_burstTimer);
		}

		if (_weapon != null) _weapon.WeaponOwner = this;
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		if (_targetPlayer != null)
		{
			_aimcursor.GlobalPosition = _targetPlayer.GlobalPosition + new Vector3(0,1.4f, 0);
		}
	}
	
	// Comportement de poursuite spécifique aux ennemis à distance
	protected void Chase()
	{
		if (_targetPlayer == null) return;
		_weapon.LookAt(_aimcursor.GlobalPosition);;
		float distanceToPlayer = GlobalPosition.DistanceTo(_targetPlayer.GlobalPosition);
		
		if (distanceToPlayer < AttackRange && _attackTimer.TimeLeft == 0)
		{
			// À portée d'attaque
			_currentState = State.Attacking;
			PerformAttack();
		}
		else if (distanceToPlayer < OptimalRange * 0.7f)
		{
			// Trop proche, s'éloigner
			Vector3 directionFromPlayer = (GlobalPosition - _targetPlayer.GlobalPosition).Normalized();
			_navAgent.TargetPosition = GlobalPosition + directionFromPlayer * 5.0f;
			MoveToTarget();
		}
		else
		{
			// Continuer à poursuivre
			_navAgent.TargetPosition = _targetPlayer.GlobalPosition;
			MoveToTarget();
			
			// Si suffisamment proche pour tirer mais pas trop près
			if (distanceToPlayer <= AttackRange && distanceToPlayer >= OptimalRange * 0.7f && _attackTimer.TimeLeft == 0)
			{
				_currentState = State.Attacking;
				PerformAttack();
			}
		}
	}
	
	protected override void PerformAttack()
	{
		if (_targetPlayer == null) return;
		_weapon.LookAt(_aimcursor.GlobalPosition);;
		
		// S'assurer que l'ennemi fait face au joueur
		Vector3 directionToPlayer = (_targetPlayer.GlobalPosition - GlobalPosition).Normalized();
		if (directionToPlayer != Vector3.Zero)
		{
			var angle = Mathf.Atan2(directionToPlayer.X, directionToPlayer.Z);
			_meshHolder.Rotation = new Vector3(0, angle, 0);
		}
		Velocity = new Vector3(0, 0, 0);
		
		// Jouer l'animation de tir
		ChangeAnimation(Animations.ShootGun);
		
		// Commencer la séquence de tirs en rafale
		_remainingBurstShots = BurstCount;
		FireNextBurstShot();
	}
	
	private void FireNextBurstShot()
	{
		if (_targetPlayer == null || _currentState == State.Dead || _remainingBurstShots <= 0)
		{
			_attackTimer.Start(2.0f);
			return;
		}
		_weapon.LookAt(_aimcursor.GlobalPosition);;
		
		if (_weapon is Gun { IsUsable: true } gun)
		{
			ChangeAnimation(gun.WeaponType == WeaponTypes.Rifle ? Animations.ShootAssault : Animations.ShootGun);
			gun.UseWeapon();
		}
		Velocity = new Vector3(0, 0, 0);
		
		_remainingBurstShots--;
		
		if (_remainingBurstShots > 0)
		{
			_burstTimer.Start(BurstInterval);
		}
		else
		{
			// Fin de la rafale
			_attackTimer.Start(2.0f);
		}
	}

	
	// Spécialisation des animations pour les ennemis à distance
	protected void MoveToTarget()
	{
		base.MoveToTarget();
		
		// Animations spécifiques au RangedEnemy
		if (_currentState == State.Chasing)
		{
			ChangeAnimation(Animations.RunningGunForward);
		}
		else
		{
			ChangeAnimation(Animations.WalkingGunForward);
		}
	}
}
