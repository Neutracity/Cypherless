using Cypherless;
using Cypherless.Weapons;
using Godot;
using Godot.Collections;

[Tool]
public partial class MeleeEnemy : Enemy
{
	// Paramètres spécifiques aux ennemis au corps à corps
	[ExportGroup("Melee Parameters")]
	[Export] public float AttackDamage = 1.0f;
	[Export] public float LungeForce = 5.0f;
	[Export] public bool UseLungeAttack = true;
	
	// Timer pour appliquer les dégâts (avec délai)
	private Timer _damageTimer;
	
	protected override void InitializeMeshes()
	{
		
		// Paramètres par défaut pour les ennemis au corps à corps
		_actualMesh = 3; // Swatguy par défaut
		_weaponScene = GD.Load<PackedScene>("res://scenes/Weapons/Swords/ExoKatana.tscn");
		_weaponBoneName = "mixamorig_RightHand";
		_weaponScale = new Vector3(1, 1, 1) * 100;
		_weaponRotationOffset = new Vector3(0, 180, 60);
		_weaponPositionOffset = new Vector3(0.060f, 0, -0.019f);
		
		// Paramètres de base pour les ennemis au corps à corps
		MaxHealth = 3;
		Health = 3;
		Speed = 2.0f;
		ChaseSpeed = 3.0f;
		PatrolSpeed = 1.5f;
		AttackCooldown = 1.5f;
		AttackRange = 1.5f;
	}
	
	protected override void InitializeCharacter()
	{
		// Appeler la méthode de la classe de base
		base.InitializeCharacter();
		
		// Créer le timer pour les dégâts différés
		_damageTimer = new Timer();
		_damageTimer.Name = "DamageTimer";
		_damageTimer.OneShot = true;
		_damageTimer.WaitTime = 0.8f; // Délai de 0.5 secondes pour appliquer les dégâts
		AddChild(_damageTimer);
		if (Engine.IsEditorHint() && GetTree()?.EditedSceneRoot != null)
			_damageTimer.Owner = GetTree().EditedSceneRoot;
		_damageTimer.Timeout += ApplyDamageToPlayer;
	}
	
	// Méthode pour effectuer une attaque au corps à corps
	protected override void PerformAttack()
	{
		if (_targetPlayer == null) return;
		
		// Arrêter complètement le mouvement pendant l'attaque
		Velocity = new Vector3(0, 0, 0);
		
		// Tourner vers le joueur
		Vector3 directionToPlayer = (_targetPlayer.GlobalPosition - GlobalPosition).Normalized();
		if (directionToPlayer != Vector3.Zero)
		{
			var angle = Mathf.Atan2(directionToPlayer.X, directionToPlayer.Z);
			_meshHolder.Rotation = new Vector3(0, angle, 0);
		}
		
		// Supprimer le bond d'attaque et le remplacer par une animation statique
		if (UseLungeAttack)
		{
			// Au lieu d'appliquer une force, simplement jouer une animation plus dynamique
			// mais sans mouvement physique
		}
		
		// Jouer l'animation d'attaque
		ChangeAnimation(Animations.InwardSlashSword);
		
		// Utiliser l'arme si disponible
		if (_weapon != null && _weapon.IsUsable)
		{
			_weapon.UseWeapon();
		}
		
		// Démarrer le timer pour appliquer les dégâts après un délai
		_damageTimer.Start();
		
		// Démarrer le timer de cooldown
		_attackTimer.Start(AttackCooldown);
	}
	
	// Méthode pour appliquer les dégâts au joueur après le délai
	private void ApplyDamageToPlayer()
	{
		if (_targetPlayer == null || _currentState == State.Dead) return;
		
		// Vérifier si le joueur est toujours à portée d'attaque (avec une légère marge)
		float distanceToPlayer = GlobalPosition.DistanceTo(_targetPlayer.GlobalPosition);
		if (distanceToPlayer <= AttackRange * 1.2f)
		{
			// Calculer les dégâts (utiliser les dégâts de l'arme si disponible)
			int damage = _weapon != null ? _weapon.Damage : (int)AttackDamage;
			
			// Appliquer les dégâts
			_targetPlayer.TakeDamage(damage);
			GD.Print($"Melee enemy deals {damage} damage to player");
		}
	}
	
	// Redéfinir la méthode de mouvement pour les comportements spécifiques
	protected override void ProcessChase(double delta)
	{
		base.ProcessChase(delta);
		
		// Ajouter des comportements spécifiques aux ennemis au corps à corps pendant la poursuite
		if (_currentState == State.Chasing && _targetPlayer != null)
		{
			// Par exemple, utiliser des animations plus agressives à mesure qu'on s'approche
			float distanceToPlayer = GlobalPosition.DistanceTo(_targetPlayer.GlobalPosition);
			if (distanceToPlayer <= AttackRange * 2.0f)
			{
				// Animation plus agressive quand proche de la cible
				//ChangeAnimation(Animations.RunningSwordForward);
			}
		}
	}
	
// Modifions également la méthode ProcessAttack dans Enemy.cs ou surchargeons-la ici
	protected override void ProcessAttack(double delta)
	{
		// Bloquer tout mouvement pendant l'attaque pour les ennemis corps-à-corps
		Velocity = new Vector3(0, 0, 0);
		
		// S'assurer que l'ennemi fait face au joueur
		if (_targetPlayer != null)
		{
			Vector3 directionToPlayer = (_targetPlayer.GlobalPosition - GlobalPosition).Normalized();
			if (directionToPlayer != Vector3.Zero)
			{
				float angle = Mathf.Atan2(directionToPlayer.X, directionToPlayer.Z);
				_meshHolder.Rotation = new Vector3(0, angle, 0);
			}
		}
	}
}
