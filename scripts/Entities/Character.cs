using Godot;
using System.Collections.Generic;
using Cypherless;
using Cypherless.Weapons;

[Tool]
public abstract partial class Character : CharacterBody3D
{
	#region Animations and Meshes
	
	// Enum pour les types d'animations (commun à tous les personnages)
	public enum Animations
	{
		CrouchBack,
		Dance1,
		DrawAssault,
		DrawGun,
		DrawSword,
		Dying,
		Flair,
		Hacking1,
		Idle,
		IdleCrouchGun,
		IdleCrouchSword,
		IdleGun,
		IdleLongAssault,
		IdleSword,
		InwardSlashSword,
		JumpUpGun,
		LoadingBow,
		LongJumpGun,
		RunningForward,
		RunningAssault,
		RunningBack,
		RunningGunBack,
		RunningGunForward,
		RunningGunLeft,
		RunningGunRight,
		RunningGunStraitLeft,
		RunningGunStraitRight,
		RunningLokingBehind,
		RunningPickupLeft,
		RunningPickupRight,
		RunningSword,
		RunningSwordForward,
		StepBackGun,
		TPOSE,
		UncrouchGun,
		WalkingForward,
		WalkingGunBack,
		WalkingGunForward,
		WalkingSwordForward,
		ShootGun,
		ShootAssault,
		CrouchReloadAssault,
		FallingToRoll,
		Roll,
		OutwardSlashSword,
		WalkingReloadAssault,
		WalkingBackward,
		
		
	}
	
	// Dictionnaire commun pour toutes les classes dérivées (peut être étendu par héritage)
	protected static Dictionary<Animations, string> _animationPaths = new Dictionary<Animations, string>
	{
		{Animations.Dance1,"MixamoRig/Dance1"},
		{Animations.DrawGun,"MixamoRig/DrawGun"},
		{Animations.DrawSword ,"MixamoRig/DrawSword"},
		{Animations.Flair ,"MixamoRig/Flair"},
		{Animations.Hacking1,"MixamoRig/Hacking1"},
		{Animations.RunningLokingBehind,"MixamoRig/RunningLokingBehind"},
		{Animations.RunningPickupLeft,"MixamoRig/RunningPickupLeft"},
		{Animations.RunningPickupRight,"MixamoRig/RunningPickupRight"},
		{Animations.LoadingBow,"MixamoRig/LoadingBow"},
		{Animations.CrouchBack,"MixamoRig/CrouchBack"},
		{Animations.IdleCrouchGun,"MixamoRig/IdleCrouchGun"},
		{Animations.Dying,"MixamoRig/Dying"},
		{Animations.DrawAssault,"MixamoRig/DrawAssault"},
		{Animations.IdleGun,"MixamoRig/IdleGun"},
		{Animations.JumpUpGun,"MixamoRig/JumpUpGun"},
		{Animations.IdleLongAssault,"MixamoRig/IdleLongAssault"},
		{Animations.LongJumpGun,"MixamoRig/LongJumpGun"},
		{Animations.RunningAssault,"MixamoRig/RunningAssault"},
		{Animations.RunningGunBack,"MixamoRig/RunningGunBack"},
		{Animations.RunningGunForward,"MixamoRig/RunningGunForward"},
		{Animations.WalkingGunForward,"MixamoRig/WalkingGunForward"},
		{Animations.RunningGunLeft,"MixamoRig/RunningGunLeft"},
		{Animations.RunningGunRight,"MixamoRig/RunningGunRight"},
		{Animations.RunningGunStraitLeft,"MixamoRig/RunningGunLeft"},
		{Animations.RunningGunStraitRight,"MixamoRig/RunningGunStraitRight"},
		{Animations.TPOSE,"MixamoRig/TPOSE"},
		{Animations.UncrouchGun,"MixamoRig/UncrouchGun"},
		{Animations.WalkingGunBack,"MixamoRig/WalkingBackGun"},
		{Animations.StepBackGun,"MixamoRig/StepBackGun"},
		{Animations.RunningForward,"MixamoRig/Running"},
		{Animations.IdleCrouchSword, "MixamoRig/IdleCrouchSword"},
		{Animations.IdleSword,"MixamoRig/IdleSword"},
		{Animations.InwardSlashSword ,"MixamoRig/InwardSlashSword"},
		{Animations.RunningBack,"MixamoRig/RunningBack"},
		{Animations.RunningSword,"MixamoRig/RunningSword"},
		{Animations.RunningSwordForward,"MixamoRig/RunningSwordForward"},
		{Animations.WalkingSwordForward,"MixamoRig/WalkingForwardSword"},
		{Animations.Idle,"MixamoRig/Idle"},
		{Animations.WalkingForward,"MixamoRig/WalkingForward"},
		{ Animations.ShootGun,"MixamoRig/ShootGun"},
		{ Animations.ShootAssault,"MixamoRig/ShootAssault"},
		{ Animations.CrouchReloadAssault,"MixamoRig/CrouchReloadAssault"},
		{ Animations.FallingToRoll,"MixamoRig/FallingToRoll"},
		{ Animations.Roll,"MixamoRig/Roll"},
		{ Animations.OutwardSlashSword,"MixamoRig/OutwardSlashSword"},
		{ Animations.WalkingReloadAssault,"MixamoRig/WalkingReloadAssault"},
		{ Animations.WalkingBackward,"MixamoRig/WalkingBackward"}
	};
	
	// Dictionnaire des meshes disponibles (à définir dans les classes dérivées)
	protected abstract Dictionary<int, string> _meshs { get; set; }
	// Nœud contenant le mesh actuel
	protected Node3D _meshHolder;
	
	// Animation player courant
	protected AnimationPlayer _animation;
	
	// Propriétés pour l'éditeur
	[Export] protected int _actualMesh = 1;
	[Export] protected Animations _actualAnimation = Animations.Idle;
	[Export] protected bool _update = false;
	
	// Squelette pour les attachements
	protected Skeleton3D _skeleton;
	
	#endregion
	
	#region Character Stats
	
	[Export] public float Speed = 5.0f;
	[Export] public float JumpForce = 11f;
	[Export] public int MaxHealth { get; set; } = 10;
	[Export] public int Health { get; set; } = 10;
	
	#endregion
	
	#region Weapons
	
	// Arme principale
	[Export] public PackedScene _weaponScene;
	protected Weapon _weapon;
	protected Marker3D _gunPivot;
	
	// Référence pour le BoneAttachment
	protected BoneAttachment3D _weaponAttachment;
	[Export] protected string _weaponBoneName = "mixamorig_RightHand";
	
	// Offsets pour l'arme
	[Export] protected Vector3 _weaponPositionOffset = new Vector3(0, 0, 0);
	[Export] protected Vector3 _weaponRotationOffset = new Vector3(0, 0, 0);
	[Export] protected Vector3 _weaponScale = new Vector3(100, 100, 100);
	
	#endregion

	private Animations? _currentAnimations = null;
	
	#region Core Methods
	
	public override void _Ready()
	{
		// Initialiser les meshes disponibles si nécessaire
		InitializeMeshes();
		
		// Configurer le mesh holder
		_meshHolder = GetNodeOrNull<Node3D>("MeshHolder");
		if (_meshHolder == null)
		{
			_meshHolder = new Node3D();
			_meshHolder.Name = "MeshHolder";
			AddChild(_meshHolder);
			if (Engine.IsEditorHint())
				_meshHolder.Owner = GetTree().EditedSceneRoot;
		}
		
		// Charger le mesh actuel
		ChangeMesh(_actualMesh);
		
		if (!Engine.IsEditorHint())
		{
			// Initialiser les fonctionnalités spécifiques au personnage
			InitializeCharacter();
			
			_gunPivot = new Marker3D();
			_gunPivot.Name = "GunPivot";
			var _weaponPivotAttach = new BoneAttachment3D();
			_skeleton.AddChild(_weaponPivotAttach);
			_weaponPivotAttach.BoneName = "mixamorig_LeftShoulder";
			_weaponPivotAttach.AddChild(_gunPivot);
			
			// Attachement d'arme si une scène d'arme est fournie
			if (_weaponScene != null)
			{
				GD.Print("");
				CallDeferred(nameof(AttachWeapon));
			}
		}
		else
		{
			// En mode éditeur, on désactive la physique
			SetPhysicsProcess(false);
		}
	}
	
	public override void _Process(double delta)
	{
		if (Engine.IsEditorHint())
		{
			// Mettre à jour le mesh et l'animation en mode éditeur
			if (_update)
			{
				_update = false;
				ChangeMesh(_actualMesh);
				ChangeAnimation(_actualAnimation);
				
				// Réattacher l'arme si nécessaire
				CallDeferred(nameof(AttachWeapon));
			}
		}
		else
		{
			// Traitement spécifique au personnage
			ProcessCharacter(delta);
		}
	}
	
	public override void _PhysicsProcess(double delta)
	{
		if (!Engine.IsEditorHint())
		{
			// Traitement de la gravité si on n'est pas au sol
			if (!IsOnFloor())
			{
				Velocity += GetGravity() * (float)delta;
			}
			
			// Implémentation spécifique au personnage pour le mouvement
			PhysicsProcessCharacter(delta);
			
			// Application du mouvement
			MoveAndSlide();
		}
	}
	
	#endregion
	
	#region Character Management Methods
	
	// À implémenter dans les classes dérivées pour l'initialisation des meshes
	protected virtual void InitializeMeshes() 
	{
		// Par défaut vide, à surcharger dans les classes dérivées
	}
	
	// À implémenter dans les classes dérivées pour l'initialisation spécifique
	protected virtual void InitializeCharacter() { }
	
	// À implémenter dans les classes dérivées pour le traitement spécifique
	protected virtual void ProcessCharacter(double delta) { }
	
	// À implémenter dans les classes dérivées pour la physique spécifique
	protected virtual void PhysicsProcessCharacter(double delta) { }
	
	#endregion
	
	#region Mesh and Animation Methods
	
	// Changer le mesh du personnage
	public virtual void ChangeMesh(int meshIndex)
	{
		if (!_meshs.ContainsKey(meshIndex))
		{
			GD.PrintErr($"Mesh index '{meshIndex}' not found in mesh dictionary");
			return;
		}
		
		string currentAnimation = "";
		double animationTime = 0;
		
		// Sauvegarder l'animation actuelle et sa position
		if (_animation != null)
		{
			currentAnimation = _animation.CurrentAnimation;
			animationTime = _animation.GetCurrentAnimationPosition();
		}
		
		// Supprimer les enfants existants
		foreach (var child in _meshHolder.GetChildren())
		{
			child.Free();
		}
		
		// Charger et ajouter le nouveau mesh
		var meshInstance = GD.Load<PackedScene>(_meshs[meshIndex]).Instantiate<Node3D>();
		_meshHolder.AddChild(meshInstance);
		
		if (Engine.IsEditorHint() && GetTree().EditedSceneRoot != null)
		{
			meshInstance.Owner = GetTree().EditedSceneRoot;
		}
		
		// Obtenir l'animation player
		_animation = FindNode(meshInstance, "AnimationPlayer") as AnimationPlayer;
		
		if (_animation != null)
		{
			// Charger la bibliothèque d'animations
			_animation.AddAnimationLibrary("MixamoRig", GD.Load<AnimationLibrary>("res://3d-assets/Entities/Animations/MixamoRig.glb"));
			
			// Restaurer l'animation si possible
			if (currentAnimation != "")
			{
				try
				{
					_animation.Play(currentAnimation);
					_animation.Advance(animationTime);
				}
				catch (System.Exception e)
				{
					GD.PrintErr($"Failed to restore animation: {e.Message}");
				}
			}
		}
		
		// Mettre à jour l'indice du mesh actuel
		_actualMesh = meshIndex;
		
		// Notifier que le mesh a changé
		OnMeshChanged();
	}
	
	// Jouer une animation spécifique
	public virtual void ChangeAnimation(Animations animation, float blendTime = 0.3f)
	{
		_currentAnimations = animation;
		//Rpc(nameof(_auxChangeAnimation), blendTime);
		_auxChangeAnimation(blendTime);
	}
	
	
	
	[Rpc(MultiplayerApi.RpcMode.AnyPeer,CallLocal = true)]
	private void _auxChangeAnimation( float blendTime = 0.3f)
	{
		var animation = _currentAnimations ?? Animations.Idle;
		if (_animation == null) return;
		
		if (_animationPaths.ContainsKey(animation))
		{
			try
			{
				string animPath = _animationPaths[animation];
				_animation.Play(animPath, blendTime);
				_actualAnimation = animation;
			}
			catch (System.Exception e)
			{
				GD.PrintErr($"Failed to play animation: {e.Message}");
			}
		}
		else
		{
			GD.PrintErr($"Animation {animation} not found in paths dictionary");
		}
	}
	
	// Méthode appelée après un changement de mesh
	protected virtual void OnMeshChanged()
	{
		// Chercher le squelette dans le nouveau mesh
		_skeleton = FindSkeletonInNode(_meshHolder);
		
		// Réattacher l'arme si nécessaire
		if (_weaponScene != null && !Engine.IsEditorHint())
		{
			CallDeferred(nameof(AttachWeapon));
		}
	}
	
	// Trouver un nœud par nom dans un arbre
	protected Node FindNode(Node parent, string name)
	{
		if (parent.Name == name)
			return parent;
			
		foreach (var child in parent.GetChildren())
		{
			var result = FindNode(child, name);
			if (result != null)
				return result;
		}
		
		return null;
	}
	
	// Méthode pour trouver le Skeleton3D dans un nœud et ses enfants
	protected Skeleton3D FindSkeletonInNode(Node node)
	{
		if (node is Skeleton3D skeleton)
			return skeleton;
			
		foreach (Node child in node.GetChildren())
		{
			var result = FindSkeletonInNode(child);
			if (result != null)
				return result;
		}
		
		return null;
	}
	
	#endregion
	
	#region Weapon Management
	
	// Méthode pour attacher l'arme à l'os
	protected virtual void AttachWeapon()
	{
		if (_weaponScene == null) return;
		
		// Attendre que le squelette soit disponible
		if (_meshHolder.GetChildCount() == 0 || _skeleton == null)
		{
			GD.PrintErr("Skeleton3D not found in mesh or mesh not loaded yet!");
			return;
		}
		
		// Trouver l'ID de l'os
		int boneId = _skeleton.FindBone(_weaponBoneName);
		if (boneId == -1)
		{
			GD.PrintErr($"Bone '{_weaponBoneName}' not found! Make sure to use the correct bone name.");
			
			// Debug: afficher tous les noms d'os disponibles
			/*for (int i = 0; i < _skeleton.GetBoneCount(); i++)
			{
				GD.Print($"Bone {i}: {_skeleton.GetBoneName(i)}");
			}
			return;*/
		}
		
		/*/ Supprimer l'ancien attachment s'il existe
		if (_weaponAttachment != null && _weaponAttachment.IsInsideTree())
		{
			_weaponAttachment.QueueFree();
			_weaponAttachment = null;
		}*/
		
		// Créer un BoneAttachment3D
		_weaponAttachment = new BoneAttachment3D();
		_skeleton.AddChild(_weaponAttachment);
		_weaponAttachment.BoneName = _weaponBoneName;
		
		
		if (Engine.IsEditorHint() && GetTree().EditedSceneRoot != null)
		{
			_weaponAttachment.Owner = GetTree().EditedSceneRoot;
		}
		
		try
		{
			// Instancier l'arme comme Node3D
			Weapon weaponNode = _weaponScene.Instantiate<Weapon>();
			weaponNode.Aimcursor = GetNodeOrNull<Marker3D>("CursorPosition");
			GD.Print(weaponNode.Aimcursor);
			_weaponAttachment.AddChild(weaponNode);
			
			if (Engine.IsEditorHint() && GetTree().EditedSceneRoot != null)
			{
				weaponNode.Owner = GetTree().EditedSceneRoot;
			}
			
			// Configurer la position, rotation et échelle de l'arme
			weaponNode.RotationDegrees = _weaponRotationOffset;
			weaponNode.Scale = _weaponScale;
			
			
			
			// Essayer de stocker une référence à l'arme si ce n'est pas en mode éditeur
			if (!Engine.IsEditorHint() && weaponNode is Weapon weapon)
			{
				_weapon = weapon;
				
				// Configurer l'arme
				try 
				{
					_weapon.WeaponOwner = this;
					
					// Créer un timer si nécessaire
					if (!weaponNode.HasNode("UseCooldown"))
					{
						var timer = new Timer();
						timer.Name = "UseCooldown";
						timer.OneShot = true;
						timer.WaitTime = 1.0f;
						weaponNode.AddChild(timer);
						
						if (Engine.IsEditorHint() && GetTree().EditedSceneRoot != null)
						{
							timer.Owner = GetTree().EditedSceneRoot;
						}
					}
					
					GD.Print($"Weapon attached to '{_weaponBoneName}' bone successfully!");
				}
				catch (System.Exception e)
				{
					GD.PrintErr($"Error configuring weapon: {e.Message}");
				}
			}
			else if (!Engine.IsEditorHint())
			{
				GD.Print($"Instanced node is not a Weapon, but keeping it as a visual node");
			}
		}
		catch (System.Exception e)
		{
			GD.PrintErr($"Error instantiating weapon: {e}");
		}
	}
	
	// Méthode pour faire tomber l'arme
	protected virtual void DropWeapon()
	{
		if (_weaponAttachment == null || _weaponAttachment.GetChildCount() == 0 || Engine.IsEditorHint()) return;
		
		try
		{
			// Obtenir le premier enfant (l'arme)
			var weaponNode = _weaponAttachment.GetChild(0);
			
			// Créer un objet d'arme physique
			if (_weapon != null && _weapon.WeaponItem != null)
			{
				var weaponItem = _weapon.WeaponItem.Instantiate<Node3D>();
				GetTree().Root.AddChild(weaponItem);
				
				// Positionner l'objet d'arme
				weaponItem.GlobalPosition = GlobalPosition + new Vector3(0, 0.5f, 0);
			}
			
			// Retirer l'arme
			_weaponAttachment.RemoveChild(weaponNode);
			weaponNode.QueueFree();
			
			// Réinitialiser la référence
			_weapon = null;
		}
		catch (System.Exception e)
		{
			GD.PrintErr($"Error dropping weapon: {e.Message}");
		}
	}
	
	#endregion
	
	#region Health and Damage
	
	// Méthode pour prendre des dégâts
	public virtual void TakeDamage(int amount)
	{
		if (Engine.IsEditorHint()) return;
		
		Health -= amount;
		GD.Print($"{GetType().Name} takes {amount} damage. Remaining HP: {Health}");
		
		// Vérifier si le personnage est mort
		if (Health <= 0)
		{
			Health = 0;
			Die();
		}
	}
	
	// Méthode pour la mort du personnage
	protected virtual void Die()
	{
		if (Engine.IsEditorHint()) return;
		
		ChangeAnimation(Animations.Dying);
		
		// Lâcher l'arme si elle existe
		if (_weapon != null)
		{
			DropWeapon();
		}
		
		// Autres comportements à la mort, à définir dans les classes dérivées
	}
	
	#endregion

	public void GunReloadCallBack()
	{
		ChangeAnimation(_weapon.WeaponType == WeaponTypes.Rifle ? Animations.WalkingReloadAssault : Animations.WalkingReloadAssault);
	}
}
