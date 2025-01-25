using System;
using Godot;

public partial class Player : CharacterBody2D
{
	public const float Speed = 300.0f;

	private Camera2D Camera;

	public override void _EnterTree()
	{
		SetMultiplayerAuthority(GetPlayerId());
	}

	public override void _Ready()
	{
		if (!IsMultiplayerAuthority())
			return;
		Camera = GetNode<Camera2D>("Camera");
		Camera.MakeCurrent();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!IsMultiplayerAuthority())
			return;
		// Only process input if we're the authority for this player
		HandleMovement(delta);

	}

	public void HandleMovement(double delta)
	{
		Vector2 velocity = Velocity;

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		if (direction != Vector2.Zero)
		{
			velocity.X = direction.X * Speed;
			velocity.Y = direction.Y * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Y = Mathf.MoveToward(Velocity.Y, 0, Speed);
		}

		Velocity = velocity;
		MoveAndSlide();
	}

	private int GetPlayerId()
	{
		string numberPart = Name.ToString().Replace("Player", "");
		return int.Parse(numberPart);
	}
}
