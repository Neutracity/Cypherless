using System;
using Cypherless;
using Cypherless.Weapons;
using Godot;
using Godot.Collections;
[Tool][Obsolete]
public partial class OutdateMeleeEnemy : CharacterBody3D,IDamagable 
{
    public enum State
    {
        Patrolling,
        Chasing,
        Attacking,
        Dead,
    }
    
    private Godot.Collections.Dictionary<int,string> EnemyMeshes = new Godot.Collections.Dictionary<int,string>()
    {
        {1,"res://3d-assets/Entities/Enemies/Ennemis_armure_rockets_dorsale.blend"},
        {2,"res://3d-assets/Entities/Enemies/GazMask.blend"},
        {3,"res://3d-assets/Entities/Enemies/Swatguy.blend"},
        {4,"res://3d-assets/Entities/Enemies/Le_mechant_ultime.blend"},
        {5,"res://3d-assets/Entities/Enemies/Robot_femme.blend"},
        {6,"res://3d-assets/Entities/Enemies/Soldat_full_noir.blend"},
        {7,"res://3d-assets/Entities/Enemies/Soldat_militaire.blend"},
        {8,"res://3d-assets/Entities/Enemies/Soldat_militaire_casque.blend"},
        {9,"res://3d-assets/Entities/Enemies/SwatLittleGuy.blend"}
        
    };
    
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
        IdleCrouchGun,
        IdleCrouchSword,
        IdleGun,
        IdleLongAssault,
        IdleSword,
        InwardSlashSword,
        JumpUpGun,
        LoadingBow,
        LongJumpGun,
        Running,
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
        StepBackGun,
        TPOSE,
        UncrouchGun,
        WalkingGunBack,
        WalkingGunForward,
        RunningSwordForward,
        WalkingSwordForward,
    }

    private Godot.Collections.Dictionary<Animations, string> _animations = new()
    {
        { Animations.Dance1,"MixamoRig/Dance1"},
        { Animations.DrawGun,"MixamoRig/DrawGun"},
        { Animations.DrawSword ,"MixamoRig/DrawSword"},
        { Animations.Flair ,"MixamoRig/Flair"},
        {Animations.Hacking1,"MixamoRig/Hacking1"},
        { Animations.RunningLokingBehind,"MixamoRig/RunningLokingBehind"},
        { Animations.RunningPickupLeft,"MixamoRig/RunningPickupLeft"},
        { Animations.RunningPickupRight,"MixamoRig/RunningPickupRight"},
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
        { Animations.StepBackGun,"MixamoRig/StepBackGun"},
        { Animations.Running,"MixamoRig/Running"},
        { Animations.IdleCrouchSword, "MixamoRig/IdleCrouchSword"},
        { Animations.IdleSword,"MixamoRig/IdleSword"},
        { Animations.InwardSlashSword ,"MixamoRig/InwardSlashSword"},
        {Animations.RunningBack,"MixamoRig/RunningBack"},
        {Animations.RunningSwordForward,"MixamoRig/RunningSwordForward"},
        {Animations.WalkingSwordForward,"MixamoRig/RunningSwordForward"},

    };

    private Node3D _meshHolder;
    [Export] private int _actualMesh = 1;
    [Export] private Animations _actualAnimation;
    [Export] private bool _update = false;
    
    // Ajout de l'attribut pour stocker l'arme
    [Export] private PackedScene _weaponScene = GD.Load<PackedScene>("res://scenes/Weapons/Swords/ExoKatana.tscn");
    private Weapon _weapon;
    
    // Références pour le skeleton et le BoneAttachment
    private Skeleton3D _skeleton;
    private BoneAttachment3D _weaponAttachment;
    [Export] private string _weaponBoneName = "mixamorig_RightHand"; // Nom de l'os où attacher l'arme (à ajuster selon votre modèle)
    
    // Ajoutez ces variables aux autres variables de classe
    private Timer _patrolWaitTimer;
    private bool _isWaiting = false;
    private Timer _damageTimer;

    [Export] public float AttackRange = 1.5f;
    private Timer _attackTimer;
    [Export] public float ChaseRange = 5.0f;
    private State _currentState = State.Patrolling;
    private Area3D _detectionArea;
    private AnimationPlayer _animation;
    [Export] public int MaxHealth { get; set; } = 3;
    [Export] public int Health { get; set; } = 3;

    private bool _isNavSynchronized;

    private NavigationAgent3D _navAgent;
    private Player _player;

    [Export] public float Speed = 2.0f;

    // Dans la méthode _Ready, ajoutez l'initialisation du timer après les autres initialisations
    public override void _Ready()
    {
        if (!Engine.IsEditorHint())
        {
            _navAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");
            _detectionArea = GetNode<Area3D>("Area3D");
            _attackTimer = GetNode<Timer>("Timer");
        
            // Ajoutez ces lignes
            _patrolWaitTimer = new Timer();
            _patrolWaitTimer.OneShot = true;
            _patrolWaitTimer.Timeout += OnPatrolWaitComplete;
            AddChild(_patrolWaitTimer);
            
            // Timer pour le délai des dégâts
            _damageTimer = new Timer();
            _damageTimer.OneShot = true;
            _damageTimer.Timeout += ApplyDamageToPlayer;
            AddChild(_damageTimer);
        
            CallDeferred(nameof(EnableNavigation));
        }
        else
        {
            SetPhysicsProcess(false);
        }
        
        _meshHolder = GetNode<Node3D>("MeshHolder");
        ChangeMesh(_actualMesh);
        
        // Attacher l'arme après le chargement du mesh
        CallDeferred(nameof(AttachWeapon));
    }

    private void EnableNavigation()
    {
        _isNavSynchronized = true;
    }

    // Méthode pour attacher l'arme à l'os
    private void AttachWeapon()
    {
        if (_weaponScene == null) return;
        
        // Attendre que le squelette soit disponible
        if (_meshHolder.GetChildCount() == 0) return;
        
        // Obtenir le Skeleton3D
        _skeleton = FindSkeletonInNode(_meshHolder.GetChild(0));
        
        if (_skeleton == null)
        {
            GD.PrintErr("Skeleton3D non trouvé dans le mesh!");
            return;
        }
        
        /*// Debug: afficher tous les noms d'os disponibles
        for (int i = 0; i < _skeleton.GetBoneCount(); i++)
        {
            GD.Print($"Bone {i}: {_skeleton.GetBoneName(i)}");
        }*/
        
        // Trouver l'ID de l'os
        int boneId = _skeleton.FindBone(_weaponBoneName);
        if (boneId == -1)
        {
            GD.PrintErr($"Os '{_weaponBoneName}' non trouvé! Assurez-vous d'utiliser le bon nom d'os.");
            return;
        }
        
        // Créer un BoneAttachment3D
        _weaponAttachment = new BoneAttachment3D();
        _skeleton.AddChild(_weaponAttachment);
        _weaponAttachment.BoneName = _weaponBoneName;
        
        // Instancier l'arme en utilisant Instantiate() sans type générique
        var weaponInstance = GD.Load<PackedScene>(_weaponScene.GetPath()).Instantiate<Node3D>() ;
        
        // Vérifier si l'instance est du type Weapon
        if (weaponInstance is Node3D node3D)
        {
            //_weapon = node3D;
            _weaponAttachment.AddChild(node3D);
        
            // Configurer l'arme
            //_weapon.WeaponOwner = this;
        
            // Ajuster la position et rotation de l'arme si nécessaire
            node3D.Position = new Vector3(0, 0, 0); // Ajustez selon vos besoins
            node3D.Rotation = new Vector3(0, 0, 0); // Ajustez selon vos besoins
            node3D.Scale = new Vector3(1, 1, 1) * 100; // Ajustez selon vos besoins
        
            GD.Print($"Arme attachée à l'os '{_weaponBoneName}' avec succès!");
        }
        else
        {
            // L'objet instancié n'est pas une arme
            GD.PrintErr("La scène instanciée n'est pas du type Weapon mais du type " + weaponInstance.GetType().Name);
        
            // Libérer l'instance et le BoneAttachment
            weaponInstance.QueueFree();
            _weaponAttachment.QueueFree();
        }
    }
    
    // Méthode pour trouver le Skeleton3D dans un nœud et ses enfants
    private Skeleton3D FindSkeletonInNode(Node node)
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

    public override void _Process(double delta)
    {
        if (!Engine.IsEditorHint())
        {
            if (!_isNavSynchronized) return;
            switch (_currentState)
            {
                case State.Patrolling:
                    Patrol();
                    break;
                case State.Chasing:
                    Chase();
                    break;
                case State.Attacking:
                    // Ne rien faire pendant l'attaque, on attend la fin du timer
                    break;
                case State.Dead:
                    break;
            }
        }
        else
        {
            if (_update)
            {
                _update = false;
                ChangeMesh(_actualMesh);
                ChangeAnimation(_actualAnimation);
                
                // Réattacher l'arme si nécessaire
                CallDeferred(nameof(AttachWeapon));
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        /*GD.Print("Is target Reachable?" + _navAgent.IsTargetReachable() + "Is Finished ? " + _navAgent.IsNavigationFinished() + "Velocity " + Velocity);*/
        /*GD.Print(_currentState);*/
        if (!IsOnFloor())
            Velocity += GetGravity() * (float)delta;
    }

    // Modifiez la méthode Patrol
    private void Patrol()
    {
        if (_isWaiting)
        {
            // Pendant la pause, on continue de jouer l'animation idle
            ChangeAnimation(Animations.IdleGun);
            return;
        }

        if (_navAgent.IsNavigationFinished())
        {
            // Démarrer la pause
            _isWaiting = true;
            float waitTime = (float)GD.RandRange(1.0, 7.0);
            _patrolWaitTimer.Start(waitTime);
        
            // Jouer l'animation idle pendant l'attente
            ChangeAnimation(Animations.IdleGun);
            return;
        }

        MoveToTarget();
    }

    // Ajoutez cette nouvelle méthode
    private void OnPatrolWaitComplete()
    {
        _isWaiting = false;
        
        // Plus forte probabilité de mouvement sur l'axe X
        float xOffset = (float)GD.RandRange(-10, 10); // Plus grande amplitude sur X
        float zOffset = (float)GD.RandRange(-3, 3);   // Plus petite amplitude sur Z
        
        // Position actuelle
        var currentPos = GlobalTransform.Origin;
        
        // Nouvelle position cible avec préférence pour l'axe X
        var randomTarget = currentPos + new Vector3(
            xOffset,
            0,
            zOffset
        );
        
        _navAgent.TargetPosition = randomTarget;
    }

    private void ChangeMesh(int meshIndex)
    {
        string currentAnimation = "";
        double animationTime = 0;
        if (_animation != null )
        {
            currentAnimation = _animation.CurrentAnimation;
            animationTime = _animation.GetCurrentAnimationPosition();
        }
        
        foreach (var child in _meshHolder.GetChildren())
        {
            child.Free();
        }
        
        _meshHolder.AddChild(GD.Load<PackedScene>(EnemyMeshes[meshIndex]).Instantiate<Node3D>());

        _animation = _meshHolder.GetChild(0).GetNode<AnimationPlayer>("AnimationPlayer");
        _animation.AddAnimationLibrary("MixamoRig", GD.Load<AnimationLibrary>("res://3d-assets/Entities/Animations/MixamoRig.blend"));
        
        if (currentAnimation != "")
        {
            _animation.Play(currentAnimation);
            _animation.Advance(animationTime);
        }
    }

    private void ChangeAnimation(Animations animation)
    {
        if (_animation != null)
        {
            _animation.Play(_animations[animation], 0.3);
        }
    }
    
    private void Chase()
    {
        if (_player == null) return;

        _navAgent.TargetPosition = _player.GlobalTransform.Origin;
        
        // Vérifier si à portée d'attaque
        if (GlobalTransform.Origin.DistanceTo(_player.GlobalTransform.Origin) < AttackRange)
        {
            if (_attackTimer.TimeLeft == 0)
            {
                // Passer en mode attaque
                _currentState = State.Attacking;
                Attack();
            }
        }
        else
        {
            // Continuer à poursuivre
            MoveToTarget();
        }
    }

    private void Attack()
    {
        if (_player == null) return;

        // Arrêter le mouvement
        Velocity = new Vector3(0, Velocity.Y, 0);
        
        // Tourner vers le joueur
        Vector3 directionToPlayer = (_player.GlobalTransform.Origin - GlobalTransform.Origin).Normalized();
        if (directionToPlayer != Vector3.Zero)
        {
            var angle = Mathf.Atan2(directionToPlayer.X, directionToPlayer.Z);
            _meshHolder.Rotation = new Vector3(0, angle, 0);
        }
        
        // Jouer l'animation d'attaque
        ChangeAnimation(Animations.InwardSlashSword);
        
        // Utiliser l'arme si elle est disponible
        if (_weapon != null && _weapon.IsUsable)
        {
            _weapon.UseWeapon();
        }
        
        // Démarrer le timer pour appliquer les dégâts après 0.5 secondes
        _damageTimer.Start(0.5f);
        
        // Démarrer le timer pour le cooldown
        _attackTimer.Start(1.5f);
    }
    
    // Méthode pour appliquer les dégâts après le délai
    private void ApplyDamageToPlayer()
    {
        if (_player == null || _currentState == State.Dead) return;
        
        // Vérifier si le joueur est toujours à portée d'attaque
        if (GlobalTransform.Origin.DistanceTo(_player.GlobalTransform.Origin) < AttackRange * 1.2f) // Légère marge
        {
            int damage = _weapon != null ? _weapon.Damage : 1; // Utiliser les dégâts de l'arme si disponible
            _player.TakeDamage(damage);
            GD.Print($"Damage applied to player: {damage}");
        }
    }

    private void MoveToTarget()
    {
        var currentPos = GlobalTransform.Origin;
        var targetPos = _navAgent.GetNextPathPosition();
        var direction = (targetPos - currentPos).Normalized();
        
        // Calculate speed based on state
        float tspeed = _currentState == State.Chasing ? Speed * 1.5f : Speed;
        
        // Update velocity while preserving Y
        Velocity = new Vector3(direction.X * tspeed, Velocity.Y, direction.Z * tspeed);
        
        // Rotate the mesh to face the movement direction
        if (direction != Vector3.Zero)
        {
            var angle = Mathf.Atan2(direction.X, direction.Z);
            _meshHolder.Rotation = new Vector3(0, angle, 0);
        }
        
        // Update animation based on state
        ChangeAnimation(_currentState == State.Chasing 
        ? Animations.RunningSwordForward 
        : Animations.WalkingSwordForward);
        
        MoveAndSlide();
    }

    private void OnPlayerDetected(Node body)
    {
        if (body is Player p && _currentState != State.Dead)
        {
            _player = p;
            _currentState = State.Chasing;
        }
    }

    private void OnPlayerLost(Node body)
    {
        if (body == _player && _currentState != State.Dead)
        {
            _player = null;
            _currentState = State.Patrolling;
        }
    }

    private void OnAttackCooldown()
    {
        if(_currentState == State.Dead) return;
        
        if (_player == null)
        {
            _currentState = State.Patrolling;
            return;
        }

        // Une fois l'attaque terminée, revenir à l'état de poursuite
        _currentState = State.Chasing;
        
        // Si toujours à portée, attaquer à nouveau
        if (GlobalTransform.Origin.DistanceTo(_player.GlobalTransform.Origin) < AttackRange)
        {
            _currentState = State.Attacking;
            Attack();
        }
    }

    public void TakeDamage(int amount)
    {
        if (_currentState == State.Dead) return;
        Health -= amount;
        GD.Print($"The ennemy is taking damage. Remaining HP: {Health}.");
        if (Health <= 0)
        {
            _currentState = State.Dead;
            ChangeAnimation(Animations.Dying);
            
            // Détacher l'arme à la mort et la faire tomber au sol
            if (_weapon != null && _weaponAttachment != null)
            {
                DropWeapon();
            }
        };
    }
    
    // Méthode pour faire tomber l'arme au sol
    private void DropWeapon()
    {
        if (_weapon == null) return;
        
        // Créer un WeaponItem à partir de l'arme
        var weaponItem = _weapon.WeaponItem.Instantiate() as Node3D;
        GetTree().Root.AddChild(weaponItem);
        
        // Positionner l'objet d'arme au sol près de l'ennemi
        weaponItem.GlobalPosition = GlobalPosition + new Vector3(0, 0.5f, 0);
        
        // Retirer l'arme de l'ennemi
        _weaponAttachment.RemoveChild(_weapon);
        _weapon = null;
    }

    public void OnNavigationLinkReached(Dictionary d)
    {
    }
}