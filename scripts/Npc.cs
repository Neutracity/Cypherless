using Godot;

public partial class NPC : CharacterBody3D
{
	public enum State
	{
		Moving,
		Waiting
	}

	private State currentState = State.Moving;
	private Vector3 moveDirection;
	[Export] public float MoveDuration = 3.0f;

	[Export] public float Speed = 2.0f;

	private Timer stateTimer;
	[Export] public float WaitDuration = 2.0f;

	public override void _Ready()
	{
		stateTimer = GetNode<Timer>("Timer");
		stateTimer.Timeout += OnStateChange;

		StartMoving();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (currentState == State.Moving)
		{
			Velocity = moveDirection * Speed;
			MoveAndSlide();
		}
	}

	private void StartMoving()
	{
		currentState = State.Moving;

		var randomX = GD.Randf() * 2 - 1;
		var randomY = GD.Randf() * 2 - 1;
		moveDirection = new Vector3(randomX, randomY, 0).Normalized();

		stateTimer.WaitTime = MoveDuration;
		stateTimer.Start();
	}

	private void StartWaiting()
	{
		currentState = State.Waiting;
		Velocity = Vector3.Zero;

		stateTimer.WaitTime = WaitDuration;
		stateTimer.Start();
	}

	private void OnStateChange()
	{
		if (currentState == State.Moving)
			StartWaiting();
		else
			StartMoving();
	}
}
