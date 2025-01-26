using System;
using Godot;

public partial class Game : Node2D
{
	private CanvasLayer MenuCanvas;
	private LineEdit AddressEntry;

	private const int Port = 9999;

	private ENetMultiplayerPeer EnetPeer;

	public override void _Ready()
	{
		MenuCanvas = GetNode<CanvasLayer>("MenuCanvas");
		AddressEntry = GetNode<LineEdit>(
			"MenuCanvas/MainMenu/MarginContainer/VBoxContainer/AddressEntry"
		);
		EnetPeer = new ENetMultiplayerPeer();
	}

	// Hooked to the Host Button pressed signal
	public void OnHostButtonPressed()
	{
		MenuCanvas.Hide();

		EnetPeer.CreateServer(Port);
		Multiplayer.MultiplayerPeer = EnetPeer;

		// Handle peer connection
		Multiplayer.PeerConnected += AddPlayer;

		AddPlayer(Multiplayer.GetUniqueId());
	}

	// Hooked to the Join Button pressed signal
	public void OnJoinButtonPressed()
	{
		MenuCanvas.Hide();

		EnetPeer.CreateClient("localhost", Port);
		Multiplayer.MultiplayerPeer = EnetPeer;
	}

	public void AddPlayer(long peerId)
	{
		Player player = (Player)GD.Load<PackedScene>("res://scenes/player.tscn").Instantiate();
		player.Name = "Player" + peerId;
		AddChild(player);
	}
}
