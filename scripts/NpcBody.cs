using Godot;
using Cypherless;
using Godot.Collections;
[Tool]
public partial class NpcBody : Character, IInteractable
{
	#region Movement Properties
	
	[Export] public bool CanMove = true;
	[Export] public float Speed = 2.0f;
	[Export] public float MoveDuration = 3.0f;
	[Export] public float WaitDuration = 2.0f;
	[Export] public bool InvertRotation = true; // Nouvelle propriété pour inverser la rotation

	// Liste des points de déplacement, similaire aux ennemis
	[Export] public Array<Marker3D> MovementPoints = new Array<Marker3D>();
	[Export] public Marker3D StopMarker; // Marker où le NPC s'arrêtera définitivement
	private int _currentPointIndex = 0;
	private bool _hasReachedFinalDestination = false; // Indique si le NPC a atteint sa destination finale
	
	private enum MovementState
	{
		Moving,
		Waiting,
		Stopped // Nouvel état pour indiquer que le NPC s'est arrêté définitivement
	}
	
	private MovementState _currentState = MovementState.Moving;
	private Vector3 _moveDirection;
	private Timer _stateTimer;
	private float _arrivalThreshold = 0.5f; // Distance à laquelle on considère que le NPC est arrivé au point

	
	protected override System.Collections.Generic.Dictionary<int, string> _meshs { get; set; } = new System.Collections.Generic.Dictionary<int, string>
	{
		{ 1, "res://3d-assets/Entities/Pnjs/Pnj11.glb" },
		{2, "res://3d-assets/Entities/Pnjs/Pnj2.glb"},
		{3, "res://3d-assets/Entities/Pnjs/Pnj3.glb"},
		{4, "res://3d-assets/Entities/Pnjs/Pnj4.glb"},
		{5, "res://3d-assets/Entities/Pnjs/Pnj5.glb"},
		{6, "res://3d-assets/Entities/Pnjs/Pnj6.glb"},
		{7, "res://3d-assets/Entities/Pnjs/Pnj7.glb"},
		{8, "res://3d-assets/Entities/Pnjs/Pnj8.glb"},
		{9, "res://3d-assets/Entities/Pnjs/Pnj9.glb"},
		{10, "res://3d-assets/Entities/Pnjs/Pnj10.glb"},
		{11, "res://3d-assets/Entities/Enemies/Soldat_full_noir.glb"},
		{12, "res://3d-assets/Entities/Enemies/Soldat_militaire.glb"},
		{13, "res://3d-assets/Entities/Enemies/Soldat_militaire_casque.glb"},
		{14, "res://3d-assets/Entities/Enemies/SwatLittleGuy.glb"},
		{15, "res://3d-assets/Entities/Enemies/Swatguy.glb"},
		
		// Autres variantes de mesh du joueur si nécessaire
	};
	
	#endregion
	
	#region Interaction Properties
	
	// Propriétés pour la fonctionnalité d'interaction
	[Export] public bool IsInteractable = false;
	[Export] public string DialogText = "Bonjour !";
	[Export] public bool IsQuestGiver = false;
	[Export] public string QuestId = "";
	
	private Label3D _interactionLabel;
	private bool _isInteractable = false;
	
	
	#endregion
	
	#region Character Overrides
	
	public override void _Ready()
	{
		base._Ready();
		Health = 100000;
		MaxHealth = 100000;
		
		// Configuration du timer pour les états de mouvement
		_stateTimer = new Timer();
		_stateTimer.OneShot = true;
		AddChild(_stateTimer);
		_stateTimer.Timeout += OnStateChange;
		
		// Configuration de l'étiquette d'interaction
		_interactionLabel = new Label3D();
		_interactionLabel.Text = "Appuyez sur E pour interagir";
		_interactionLabel.Position = new Vector3(0, 2.0f, 0);
		_interactionLabel.Billboard = BaseMaterial3D.BillboardModeEnum.FixedY;
		_interactionLabel.Visible = false;
		AddChild(_interactionLabel);
		
		// Si le NPC peut se déplacer et que des points de mouvement sont définis, commencer à bouger
		if (CanMove && MovementPoints.Count > 0)
		{
			StartMoving();
		}
		else
		{
			// Animation par défaut si le NPC ne bouge pas
			ChangeAnimation(Animations.Idle);
		}
	}
	
	protected override void ProcessCharacter(double delta)
	{
		base.ProcessCharacter(delta);
		
		// Logique spécifique au NPC ici
	}
	
	protected override void PhysicsProcessCharacter(double delta)
	{
		base.PhysicsProcessCharacter(delta);
		
		if (!CanMove || MovementPoints.Count == 0 || _hasReachedFinalDestination)
			return;
			
		if (_currentState == MovementState.Moving)
		{
			Vector3 velocity = Velocity;
			
			// Appliquer la gravité
			if (!IsOnFloor() )
			{
				velocity += GetGravity() * (float)delta;
			}
			
			// Si des points de mouvement sont définis, se déplacer vers le point actuel
			if (MovementPoints.Count > 0)
			{
				Vector3 targetPosition = MovementPoints[_currentPointIndex].GlobalPosition;
				
				// Vérifier si le NPC est arrivé au point de destination
				float distance = GlobalPosition.DistanceTo(new Vector3(targetPosition.X, GlobalPosition.Y, targetPosition.Z));
				
				if (distance < _arrivalThreshold)
				{
					// Vérifier si c'est le marker d'arrêt final
					if (StopMarker != null && MovementPoints[_currentPointIndex] == StopMarker)
					{
						// Arrivé au point d'arrêt final - arrêter définitivement
						StopPermanently();
						velocity = Vector3.Zero;
					}
					else
					{
						// Arrivé au point - commencer à attendre
						StartWaiting();
						velocity = Vector3.Zero;
					}
				}
				else
				{
					// Calculer la direction vers le point
					Vector3 direction = (targetPosition - GlobalPosition).Normalized();
					direction.Y = 0; // Conserver le mouvement horizontal
					
					// Orienter le NPC vers la direction du mouvement
					if ( direction != Vector3.Zero)
					{
						// Calculate the target rotation angle based on movement direction
						float targetAngle = Mathf.Atan2(direction.X, direction.Z);
	   
						// Get current rotation angle
						float currentAngle = _meshHolder.Rotation.Y;
						targetAngle += Mathf.DegToRad(90f);
	   
						// Smoothly interpolate between current and target angles
						float newAngle = Mathf.LerpAngle(currentAngle, targetAngle, (float)delta * 10);
	   
						// Apply the new rotation
						_meshHolder.Rotation = new Vector3(0, newAngle, 0);
					}

					
					// Calculer la nouvelle vélocité
					velocity.X = direction.X * Speed;
					velocity.Z = direction.Z * Speed;
					
					// Jouer l'animation de marche
					ChangeAnimation(Animations.WalkingForward);
				}
			}
			else
			{
				// Déplacement aléatoire si aucun point de mouvement n'est défini
				velocity.X = _moveDirection.X * Speed;
				velocity.Z = _moveDirection.Z * Speed;
			}
			
			Velocity = velocity;
			MoveAndSlide();
		}
		else if (_currentState == MovementState.Waiting)
		{
			// Assurer que le NPC reste immobile pendant l'attente
			Velocity = Vector3.Zero;
			
			// Jouer l'animation d'idle pendant l'attente
			ChangeAnimation(Animations.Idle);
		}
		else if (_currentState == MovementState.Stopped)
		{
			// Assurer que le NPC reste immobile quand il est arrêté définitivement
			Velocity = Vector3.Zero;
			
			// Jouer l'animation d'idle quand il est arrêté
			ChangeAnimation(Animations.Idle);
		}
	}
	
	protected override void Die()
	{
		base.Die();
		// Logique supplémentaire pour la mort du NPC si nécessaire
	}
	
	#endregion
	
	#region Movement Methods
	
	private void StartMoving()
	{
		if (_hasReachedFinalDestination)
			return;
			
		_currentState = MovementState.Moving;
		
		if (MovementPoints.Count > 0)
		{
			// Passer au point suivant dans la liste
			_currentPointIndex = (_currentPointIndex + 1) % MovementPoints.Count;
			GD.Print($"Moving to point {_currentPointIndex}");
		}
		else
		{
			// Mouvement aléatoire si aucun point n'est défini
			var randomX = GD.Randf() * 2 - 1;
			var randomZ = GD.Randf() * 2 - 1;
			_moveDirection = new Vector3(randomX, 0, randomZ).Normalized();
			
			// Commencer le timer pour passer à l'état d'attente après un certain temps
			_stateTimer.WaitTime = MoveDuration;
			_stateTimer.Start();
		}
	}
	
	private void StartWaiting()
	{
		_currentState = MovementState.Waiting;
		Velocity = Vector3.Zero;
		
		GD.Print($"Waiting at point {_currentPointIndex} for {WaitDuration} seconds");
		
		// Attendre le temps spécifié avant de passer au point suivant
		_stateTimer.WaitTime = WaitDuration;
		_stateTimer.Start();
	}
	
	private void StopPermanently()
	{
		_currentState = MovementState.Stopped;
		_hasReachedFinalDestination = true;
		Velocity = Vector3.Zero;
		
		GD.Print("NPC has reached final destination and stopped permanently");
		
		// Arrêter le timer pour ne plus changer d'état
		_stateTimer.Stop();
		
		// Jouer une animation d'idle
		ChangeAnimation(Animations.Idle);
	}
	
	private void OnStateChange()
	{
		if (_hasReachedFinalDestination)
			return;
			
		if (_currentState == MovementState.Moving)
		{
			// Si on était en train de bouger (pour des déplacements aléatoires), passer à l'attente
			StartWaiting();
		}
		else
		{
			// Si on était en train d'attendre, passer au mouvement
			StartMoving();
		}
	}
	
	#endregion
	
	#region IInteractable Implementation
	
	public void Interact()
	{
		if (IsInteractable)
		{
			// Logique d'interaction - par exemple, afficher un dialogue
			GD.Print("Interaction avec le NPC: " + DialogText);
		
		}
	}
	
	public void IsNowInteractable()
	{
		if (IsInteractable)
		{
			_isInteractable = true;
			_interactionLabel.Visible = true;
			
		}
	}
	
	public void NoMoreInteractable()
	{
		if (IsInteractable)
		{
			_isInteractable = false;
			_interactionLabel.Visible = false;
		}
	}
	
	#endregion
}
