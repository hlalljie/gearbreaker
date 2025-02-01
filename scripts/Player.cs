using System;
using Godot;

public partial class Player : CharacterBody2D
{
	public const float Speed = 300.0f;
	public int Health = 100; // Our Player's Health

	private Camera2D Camera;
	
	private bool isInvincible = false; // ✅ Prevents repeated damage
	private float invincibleTime = 1.0f; // ✅ Time (in seconds) before player can be hit again


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

		HandleMovement(delta);
	}

	public void HandleMovement(double delta)
	{
		Vector2 velocity = Velocity;

		// Get the input direction and handle the movement/deceleration.
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

	// Handle attacks
	public void OnHurtBoxEntered(Area2D area)
	{
		GD.Print($"🛑 Player hit! Checking attack properties...");
		GD.Print($"🔍 Object Name: {area.Name}, Type: {area.GetType()}, Groups: {string.Join(",", area.GetGroups())}");

		if (area.IsInGroup("attack")) 
		{
			GD.Print("✅ Attack detected in 'attack' group! Now checking type...");

			if (area is Attack attack)
			{
				GD.Print($"✅ Attack is a valid Attack instance! Damage: {attack.Damage}");
				TakeDamage(attack.Damage);
			}
			else
			{
				GD.Print("⚠️ Error: The object belongs to 'attack' group but is NOT an Attack instance.");
			}
		}
	}




	// Apply damage to player
	[Rpc]
	public void TakeDamage(int damage)
	{
		if (isInvincible) // ✅ Ignore damage if invincible
		{
			GD.Print("🚫 Player is invincible. No damage taken.");
			return;
		}

		GD.Print($"🛡️ TakeDamage called with damage: {damage}");
		Health -= damage;
		GD.Print($"💔 Player took {damage} damage! Health after: {Health}");

		if (Health <= 0)
		{
			Die();
		}
		else
		{
			// ✅ Activate invincibility and set a timer to turn it off
			isInvincible = true;
			GetTree().CreateTimer(invincibleTime).Timeout += () =>
			{
				isInvincible = false;
				GD.Print("🟢 Player is vulnerable again.");
			};
		}
	}



	// Handle death
	private void Die()
	{
		GD.Print("Player has been defeated!");
		QueueFree(); // Removes the player from the scene (can replace with respawn logic)
	}

	// Keep only one of the GetPlayerId methods
	private int GetPlayerId()
	{
		string numberPart = Name.ToString().Replace("Player", "");
		return int.Parse(numberPart);
	}
}
