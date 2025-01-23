using System;
using Godot;

public partial class Player : CharacterBody2D
{
	public AnimationPlayer AnimPlayer;
	public const float Speed = 300.0f;

	public enum CharacterActions {
		none,
		idle,
		walk,
		run,
		bite
	}

    public override void _Ready()
    {
		AnimPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    }

    public override void _PhysicsProcess(double delta)
	{
		HandleMovement(delta);
	}
	public void HandleMovement(double delta){

		Vector2 velocity = Velocity;

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		if (direction != Vector2.Zero)
		{
			velocity.X = direction.X * Speed;
			velocity.Y = direction.Y * Speed;

			HandleAnim(CharacterActions.none);
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Y = Mathf.MoveToward(Velocity.Y, 0, Speed);
		}

		Velocity = velocity;
		MoveAndSlide();
	}
	public void HandleAnim(CharacterActions type){

		switch(type){
			case CharacterActions.idle:
			break;
		}
	}
}
