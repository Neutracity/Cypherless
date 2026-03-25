using System;
using Cypherless;
using Godot;
[Obsolete]
public partial class OutdatedRangedEnemy : CharacterBody3D
{
    public enum State
    {
        Patrolling,
        Chasing,
        Shooting
    }

    private PackedScene bulletScene = GD.Load<PackedScene>("res://scenes/bullet.tscn");
    private Marker3D bulletSpawn;
    [Export] public float ChaseRange = 10.0f;
    private State currentState = State.Patrolling;
    private Area3D detectionArea;
    [Export] public float FireRate = 1.5f;
    [Export] public int Health = 5;

    private NavigationAgent3D navAgent;
    private Node3D player;
    [Export] public float ShootingRange = 7.0f;
    private Timer shootingTimer;

    [Export] public float Speed = 2.5f;

    public override void _Ready()
    {
        navAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");
        detectionArea = GetNode<Area3D>("Area3D");
        shootingTimer = GetNode<Timer>("Timer");
        bulletSpawn = GetNode<Marker3D>("Marker3D");

        detectionArea.BodyEntered += OnPlayerDetected;
        detectionArea.BodyExited += OnPlayerLost;
        shootingTimer.Timeout += Shoot;

        shootingTimer.WaitTime = FireRate;
        shootingTimer.OneShot = false;
    }

    public override void _PhysicsProcess(double delta)
    {
        switch (currentState)
        {
            case State.Patrolling:
                Patrol();
                break;
            case State.Chasing:
                Chase();
                break;
            case State.Shooting:
                ShootAtPlayer();
                break;
        }
    }

    private void Patrol()
    {
        if (navAgent.IsNavigationFinished())
        {
            var randomTarget = GlobalTransform.Origin + new Vector3(
                GD.Randf() * 10 - 5,
                0,
                GD.Randf() * 10 - 5);
            navAgent.TargetPosition = randomTarget;
        }

        MoveToTarget();
    }

    private void Chase()
    {
        if (player == null) return;

        navAgent.TargetPosition = player.GlobalTransform.Origin;
        MoveToTarget();

        if (GlobalTransform.Origin.DistanceTo(player.GlobalTransform.Origin) < ShootingRange)
        {
            currentState = State.Shooting;
            shootingTimer.Start();
        }
    }

    private void ShootAtPlayer()
    {
        if (player == null)
        {
            currentState = State.Patrolling;
            return;
        }

        LookAt(player.GlobalTransform.Origin, Vector3.Up);
    }

    private void Shoot()
    {
        if (player == null) return;

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
            currentState = State.Patrolling;
            shootingTimer.Stop();
        }
    }

    public void TakeDamage(int amount)
    {
        Health -= amount;
        GD.Print($"The ennemy is taking damage. Remaining HP: {Health}.");
        if (Health <= 0) QueueFree();
    }
}