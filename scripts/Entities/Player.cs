using Cypherless;
using Cypherless.Weapons;
using Godot;
using System.Collections.Generic;

namespace Cypherless;

[Tool]
public partial class Player : Character
{
    #region Player-specific properties
    
    [Export] public int PlayerIndex = 0;
    [Export] public float CameraDistance = 5.0f;
    
    // Mouvement et contrôle
    private Vector2 _inputDir = Vector2.Zero;
    private Vector2 _gamepadv = Vector2.Zero;
    private Vector2 _currentBlendVector = Vector2.Zero;
    private Vector2 _beforecursor = Vector2.Zero;
    private bool _wasFalling = false;
    private bool right = true;
    private bool Moving = false;
    
    // Gestion de caméra
    private ShakeController _camera;
    private float _curentfov;
    private float _maxfov = 90f;
    private float _minfov = 70f;
    
    // Armes et interaction
    [Export] public Weapon Primary;
    [Export] public Weapon Secondary;
    private Marker3D _cursorPosition;
    private Marker3D _weaponPosition;
    private Area3D _interactionarea;
    private IInteractable _interactableObject = null;
    private bool IsIsMenu => _interactableObject != null;
    private SceneTreeTimer _dropStrengthTimer;
    
    // Contrôles et État
    public bool IsGamepad;
    private RedDamageFade _damageHud;
    private IDamagable _damagableImplementation;
    
    
    protected override Dictionary<int, string> _meshs { get; set; } = new Dictionary<int, string>
    {
        { 1, "res://3d-assets/Entities/Player/MixamoRigDavid.glb" },
        {2, "res://3d-assets/Entities/Player/Scientifique_noire_chauve.glb"},
    };

    #endregion
    
    // Add a MultiplayerSynchronizer property
    
    #region Initialization
    
    public override void _EnterTree()
    {
        if (!Engine.IsEditorHint())
        {
            SetMultiplayerAuthority(int.Parse(Name));
            GD.Print($"Player name :{Name}");
        }
    }


    public override void _Ready()
    {
        // Appeler la méthode de la classe parente pour initialiser les meshes
        base._Ready();
        
        
        // Only the authority should process input
        //SetProcess(IsMultiplayerAuthority());
        //SetPhysicsProcess(IsMultiplayerAuthority());
        
        // Réinitialisation des valeurs de santé
        Health = 10;
        MaxHealth = 10;
        
        
        // Initialiser les composants du joueur
        _camera = GetNode<ShakeController>("Camera3D");
        _camera.Current = IsMultiplayerAuthority();
        _camera.CameraDistance = CameraDistance;
        _curentfov = _minfov;
        
        _interactionarea = GetNode<Area3D>("InteractionArea");
        _cursorPosition = GetNode<Marker3D>("CursorPosition");
        IsGamepad = PlayerIndex != 0;
        _beforecursor = GetViewport().GetMousePosition();
        _weaponPosition = GetNode<Marker3D>("WeaponPosition");
        _damageHud = GetNode<RedDamageFade>("Camera3D/damage_hud/red_damage_fade");
        _damageHud.Hide();
        
        // Jouer l'animation d'idle par défaut
        ChangeAnimation(Animations.Idle);
    }
    
    #endregion
    
    #region Character Processing
    
    protected override void ProcessCharacter(double delta)
    {
        // Gestion des animations et mouvement en fonction des armes
        if (Health <= 0) return;
        UpdateWeaponAnimations();
        if (IsMultiplayerAuthority())
        {
            // Gestion de la santé
            CheckDamage();
        
            // Mise à jour des contrôles
            if (PlayerIndex == 0) UpdateIsGamepad();
            UpdateInteraction();
            UpdateCursorPos();
            Rpc(nameof(UpdateGunPos),delta,0.6f);
            UpdateCamera(delta);
            UpdateWeapon(delta);
            Rpc(nameof(UpdateDavidRotation));
            
            // Mise à jour des animations en fonction du mouvement
            bool isRunning = Input.IsActionPressed("sprint" + PlayerIndex);
            _currentBlendVector = _currentBlendVector.Lerp(_inputDir * (isRunning ? 1.0f : 0.5f), (float)delta * 10);
            
            // Debug features
            UpdateDebugThing();
            
        }
        
        // Gestion des animations de saut
        if (_wasFalling && IsOnFloor())
        {
            _wasFalling = false;
            ChangeAnimation(Animations.Idle);
        }
    }
    
    protected override void PhysicsProcessCharacter(double delta)
    {
        if (Health <= 0) return;
        var velocity = Velocity;
        
        // Ajouter la gravité si le joueur n'est pas au sol
        if (!IsOnFloor())
        {
            velocity += GetGravity() * (float)delta;
            _wasFalling = true;
        }
        
        if (IsMultiplayerAuthority())
        {
            // Gestion du saut
            if (Input.IsActionJustPressed("jump" + PlayerIndex) && IsOnFloor())
            {
                velocity.Y = JumpForce;
                ChangeAnimation(Animations.JumpUpGun);
            }
            
            // Gestion du mouvement horizontal
            _inputDir = Input.GetVector("move_left" + PlayerIndex, "move_right" + PlayerIndex, "move_up" + PlayerIndex,
                "move_down" + PlayerIndex);
            var direction = (Transform.Basis * new Vector3(_inputDir.X, 0, _inputDir.Y)).Normalized();
            
            if (direction != Vector3.Zero)
            {
                bool isRunning = Input.IsActionPressed("sprint" + PlayerIndex);
                velocity.X = direction.X * Speed * (isRunning ? 2.0f : 1.0f);
                velocity.Z = direction.Z * Speed * (isRunning ? 2.0f : 1.0f);
                
                // Choisir l'animation de déplacement appropriée
                if (isRunning)
                {
                    ChooseMovementAnimation(true);
                }
                else
                {
                    ChooseMovementAnimation(false);
                }
            }
            else
            {
                // Décélération
                velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
                velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
                
                // Animation d'idle
                ChooseIdleAnimation();
            }
        }
        
        Moving = velocity != Vector3.Zero;
        Velocity = velocity;
    }
    
    #endregion
    
    #region Animation Management
    
    private void UpdateWeaponAnimations()
    {
        // Adapter cette méthode pour utiliser le nouveau système d'animations au lieu de l'AnimationTree
        if (Primary != null)
        {
            // Configurer la position de l'arme et des animations spécifiques à l'arme
            if (Primary is Gun)
            {
                _weaponBoneName = "mixamorig_RightHand"; // Ajuster selon vos besoins
            }
            else if (Primary is Sword1)
            {
                _weaponBoneName = "mixamorig_RightHand"; // Ajuster selon vos besoins
            }
        }
    }
    
    private void ChooseIdleAnimation()
    {
        if (Primary != null)
        {
            if (Primary is Sword1)
            {
                ChangeAnimation(Animations.IdleSword);
            }
            else if (Primary is Gun)
            {
                ChangeAnimation(Animations.IdleGun);
            }
            else
            {
                ChangeAnimation(Animations.Idle);
            }
        }
        else
        {
            ChangeAnimation(Animations.Idle);
        }
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer,CallLocal = true)]
    private void ChooseMovementAnimation(bool isRunning)
    {
        if (Primary != null)
        {
            if (Primary is Sword1)
            {
                if (isRunning)
                {
                    ChangeAnimation(Animations.RunningSwordForward);
                }
                else
                {
                    ChangeAnimation(Animations.WalkingSwordForward);
                }
            }
            else if (Primary is Gun)
            {
                if (_inputDir.Y < 0) // Reculer
                {
                    if (isRunning)
                    {
                        ChangeAnimation(Animations.RunningGunBack);
                    }
                    else
                    {
                        ChangeAnimation(Animations.WalkingGunBack);
                    }
                }
                else // Avancer
                {
                    if (isRunning)
                    {
                        ChangeAnimation(Animations.RunningGunForward);
                    }
                    else
                    {
                        ChangeAnimation(Animations.WalkingGunForward);
                    }
                }
            }
            else
            {
                if (isRunning)
                {
                    ChangeAnimation(Animations.RunningForward);
                }
                else
                {
                    ChangeAnimation(Animations.WalkingForward);
                }
            }
        }
        else
        {
            if (_inputDir.Y < 0) // Reculer
            {
                if (isRunning)
                {
                    ChangeAnimation(Animations.RunningBack);
                }
                else
                {
                    ChangeAnimation(Animations.WalkingForward); // Utiliser l'animation de marche standard
                }
            }
            else // Avancer
            {
                if (isRunning)
                {
                    ChangeAnimation(Animations.RunningForward);
                }
                else
                {
                    ChangeAnimation(Animations.WalkingForward);
                }
            }
        }
    }
    
    #endregion
    
    #region Input and Interaction
    
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured)
        {
            // Vous pouvez ajouter ici la gestion du mouvement de la caméra si nécessaire
        }
    }
    
    private void UpdateIsGamepad()
    {
        var mousePos = GetViewport().GetMousePosition();
        
        if (mousePos != _beforecursor && IsGamepad)
        {
            IsGamepad = false;
            _beforecursor = mousePos;
            GD.Print("BeforeCursor is " + _beforecursor);
        }
        else if (Input.IsJoyButtonPressed(0, JoyButton.A) || _gamepadv != Vector2.Zero)
        {
            IsGamepad = true;
        }
    }
    
    private void UpdateInteraction()
    {
        if (Input.IsActionJustPressed("interact" + PlayerIndex))
        {
            if (_interactableObject is not null)
            {
                _interactableObject.Interact();
            }
        }
    }
    
    private void UpdateDebugThing()
    {
        if (Input.IsActionJustPressed("debug_spawn_gun" + PlayerIndex))
            Rpc(nameof(AddWeapon),"res://scenes/Weapons/Guns/HandGuns/J42.tscn");
    }
    
    private void UpdateCamera(double delta)
    {
        var targetFov = Mathf.Lerp(_minfov, _maxfov, Velocity.Length() / 40);
        _curentfov = (float)Mathf.MoveToward(_curentfov, targetFov, delta * 10);
        _camera.Fov = _curentfov;
    }
    
    private void UpdateCursorPos()
    {
        _gamepadv = Input.GetVector("look_left" + PlayerIndex, "look_right" + PlayerIndex, "look_up" + PlayerIndex,
            "look_down" + PlayerIndex);
        if (IsGamepad && _gamepadv != Vector2.Zero)
        {
            _cursorPosition.GlobalPosition = new Vector3(_gamepadv.X, -_gamepadv.Y, 0) + _weaponPosition.GlobalPosition;
        }
        else if (!IsGamepad)
        {
            var mousePosition = GetViewport().GetMousePosition();
            
            var from = _camera.ProjectRayOrigin(mousePosition);
            var to = from + _camera.ProjectRayNormal(mousePosition) * 1000;
            
            var plane = new Plane(Vector3.Forward, _cursorPosition.GlobalTransform.Origin);
            float intersectionDistance;
            Vector3? intersectionPos;
            if ((intersectionPos = plane.IntersectsRay(from, to)) is not null)
            {
                intersectionDistance = _camera.GlobalPosition.DistanceTo((Vector3)intersectionPos);
                var intersectionPoint = from + (to - from).Normalized() * intersectionDistance;
                
                intersectionPoint.Z = _cursorPosition.GlobalTransform.Origin.Z; // Keep the Z position of the marker
                if (_weaponPosition.GlobalPosition.DistanceTo(intersectionPoint) > 0.1)
                    _cursorPosition.GlobalPosition = intersectionPoint;
            }
        }
    }
    
    [Rpc(MultiplayerApi.RpcMode.AnyPeer,CallLocal = true)]
    private void UpdateDavidRotation()
    {
        // Dans le nouveau système, nous devons tourner le MeshHolder
        if (_cursorPosition.GlobalPosition.X > GlobalPosition.X)
        {
            _meshHolder.Rotation = new Vector3(0, Mathf.DegToRad(90), 0);
            right = true;
        }
        else
        {
            _meshHolder.Rotation = new Vector3(0, Mathf.DegToRad(-90), 0);
            right = false;
        }
    }
    
    #endregion
    
    #region Weapon Management
    
    private void UpdateWeapon(double delta)
    {
        if(Primary is null) return;
        if (IsIsMenu) return;
        if (Input.IsActionJustPressed("drop" + PlayerIndex)) _dropStrengthTimer = GetTree().CreateTimer(1);
        
        if (Input.IsActionJustReleased("drop" + PlayerIndex) && _dropStrengthTimer is not null)
        {
            ShootTheWeapon((float)(100 / (10 * _dropStrengthTimer.TimeLeft + 1)));
            _dropStrengthTimer.TimeLeft = 0;
        }
        
        if (Input.IsActionJustPressed("shoot" + PlayerIndex))
        {
            if (Primary.IsUsable)
            {
                //Primary.UseWeapon(
                Primary.UseWeapon();
            }
            else if (Primary is Gun gun)
            {
                if (gun.IsEmpty)
                {
                    ShootTheWeapon(30);
                }
            }
        }
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer,CallLocal = true)]
    public void ShootTheWeapon(float strength = 5f)
    {
        if (IsMultiplayerAuthority())
        {
            if (Primary.WeaponItem is not null)
            {
                WeaponItem item = Primary.WeaponItem.Instantiate<WeaponItem>();
                item.IsPickable = false;
                item.WeaponScene = GD.Load<PackedScene>(Primary.GetSceneFilePath());
                GetTree().Root.AddChild(item);
                
                
                item.GlobalPosition = Primary.GlobalPosition;
                Vector3 impulseDir = -Primary.Transform.Basis.Z * strength;
                Vector3 torque = Vector3.Up.Rotated(Vector3.Right, GD.Randf() * Mathf.Pi) * strength;
                
                item.CallDeferred("ApplyForces", impulseDir, torque*100f);
            }
            Primary.QueueFree();
            Primary = Secondary;
            Primary.Aimcursor = _cursorPosition;
            Primary.Show();
            Secondary = null;
        }
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer,CallLocal = true)]
    private void AddWeapon(string packedSceneWeaponPath)
    {
        {
            var packedSceneWeapon = GD.Load<PackedScene>(packedSceneWeaponPath);
            if (Primary is null)
            {
                // Pour le système de Character, on doit utiliser une autre méthode d'attachement
                // Stocker le packedScene pour utiliser la méthode AttachWeapon plus tard
                if (packedSceneWeapon.Instantiate() is Weapon weapon)
                {
                    AddChild(weapon);
                    weapon.WeaponOwner = this;
                    Primary = weapon;
                    weapon.GlobalPosition = _weaponPosition.GlobalPosition;
                    
                    // Changer l'animation pour corresponder à l'arme
                    if (weapon is Sword1)
                    {
                        ChangeAnimation(Animations.DrawSword);
                    }
                    else if (weapon is Gun gun)
                    {
                        ChangeAnimation(Animations.DrawGun);
                        gun.Aimcursor = _cursorPosition;
                    }
                }
            }
            else if (Secondary is null)
            {
                var weapon = packedSceneWeapon.Instantiate<Weapon>();
                AddChild(weapon);
                Secondary = weapon;
                weapon.GlobalPosition = GlobalPosition + new Vector3(0,2,0);
                weapon.Hide();
            }
            GD.Print($"PrimaryName : '{Primary?.Name}', SecondaryName : '{Secondary?.Name}'");
        }
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer,CallLocal = true)]
    private void UpdateGunPos(double delta, float followDistance = 0.6f)
    {
        if (_gunPivot == null || _weaponPosition == null) return;
        
        var distance = _gunPivot.GlobalPosition.DistanceTo(_cursorPosition.GlobalPosition);
        
        if (distance > followDistance)
        {
            var direction = (_gunPivot.GlobalPosition - _cursorPosition.GlobalPosition).Normalized();
            var desiredPosition = _gunPivot.GlobalPosition - direction * followDistance;
            _weaponPosition.GlobalPosition = _weaponPosition.GlobalPosition.Lerp(desiredPosition, (float)(10 * delta));
            if (Primary is not null)
                Primary.GlobalPosition = _weaponPosition.GlobalPosition;
        }
    }
    
    #endregion
    
    /*
    // Override TakeDamage to ensure it's properly synchronized
    [Rpc(MultiplayerApi.RpcMode.Authority)]
    public override void TakeDamage(int amount)
    {
        // Only the server can modify health
        if (!Multiplayer.IsServer()) 
        {
            // If we're not the server, request the server to apply damage
            RpcId(1, nameof(TakeDamage), amount);
            return;
        }
        
        base.TakeDamage(amount);
        
        // Notify all clients about the health change
        Rpc(nameof(SyncHealth), Health);
        
        if (IsMultiplayerAuthority())
        {
            GetNode<ShakeController>("Camera3D").OnHit();
            if(Health <= MaxHealth * 0.20)
                _damageHud.Visible = true;
            if (Health <= 0) Die();
        }
    }
    
    [Rpc(MultiplayerApi.RpcMode.Authority)]
    public void SyncHealth(int newHealth)
    {
        // Update health for clients
        Health = newHealth;
        
        // Update HUD if this is the local player
        if (IsMultiplayerAuthority())
        {
            CheckDamage();
        }
    }
    */
    
    #region Health and Damage


    public override void TakeDamage(int amount)
    {
        base.TakeDamage(amount);
        
        if (IsMultiplayerAuthority())
        {
            GetNode<ShakeController>("Camera3D").OnHit();
            if(Health <= MaxHealth * 0.20)
                _damageHud.Visible = true;
            if (Health <= 0) Die();
        }
    }
    
    protected override void Die()
    {
        base.Die();
        
        // Actions spécifiques au joueur lors de la mort
        if (IsMultiplayerAuthority())
        {
            CollisionLayer = 0;
            CollisionMask = 1;
        
        
            // Ajouter un timer pour supprimer l'ennemi après un certain délai
            Timer deathTimer = new Timer();
            deathTimer.WaitTime = 10.0f; // 10 secondes pour laisser l'animation de mort se terminer
            deathTimer.OneShot = true;
            deathTimer.Timeout += () => QueueFree();
            AddChild(deathTimer);
            deathTimer.Start();
            // Logique supplémentaire spécifique au joueur
        }
    }
    
    public void CheckDamage()
    {
        if (!IsMultiplayerAuthority()) return;
        if (Health < ((float)MaxHealth)*0.50)
        {
            _damageHud.Visible = true;
        }
        else 
        {
            _damageHud.Visible = false;
        }
    }
    
    #endregion
    
    #region Collision Detection
    
    private void BodyEntered(Node3D body)
    {
        if (Primary is null || Secondary is null)
        {
            var pickuparea = GetNode<Area3D>("PickupArea");
            var pickuparray = pickuparea.GetOverlappingBodies();
            
            foreach (var item in pickuparray)
            {
                if (item is WeaponItem weaponItem)
                {
                    if (weaponItem.IsPickable)
                    {
                        AddWeapon(weaponItem.WeaponScene.GetPath());
                        item.QueueFree();
                        break;
                    }
                }
            }
        }
    }
    
    public void _on_interaction_area_body_entered(Node3D body)
    {
        if (body is IInteractable interactable)
        {
            if (_interactableObject is not null)
                _interactableObject.NoMoreInteractable();
            _interactableObject = interactable;
            _interactableObject.IsNowInteractable();
            GD.Print("IIIIIIIINTEACTABLE");
        }
    }
    
    public void _on_interaction_area_body_exited(Node3D body)
    {
        if (body == _interactableObject)
        {
            _interactableObject!.NoMoreInteractable();
            _interactableObject = null;
        }
    }
    
    #endregion
    
}