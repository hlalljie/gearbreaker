using Godot;
using System;

public partial class Attack : Area2D
{
	public int Damage = 10; // Default attack damage

	public override void _Ready()
	{
		GD.Print("Attack is ready!");
	}
}
