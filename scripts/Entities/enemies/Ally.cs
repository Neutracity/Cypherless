using Godot;

public partial class Ally : CharacterBody3D
{
    public enum State
    {
        Following,
        Attacking
    }

    [Export] public float AttackRange = 8.0f;
    private Timer attackTimer;
    private PackedScene bulletScene = GD.Load<PackedScene>("res://scenes/bullet.tscn");
    private Marker3D bulletSpawn;
    private State currentState = State.Following;
    private Area3D detectionArea;
    [Export] public float FireRate = 1.2f;
    [Export] public float FollowDistance = 3.0f;
    [Export] public int Health = 10;

    private NavigationAgent3D navAgent;
    private Node3D player;

    [Export] public float Speed = 3.0f;
    private Node3D targetEnemy;

    public override void _Ready()
    {
        navAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");
        detectionArea = GetNode<Area3D>("Area3D");
        attackTimer = GetNode<Timer>("Timer");
        bulletSpawn = GetNode<Marker3D>("Marker3D");

        detectionArea.BodyEntered += OnEnemyDetected;
        detectionArea.BodyExited += OnEnemyLost;
        attackTimer.Timeout += Shoot;

        attackTimer.WaitTime = FireRate;
        attackTimer.OneShot = false;

        player = GetTree().GetFirstNodeInGroup("Player") as Node3D;
    }

    public override void _PhysicsProcess(double delta)
    {
        switch (currentState)
        {
            case State.Following:
                FollowPlayer();
                break;
            case State.Attacking:
                AttackEnemy();
                break;
        }
    }

    private void FollowPlayer()
    {
        if (player == null) return;

        var distanceToPlayer = GlobalTransform.Origin.DistanceTo(player.GlobalTransform.Origin);

        if (distanceToPlayer > FollowDistance)
        {
            navAgent.TargetPosition = player.GlobalTransform.Origin;
            MoveToTarget();
        }
    }

    private void AttackEnemy()
    {
        if (targetEnemy == null)
        {
            currentState = State.Following;
            attackTimer.Stop();
            return;
        }

        LookAt(targetEnemy.GlobalTransform.Origin, Vector3.Up);
    }

    private void Shoot()
    {
        if (targetEnemy == null) return;

        GD.Print("The ennemy is shooting.");

        var bulletInstance = (RigidBody3D)bulletScene.Instantiate();
        bulletInstance.GlobalTransform = bulletSpawn.GlobalTransform;
        bulletInstance.ApplyImpulse(Vector3.Zero, -bulletInstance.GlobalTransform.Basis.Z * 10);
        GetParent().AddChild(bulletInstance);
    }

    private void MoveToTarget()
    {
        var direction = (navAgent.GetNextPathPosition() - GlobalTransform.Origin).Normalized();
        Velocity = new Vector3(direction.X * Speed, Velocity.Y, direction.Z * Speed);
        MoveAndSlide();
    }

    private void OnEnemyDetected(Node body)
    {
        if (body.IsInGroup("Enemy"))
        {
            targetEnemy = (Node3D)body;
            currentState = State.Attacking;
            attackTimer.Start();
        }
    }

    private void OnEnemyLost(Node body)
    {
        if (body == targetEnemy)
        {
            targetEnemy = null;
            currentState = State.Following;
            attackTimer.Stop();
        }
    }

    public void TakeDamage(int amount)
    {
        Health -= amount;
        GD.Print($"The ally is taking damage. Remaining HP: {Health}.");
        if (Health <= 0) QueueFree();
    }
}