using System;
using Cypherless;
using Cypherless.Weapons;
using Godot;
[Obsolete]
public  partial class OutdatedPlayer : CharacterBody3D, IDamagable
{
    public const float Speed = 5.0f;
    private AnimationTree _animationTree;
    private Vector2 _beforecursor;
    private ShakeController _camera;
    [Export] public float CameraDistance;
    private float _curentfov;
    private Vector2 _currentBlendVector;
    private Marker3D _cursorPosition;
    private Vector2 _gamepadv;
    private Marker3D _gunPivot;
    private Marker3D _weaponPosition;
    private Vector2 _inputDir;
    private Area3D _interactionarea;
    public bool IsGamepad;
    [Export] public float JumpVelocity = 4.5f;
    private float _maxfov = 90f;
    private float _minfov = 70f;
    public bool Moving;
    public string _name = "1";
    [Export] public int PlayerIndex;
    private bool _wasFalling;
    private bool IsIsMenu => _interactableObject != null;
    private RedDamageFade _damageHud;
    private IInteractable _interactableObject = null;
    [Export] public Weapon Primary;
    [Export] public Weapon Secondary;
    public int Health { get; set; } = 5;
    public int MaxHealth { get; set; } = 5;
    private SceneTreeTimer _dropStrengthTimer;
    private bool right = true;

    public void TakeDamage(int damage = 1)
    {
        Health -= damage;
        GetNode<ShakeController>("Camera3D").OnHit();
        if (Health <= 0) QueueFree();
    }

    public override void _EnterTree()
    {
        SetMultiplayerAuthority(int.Parse(Name));
    }

    public override void _Ready()
    {
        _camera = GetNode<ShakeController>("Camera3D");
        _camera.Current = IsMultiplayerAuthority();
        _camera.CameraDistance = CameraDistance;
        _curentfov = _minfov;
        _interactionarea = GetNode<Area3D>("InteractionArea");
        _cursorPosition = GetNode<Marker3D>("CursorPosition");
        IsGamepad = PlayerIndex != 0;
        _beforecursor = GetViewport().GetMousePosition();
        _weaponPosition = GetNode<Marker3D>("WeaponPosition");
        _animationTree = GetNode<AnimationTree>("AnimationTree");
        _currentBlendVector = Vector2.Zero;
        _gunPivot = GetNode<Marker3D>("David/Sci-Fi Rigged male character/rig/Skeleton3D/BoneAttachment3D/GunPivot");
        _damageHud = GetNode<RedDamageFade>("Camera3D/damage_hud/red_damage_fade");
    }

    public override void _Process(double delta)
    {
        var t = _inputDir;
        if (!Input.IsActionPressed("sprint" + PlayerIndex))
        {
            t = _inputDir / 2;
            _animationTree.Set("parameters/JumpHandle/conditions/IsRunning", false);
        }
        else
        {
            _animationTree.Set("parameters/JumpHandle/conditions/IsRunning", true);
        }

        _currentBlendVector = _currentBlendVector.Lerp(t, (float)delta * 10);
        _animationTree.Set("parameters/WalkRun/blend_position", _currentBlendVector.X * (right? 1 : -1));


        if (Primary == null)
        {
            GetNode<SkeletonModifier3D>("David/Sci-Fi Rigged male character/rig/Skeleton3D/SkeletonIK3D").Active =
                false;
        }
        else
        {
            GetNode<SkeletonModifier3D>("David/Sci-Fi Rigged male character/rig/Skeleton3D/SkeletonIK3D").Active = true;
            GetNode<SkeletonIK3D>("David/Sci-Fi Rigged male character/rig/Skeleton3D/SkeletonIK3D").TargetNode =
                "../../../../../WeaponPosition";
        }


        if (_wasFalling && IsOnFloor())
        {
            _wasFalling = false;
            _animationTree.Set("parameters/JumpHandle/conditions/HasLanded", true);
        }
        
        CheckDamage();
        if (IsMultiplayerAuthority())
        {
            if (PlayerIndex == 0) UpdateIsGamepad();
            UpdateInteraction();
            UpdateCursorPos();
            UpdateGunPos(delta);
            UpdateCamera(delta);
            UpdateWeapon(delta);
            UpdateDavidRotation();
            // Remove When Publishing The Game
            UpdateDebugThing();
        }
        

        /*GD.Print(_animationTree.Get("parameters/WalkRun/blend_position"));*/
        /*GD.Print(animationTree.Get("parameters/OneShot/active"));*/
        /*GD.Print($"Transition : {animationTree.Get("parameters/Transition/current_state")} \\nHasJumped : {animationTree.Get("parameters/JumpHandle/conditions/HasJumped")} \\nHas Landed : {animationTree.Get("parameters/JumpHandle/conditions/HasLanded")}");*/
    }

    public override void _PhysicsProcess(double delta)
    {
        var velocity = Velocity;
        
        // Add the gravity.
        if (!IsOnFloor())
        {
            velocity += GetGravity() * (float)delta;
            _wasFalling = true;
        }

        if (IsMultiplayerAuthority())
        {
            // Handle Jump.
            if (Input.IsActionJustPressed("jump" + PlayerIndex) && IsOnFloor())
            {
                velocity.Y = JumpVelocity;

                _animationTree.Set("parameters/JumpHandle/conditions/HasLanded", false);
                _animationTree.Set("parameters/OneShot/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);
            }

            //Handle Direction
            _inputDir = Input.GetVector("move_left" + PlayerIndex, "move_right" + PlayerIndex, "move_up" + PlayerIndex,
                "move_down" + PlayerIndex);
            var direction = (Transform.Basis * new Vector3(_inputDir.X, 0, _inputDir.Y)).Normalized();
            if (direction != Vector3.Zero)
            {
                if (Input.IsActionPressed("sprint" + PlayerIndex))
                    velocity.X = direction.X * Speed * 2F;
                else
                    velocity.X = direction.X * Speed;
            }
            else
            {
                velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
            }
        }






        Moving = velocity != Vector3.Zero;
        
        Velocity = velocity;
        MoveAndSlide();
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
            AddWeapon(GD.Load<PackedScene>("res://scenes/Weapons/Guns/HandGuns/J42.tscn"));
    }

    private void UpdateCamera(double delta)
    {
        var targetFov = Mathf.Lerp(_minfov, _maxfov, Velocity.Length() / 40);
        _curentfov = (float)Mathf.MoveToward(_curentfov, targetFov, delta * 10);
        _camera.Fov = _curentfov;
    }
    
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
            Secondary = null;
        }
    }

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
            if (Primary.IsUsable )
            {
                Primary.UseWeapon();
            }else if (Primary is Gun gun){
                if (gun.IsEmpty)
                {
                    ShootTheWeapon(30);
                }
            }
        }
        
    }

    private void AddWeapon(PackedScene packedSceneWeapon)
    {
        if (IsMultiplayerAuthority())
        {
            if (Primary is null)
            {
                var weapon = packedSceneWeapon.Instantiate<Weapon>();
                AddChild(weapon);
                Primary = weapon;
                weapon.GlobalPosition = _weaponPosition.GlobalPosition;
            }
            else if (Secondary is null)
            {
                var weapon = packedSceneWeapon.Instantiate<Weapon>();
                AddChild(weapon);
                Secondary = weapon;
                weapon.GlobalPosition = _weaponPosition.GlobalPosition;
            }
            GD.Print($"PrimaryName : '{Primary?.Name}', SecondaryName : '{Secondary?.Name}'");
        }
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

    private void UpdateDavidRotation()
    {
        var davidMesh = GetNode<Node3D>("David");
        if (_cursorPosition.GlobalPosition.X > GlobalPosition.X)
        {
            davidMesh.SetRotation(new Vector3(0,90,0));
            right = true;
        }
        else
        {
            davidMesh.SetRotation(new Vector3(0,-90,0));
            right = false;
        }
        
    }
    
    private void UpdateGunPos(double delta, float followDistance = 0.6f)
    {
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
                        AddWeapon(weaponItem.WeaponScene);
                        item.QueueFree();
                        break;
                    }
                    
                }
                    
            }
        }
    }

    public void CheckDamage()
    {
        if (Health < ((float)MaxHealth)*0.50)
        {
            _damageHud.Visible = true;
        }
        else 
        {
            _damageHud.Visible = false;
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
}