using System;
using Godot;

public partial class Player : CharacterBody2D
{
    public const float Speed = 300.0f;
    private MultiplayerSynchronizer _sync;

    public override void _Ready()
    {
        _sync = GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer");
        _sync.SetMultiplayerAuthority(Convert.ToInt32(Name));
    }


    public override void _PhysicsProcess(double delta)
    {
        // Only process input if we're the authority for this player
        if (_sync.IsMultiplayerAuthority())
        {
            HandleMovement(delta);
        }

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
}
