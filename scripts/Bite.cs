using Godot;
using System;

public partial class Bite : Attack // ✅ Inherit from Attack instead of Area2D
{
	public override void _Ready()
	{
		GD.Print("Bite attack is ready!");
	}

	// You can override damage or effects here
}
