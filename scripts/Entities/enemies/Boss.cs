using Cypherless;
using Godot;

public partial class Boss : CharacterBody3D
{
    public enum State
    {
        Idle,
        Chasing,
        Shooting,
        SpecialAttack
    }

    private Timer attackTimer;
    private PackedScene bulletScene = GD.Load<PackedScene>("res://scenes/bullet.tscn");
    private Marker3D bulletSpawn;
    [Export] public float ChaseRange = 15.0f;
    private State currentState = State.Idle;
    private Area3D detectionArea;
    [Export] public float FireRate = 2.0f;
    [Export] public int Health = 20;

    private NavigationAgent3D navAgent;
    private Node3D player;
    [Export] public float ShootingRange = 10.0f;
    [Export] public float SpecialAttackRate = 8.0f;
    private Timer specialAttackTimer;

    [Export] public float Speed = 3.0f;

    public override void _Ready()
    {
        navAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");
        detectionArea = GetNode<Area3D>("Area3D");
        attackTimer = GetNode<Timer>("Timer");
        specialAttackTimer = new Timer();
        bulletSpawn = GetNode<Marker3D>("Marker3D");

        detectionArea.BodyEntered += OnPlayerDetected;
        detectionArea.BodyExited += OnPlayerLost;
        attackTimer.Timeout += Shoot;
        specialAttackTimer.Timeout += SpecialAttack;

        attackTimer.WaitTime = FireRate;
        attackTimer.OneShot = false;

        specialAttackTimer.WaitTime = SpecialAttackRate;
        specialAttackTimer.OneShot = false;
        AddChild(specialAttackTimer);
    }

    public override void _PhysicsProcess(double delta)
    {
        switch (currentState)
        {
            case State.Idle:
                Idle();
                break;
            case State.Chasing:
                Chase();
                break;
            case State.Shooting:
                ShootAtPlayer();
                break;
            case State.SpecialAttack:
                SpecialAttack();
                break;
        }
    }

    private void Idle()
    {
        if (player != null) currentState = State.Chasing;
    }

    private void Chase()
    {
        if (player == null) return;

        navAgent.TargetPosition = player.GlobalTransform.Origin;
        MoveToTarget();

        if (GlobalTransform.Origin.DistanceTo(player.GlobalTransform.Origin) < ShootingRange)
        {
            currentState = State.Shooting;
            attackTimer.Start();
            specialAttackTimer.Start();
        }
    }

    private void ShootAtPlayer()
    {
        if (player == null)
        {
            currentState = State.Idle;
            return;
        }

        LookAt(player.GlobalTransform.Origin, Vector3.Up);
    }

    private void Shoot()
    {
        if (player == null) return;

        GD.Print("The boss is shooting.");

        var bulletInstance = (RigidBody3D)bulletScene.Instantiate();
        bulletInstance.GlobalTransform = bulletSpawn.GlobalTransform;
        bulletInstance.ApplyImpulse(Vector3.Zero, -bulletInstance.GlobalTransform.Basis.Z * 10);
        GetParent().AddChild(bulletInstance);
    }

    private void SpecialAttack()
    {
        if (player == null) return;

        GD.Print("The boss is attacking.");

        for (var i = 0; i < 5; i++)
        {
            var bulletInstance = (RigidBody3D)bulletScene.Instantiate();
            bulletInstance.GlobalTransform = bulletSpawn.GlobalTransform;
            var randomDir = new Vector3(GD.Randf() * 2 - 1, 0, GD.Randf() * 2 - 1).Normalized();
            bulletInstance.ApplyImpulse(Vector3.Zero, randomDir * 10);
            GetParent().AddChild(bulletInstance);
        }
    }

    private void MoveToTarget()
    {
        var direction = (navAgent.GetNextPathPosition() - GlobalTransform.Origin).Normalized();
        Velocity = new Vector3(direction.X * Speed, Velocity.Y, direction.Z * Speed);
        MoveAndSlide();
    }

    private void OnPlayerDetected(Node body)
    {
        if (body is Player)
        {
            player = (Node3D)body;
            currentState = State.Chasing;
        }
    }

    private void OnPlayerLost(Node body)
    {
        if (body == player)
        {
            player = null;
            currentState = State.Idle;
            attackTimer.Stop();
            specialAttackTimer.Stop();
        }
    }

    public void TakeDamage(int amount)
    {
        Health -= amount;
        GD.Print($"The boss is taking damage. Remaining HP: {Health}.");
        if (Health <= 0)
        {
            GD.Print("The boss is dead.");
            QueueFree();
        }
    }
}