using System;
using Godot;

public partial class Game : Node2D
{
	private PanelContainer MainMenu;
	private LineEdit AddressEntry;

	private const int Port = 9999;

	private ENetMultiplayerPeer EnetPeer;

	public override void _Ready()
	{
		MainMenu = GetNode<PanelContainer>("CanvasLayer/MainMenu");
		AddressEntry = GetNode<LineEdit>(
			"CanvasLayer/MainMenu/MarginContainer/VBoxContainer/AddressEntry"
		);
		EnetPeer = new ENetMultiplayerPeer();
	}

	// Hooked to the Host Button pressed signal
	public void OnHostButtonPressed()
	{
		MainMenu.Hide();

		EnetPeer.CreateServer(Port);
		Multiplayer.MultiplayerPeer = EnetPeer;

		// Handle peer connection
		Multiplayer.PeerConnected += AddPlayer;

		AddPlayer(Multiplayer.GetUniqueId());
	}

	// Hooked to the Join Button pressed signal
	public void OnJoinButtonPressed()
	{
		MainMenu.Hide();

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
