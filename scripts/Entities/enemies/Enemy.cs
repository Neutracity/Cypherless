using Cypherless;
using Cypherless.Weapons;
using Godot;
using System.Collections.Generic;

[Tool]
public abstract partial class Enemy : Character, IDamagable
{
	#region Common Enemy States
	
	public enum State
	{
		Idle,
		Patrolling,
		Chasing,
		Attacking,
		Fleeing,
		Dead
	}
	
	// État actuel de l'ennemi
	protected State _currentState = State.Idle;
	
	// Dictionnaire pour debug/éditeur qui convertit les états en texte
	protected static readonly Dictionary<State, string> StateNames = new Dictionary<State, string>
	{
		{ State.Idle, "Idle" },
		{ State.Patrolling, "Patrolling" },
		{ State.Chasing, "Chasing" },
		{ State.Attacking, "Attacking" },
		{ State.Fleeing, "Fleeing" },
		{ State.Dead, "Dead" }
	};
	
	// Implémentation explicite pour éviter conflits avec Character
	int IDamagable.MaxHealth { get => MaxHealth; set => MaxHealth = value; }
	int IDamagable.Health { get => Health; set => Health = value; }
	
	#endregion
	
	#region Navigation and Detection
	
	// Navigation
	protected NavigationAgent3D _navAgent;
	protected bool _isNavSynchronized;
	
	// Paramètres de comportement
	[Export] public float DetectionRange = 10.0f;
	[Export] public float AttackRange = 2.0f;
	[ExportGroup("Patrol Settings")]
	[Export] public float PatrolRadius = 10.0f;
	[Export] public bool RandomPatrol = true;
	[Export] public float MinPatrolWaitTime = 1.0f;
	[Export] public float MaxPatrolWaitTime = 5.0f;
	
	// Points de patrouille (si RandomPatrol = false)
	[Export] public Node3D[] PatrolPoints;
	
	// Détection
	protected Area3D _detectionArea;
	protected Player _targetPlayer;
	
	// Timers
	protected Timer _attackTimer;
	protected Timer _patrolWaitTimer;
	
	// Animations 
	
	
	#endregion
	
	#region Dictionnaire de maillages et d'animations commun à tous les ennemis
	
	// Dictionnaire commun pour tous les types d'ennemis
	protected override Dictionary<int, string> _meshs { get; set; } = new()
	{
		{1, "res://3d-assets/Entities/Enemies/Ennemis_armure_rockets_dorsale.glb"},
		{2, "res://3d-assets/Entities/Enemies/Swatguy.glb"},
		{3, "res://3d-assets/Entities/Enemies/Soldat_militaire.glb"},
		{4, "res://3d-assets/Entities/Enemies/Le_mechant_ultime.glb"},
		{5, "res://3d-assets/Entities/Enemies/Soldat_militaire.glb"},
		{6, "res://3d-assets/Entities/Enemies/Soldat_full_noir.glb"},
		{7, "res://3d-assets/Entities/Enemies/Soldat_militaire.glb"},
		{8, "res://3d-assets/Entities/Enemies/Soldat_militaire_casque.glb"},
		{9, "res://3d-assets/Entities/Enemies/SwatLittleGuy.glb"}
	};
	
	#endregion

	
	#region AI Behavior Parameters
	
	[ExportGroup("AI Behavior")]
	[Export] public float AggressionLevel = 0.5f; // 0.0 to 1.0, affects how likely to attack vs flee
	[Export] public float AttackCooldown = 1.5f;
	[Export] public float ChaseSpeed = 3.0f;
	[Export] public float PatrolSpeed = 1.5f;
	[Export] public bool FleeWhenLowHealth = false;
	[Export] public float LowHealthThreshold = 0.3f; // Percentage of max health
	
	#endregion
	
	#region Base Methods
	
	// Dans la classe Enemy.cs, ajoutez cet attribut dans la section #region Navigation and Detection
	[ExportGroup("Detection Settings")]
	[Export] public float RangeDetectionFactor = 1.0f; // Valeur par défaut = 1.0 (100%)
	
	// Puis, modifiez la méthode InitializeCharacter() pour appliquer ce facteur à la zone de détection
	protected override void InitializeCharacter()
	{
		// Chercher ou créer l'agent de navigation
		_navAgent = GetNodeOrNull<NavigationAgent3D>("NavigationAgent3D");
		if (_navAgent == null)
		{
			_navAgent = new NavigationAgent3D();
			_navAgent.Name = "NavigationAgent3D";
			AddChild(_navAgent);
			
			if (Engine.IsEditorHint() && GetTree()?.EditedSceneRoot != null)
				_navAgent.Owner = GetTree().EditedSceneRoot;
		}
		
		// Chercher ou créer l'area de détection
		_detectionArea = GetNodeOrNull<Area3D>("Area3D");
		
		// Appliquer le facteur de détection à la zone de détection
		if (_detectionArea != null)
		{
			// Trouver tous les CollisionShape3D enfants et appliquer le facteur d'échelle
			foreach (var child in _detectionArea.GetChildren())
			{
				if (child is CollisionShape3D collisionShape)
				{
					// Obtenir la forme actuelle et ses dimensions
					var shape = collisionShape.Shape;
					if (shape is SphereShape3D sphereShape)
					{
						// Modifier le rayon pour les sphères
						sphereShape.Radius *= RangeDetectionFactor;
					}
					else if (shape is BoxShape3D boxShape)
					{
						// Modifier la taille pour les boîtes
						boxShape.Size *= RangeDetectionFactor;
					}
					else if (shape is CylinderShape3D cylinderShape)
					{
						// Modifier les dimensions pour les cylindres
						cylinderShape.Radius *= RangeDetectionFactor;
					}
					// Ajoutez d'autres types de formes si nécessaire
				}
			}
		}
		
		// Connecter les signaux
		_detectionArea.BodyEntered += OnBodyEntered;
		_detectionArea.BodyExited += OnBodyExited;
		
		// Créer le timer d'attaque
		_attackTimer = GetNodeOrNull<Timer>("AttackTimer");
		if (_attackTimer == null) 
		{
			_attackTimer = new Timer();
			_attackTimer.Name = "AttackTimer";
			_attackTimer.OneShot = true;
			_attackTimer.WaitTime = AttackCooldown;
			AddChild(_attackTimer);
			
			if (Engine.IsEditorHint() && GetTree()?.EditedSceneRoot != null)
				_attackTimer.Owner = GetTree().EditedSceneRoot;
		}
		_attackTimer.Timeout += OnAttackCooldownFinished;
		
		// Créer le timer de patrouille
		_patrolWaitTimer = new Timer();
		_patrolWaitTimer.Name = "PatrolWaitTimer";
		_patrolWaitTimer.OneShot = true;
		AddChild(_patrolWaitTimer);
		if (Engine.IsEditorHint() && GetTree()?.EditedSceneRoot != null)
			_patrolWaitTimer.Owner = GetTree().EditedSceneRoot;
		_patrolWaitTimer.Timeout += OnPatrolWaitComplete;
		
		// Initialiser la navigation
		CallDeferred(nameof(EnableNavigation));
		
		// Initialiser l'état initial
		_currentState = State.Idle;
	}
	
	protected override void ProcessCharacter(double delta)
	{
		if (!_isNavSynchronized) return;
		
		// Mettre à jour le comportement en fonction de l'état
		switch (_currentState)
		{
			case State.Idle:
				ProcessIdle(delta);
				break;
			case State.Patrolling:
				ProcessPatrol(delta);
				break;
			case State.Chasing:
				ProcessChase(delta);
				break;
			case State.Attacking:
				ProcessAttack(delta);
				break;
			case State.Fleeing:
				ProcessFlee(delta);
				break;
			case State.Dead:
				// Rien à faire quand mort
				break;
		}
		
		// Mettre à jour l'animator en fonction de l'état si nécessaire
		UpdateAnimation(delta);
	}
	
	protected override void PhysicsProcessCharacter(double delta)
	{
		// La gravité est gérée par la classe de base
		// Logique de mouvement spécifique aux ennemis
		switch (_currentState)
		{
			case State.Idle:
				// Pas de mouvement en idle
				Velocity = new Vector3(0, Velocity.Y, 0);
				break;
			
			case State.Dead:
				// Pas de mouvement quand mort
				Velocity = new Vector3(0, Velocity.Y, 0);
				break;
				
			// Pour les autres états, le mouvement est géré dans leurs méthodes respectives
		}
	}
	
	#endregion
	
	#region Navigation and AI Methods
	
	// Activer la navigation (appelé en différé pour laisser le temps à la scène de se charger)
	protected void EnableNavigation()
	{
		_isNavSynchronized = true;
		
		// Démarrer la patrouille si c'est l'état initial
		if (_currentState == State.Patrolling)
		{
			StartPatrol();
		}
	}
	
	

	// Obtenir le prochain point de patrouille
	protected Vector3 GetNextPatrolPoint()
	{
		if (RandomPatrol || PatrolPoints == null || PatrolPoints.Length == 0)
		{
			return GetRandomPatrolPosition();
		}
		else
		{
			// Trouver le point de patrouille le plus proche qui n'est pas la position actuelle
			float minDist = float.MaxValue;
			Vector3 bestPoint = GlobalPosition;
			
			foreach (var point in PatrolPoints)
			{
				if (point == null) continue;
				
				float dist = GlobalPosition.DistanceTo(point.GlobalPosition);
				if (dist < minDist && dist > 1.0f) // Ne pas retourner à la position actuelle
				{
					minDist = dist;
					bestPoint = point.GlobalPosition;
				}
			}
			
			// Si aucun point valide trouvé, générer une position aléatoire
			if (bestPoint == GlobalPosition)
			{
				return GetRandomPatrolPosition();
			}
			
			return bestPoint;
		}
	}
	
	// Commencer la patrouille
	protected virtual void StartPatrol()
	{
		if (_currentState == State.Dead) return;
		
		_currentState = State.Patrolling;
		
		// Définir le prochain point de patrouille
		_navAgent.TargetPosition = GetNextPatrolPoint();
		
		// Ajuster la vitesse
		Speed = PatrolSpeed;
	}
	
	// Se déplacer vers la cible actuelle (utilisé par plusieurs états)
	protected void MoveToTarget(float speedMultiplier = 1.0f)
	{
		if (!_navAgent.IsNavigationFinished())
		{
			var currentPos = GlobalPosition;
			var targetPos = _navAgent.GetNextPathPosition();
			
			// Contraindre la position cible à Z=0 (ou près de 0)
			targetPos.Z = Mathf.Lerp(targetPos.Z, 0, 0.2f);
			
			var direction = (targetPos - currentPos).Normalized();
			
			// Vérifier si la direction sort du navmesh
			if (_navAgent.IsTargetReachable() && _navAgent.IsNavigationFinished() == false)
			{
				// Mettre à jour la vélocité en préservant Y et contraignant Z
				Velocity = new Vector3(
					direction.X * Speed * speedMultiplier,
					Velocity.Y,
					direction.Z * Speed * speedMultiplier
				);
				
				// Appliquer une force progressive pour ramener l'ennemi vers Z=0
				Velocity = new Vector3(
					Velocity.X,
					Velocity.Y,
					Mathf.Lerp(Velocity.Z, 0, 0.1f)
				);
				
				// Rotation du mesh pour faire face à la direction
				if (direction != Vector3.Zero)
				{
					var angle = Mathf.Atan2(direction.X, direction.Z);
					_meshHolder.Rotation = new Vector3(0, angle, 0);
				}
			}
			else
			{
				// Si on sort du navmesh, ralentir et chercher un point valide
				Velocity = Velocity * 0.5f;
				_navAgent.TargetPosition = GetValidNavMeshPosition(currentPos);
			}
		}
		else
		{
			// Si on a atteint la destination, s'arrêter
			Velocity = new Vector3(0, Velocity.Y, 0);
		}
	}
	
	// Fin de la pause de patrouille
	protected virtual void OnPatrolWaitComplete()
	{
		if (_currentState == State.Dead) return;
		_navAgent.TargetPosition = GetNextPatrolPoint();
		GD.Print("FinishedPatrol");
		_currentState = State.Patrolling;
	}
	
	// Détecter un objet entrant dans la zone
	protected virtual void OnBodyEntered(Node body)
	{
		if (body is Player player && _currentState != State.Dead)
		{
			_targetPlayer = player;
			
			// Vérifier s'il faut fuir en fonction de la santé
			if (FleeWhenLowHealth && ((float)Health / MaxHealth) < LowHealthThreshold)
			{
				_currentState = State.Fleeing;
			}
			else
			{
				_currentState = State.Chasing;
				_patrolWaitTimer.Stop();
			}
		}
	}
	
	// Détecter un objet sortant de la zone
	protected virtual void OnBodyExited(Node body)
	{
		if (body == _targetPlayer && _currentState != State.Dead)
		{
			_targetPlayer = null;
			
			// Retourner à la patrouille
			_currentState = State.Patrolling;
			StartPatrol();
		}
	}
	
	// Fin du cooldown d'attaque
	protected virtual void OnAttackCooldownFinished()
	{
		if (_currentState == State.Dead) return;
		
		// Si la cible n'est plus là, retourner à la patrouille
		if (_targetPlayer == null)
		{
			_currentState = State.Patrolling;
			return;
		}
		
		// Vérifier s'il faut fuir en fonction de la santé
		if (FleeWhenLowHealth && ((float)Health / MaxHealth) < LowHealthThreshold)
		{
			_currentState = State.Fleeing;
			return;
		}
		
		// Sinon, revenir à la poursuite
		_currentState = State.Chasing;
		
		// Si toujours à portée d'attaque, attaquer à nouveau
		float distanceToPlayer = GlobalPosition.DistanceTo(_targetPlayer.GlobalPosition);
		if (distanceToPlayer <= AttackRange)
		{
			_currentState = State.Attacking;
			PerformAttack();
		}
	}
	
	#endregion
	
	#region State Processing Methods
	
	// Comportement en état Idle
	protected virtual void ProcessIdle(double delta)
	{
		// Animation par défaut
		ChangeAnimation(Animations.Idle);
		// Après un temps, revenir à la patrouille
		if (_patrolWaitTimer.IsStopped())
		{
			_patrolWaitTimer.Start((float)GD.RandRange(MinPatrolWaitTime, MaxPatrolWaitTime));
		}
	}
	
	// Comportement en état Patrol
	protected virtual void ProcessPatrol(double delta)
	{
		
		if (_navAgent.IsNavigationFinished())
		{
			_currentState = State.Idle;
			return;
		}
		
		// Se déplacer vers la cible
		MoveToTarget();
		
		// Animation de marche
		ChangeAnimation(Animations.WalkingForward);
	}
	
	// Comportement en état Chase
	protected virtual void ProcessChase(double delta)
	{
		if (_targetPlayer == null)
		{
			// Cible perdue, revenir à la patrouille
			_currentState = State.Patrolling;
			StartPatrol();
			return;
		}
		
		// Mettre à jour la destination
		_navAgent.TargetPosition = _targetPlayer.GlobalPosition;
		
		// Vérifier la distance par rapport à la cible
		float distanceToPlayer = GlobalPosition.DistanceTo(_targetPlayer.GlobalPosition);
		
		if (distanceToPlayer <= AttackRange)
		{
			// À portée d'attaque
			if (_attackTimer.TimeLeft == 0)
			{
				_currentState = State.Attacking;
				PerformAttack();
			}
		}
		else
		{
			// Poursuivre la cible
			Speed = ChaseSpeed;
			MoveToTarget(1.0f);
			
			// Animation de course
			ChangeAnimation(Animations.RunningForward);
		}
	}
	
	// Comportement en état Attack
	protected virtual void ProcessAttack(double delta)
	{
		// L'attaque est gérée par les événements de timer
		// Mais on peut ajouter des comportements supplémentaires ici
		
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
	
	// Comportement en état Flee
	protected virtual void ProcessFlee(double delta)
	{
		if (_targetPlayer == null)
		{
			// Cible perdue, revenir à la patrouille
			_currentState = State.Patrolling;
			StartPatrol();
			return;
		}
		
		// Calculer une direction à l'opposé du joueur
		Vector3 fleeDirection = (GlobalPosition - _targetPlayer.GlobalPosition).Normalized();
		
		// Position de fuite à une certaine distance
		Vector3 fleePosition = GlobalPosition + fleeDirection * 10.0f;
		
		// Mettre à jour la destination
		_navAgent.TargetPosition = fleePosition;
		
		// Se déplacer rapidement
		Speed = ChaseSpeed * 1.2f;
		MoveToTarget(1.0f);
		
		// Animation de course
		ChangeAnimation(Animations.RunningBack); // Utiliser l'animation de course arrière si disponible
		
		// Vérifier si l'ennemi s'est suffisamment éloigné
		float distanceToPlayer = GlobalPosition.DistanceTo(_targetPlayer.GlobalPosition);
		if (distanceToPlayer > DetectionRange * 1.5f)
		{
			// Assez loin, revenir à la patrouille
			_currentState = State.Patrolling;
			StartPatrol();
		}
	}
	
	#endregion
	
	#region Combat Methods
	
	// Méthode abstraite que les sous-classes doivent implémenter
	protected abstract void PerformAttack();
	
	// Redéfinir la méthode TakeDamage pour ajouter des comportements spécifiques aux ennemis
	public override void TakeDamage(int amount)
	{
		if (_currentState == State.Dead) return;
		
		base.TakeDamage(amount);
		
		GD.Print($"{GetType().Name} takes {amount} damage. Remaining HP: {Health}/{MaxHealth}");
		
		// Si l'ennemi a peu de vie et que la fuite est activée
		if (FleeWhenLowHealth && ((float)Health / MaxHealth) < LowHealthThreshold && _currentState != State.Fleeing)
		{
			_currentState = State.Fleeing;
		}
		
		// Si l'ennemi n'est pas déjà en train de poursuivre et qu'il voit le joueur, commencer à le poursuivre
		if (_currentState == State.Patrolling || _currentState == State.Idle)
		{
			// Chercher le joueur le plus proche
			Player nearestPlayer = null;
			float minDistance = float.MaxValue;
			
			foreach (var body in _detectionArea.GetOverlappingBodies())
			{
				if (body is Player player)
				{
					float distance = GlobalPosition.DistanceTo(player.GlobalPosition);
					if (distance < minDistance)
					{
						minDistance = distance;
						nearestPlayer = player;
					}
				}
			}
			
			if (nearestPlayer != null)
			{
				_targetPlayer = nearestPlayer;
				
				if (FleeWhenLowHealth && ((float)Health / MaxHealth) < LowHealthThreshold)
				{
					_currentState = State.Fleeing;
				}
				else
				{
					_currentState = State.Chasing;
				}
			}
		}
	}
	
	// Redéfinir la méthode Die pour ajouter des comportements spécifiques
	protected override void Die()
	{
		_currentState = State.Dead;
		
		// Appeler la méthode de la classe de base
		base.Die();
		
		// Désactiver les collisions
		CollisionLayer = 0;
		CollisionMask = 1;
		
		// Désactiver la détection
		if (_detectionArea != null)
		{
			_detectionArea.Monitoring = false;
			_detectionArea.Monitorable = false;
		}
		
		// Ajouter un timer pour supprimer l'ennemi après un certain délai
		Timer deathTimer = new Timer();
		deathTimer.WaitTime = 10.0f; // 10 secondes pour laisser l'animation de mort se terminer
		deathTimer.OneShot = true;
		deathTimer.Timeout += () => QueueFree();
		AddChild(deathTimer);
		deathTimer.Start();
	}
	
	#endregion
	
	#region Animation Management
	
	// Mise à jour des animations en fonction de l'état
	protected virtual void UpdateAnimation(double delta)
	{
		// Cette méthode peut être surchargée par les sous-classes pour des animations spécifiques
	}
	
	#endregion
	
	// Ajouter cette méthode pour trouver une position valide sur le navmesh
	protected Vector3 GetValidNavMeshPosition(Vector3 nearPosition)
	{
		// Essayer de trouver une position valide sur le navmesh près de la position actuelle
		// Commencer par tenter de revenir à la dernière position connue sur le navmesh
		
		// Première tentative: position actuelle mais avec Z=0
		Vector3 attempt = new Vector3(nearPosition.X, nearPosition.Y, 0);
		_navAgent.TargetPosition = attempt;
		
		if (_navAgent.IsTargetReachable())
			return attempt;
		
		// Seconde tentative: reculer un peu
		attempt = new Vector3(nearPosition.X - Mathf.Sign(Velocity.X) * 2.0f, nearPosition.Y, 0);
		_navAgent.TargetPosition = attempt;
		
		if (_navAgent.IsTargetReachable())
			return attempt;
		
		// Dernière tentative: position de patrouille aléatoire
		return GetRandomPatrolPosition();
	}
	
	// Modifier cette méthode pour s'assurer que les positions de patrouille restent sur Z=0
	protected Vector3 GetRandomPatrolPosition()
	{
		// Générer un offset aléatoire dans un cercle horizontal
		float angle = (float)GD.RandRange(0, Mathf.Pi * 2);
		float distance = (float)GD.RandRange(0, PatrolRadius);
		
		// Position actuelle
		var currentPos = GlobalPosition;
		
		// Calculer une nouvelle position sur le même plan Z=0
		Vector3 newPos = currentPos + new Vector3(
			Mathf.Cos(angle) * distance,
			0, // Garder le même Y
			0  // Forcer Z=0
		);
		
		// Vérifier si la position est valide sur le navmesh
		_navAgent.TargetPosition = newPos;
		if (!_navAgent.IsTargetReachable())
		{
			// Si non valide, essayer une autre direction
			for (int i = 0; i < 8; i++)
			{
				angle = i * Mathf.Pi / 4; // Tester 8 directions différentes
				newPos = currentPos + new Vector3(
					Mathf.Cos(angle) * (distance * 0.5f), // Réduire la distance
					0,
					0
				);
				
				_navAgent.TargetPosition = newPos;
				if (_navAgent.IsTargetReachable())
					break;
			}
		}
		
		return newPos;
	}
	
	// Ajoutez également une méthode pour mettre à jour la taille de détection en temps réel si nécessaire
	public void UpdateDetectionRange(float newFactor)
	{
		RangeDetectionFactor = newFactor;
		
		if (_detectionArea != null)
		{
			foreach (var child in _detectionArea.GetChildren())
			{
				if (child is CollisionShape3D collisionShape)
				{
					var shape = collisionShape.Shape;
					if (shape is SphereShape3D sphereShape)
					{
						// Réinitialiser puis appliquer le nouveau facteur
						sphereShape.Radius = (sphereShape.Radius / RangeDetectionFactor) * newFactor;
					}
					else if (shape is BoxShape3D boxShape)
					{
						boxShape.Size = (boxShape.Size / RangeDetectionFactor) * newFactor;
					}
					else if (shape is CylinderShape3D cylinderShape)
					{
						cylinderShape.Radius = (cylinderShape.Radius / RangeDetectionFactor) * newFactor;
						cylinderShape.Height = (cylinderShape.Height / RangeDetectionFactor) * newFactor;
					}
				}
			}
		}
	}
}
