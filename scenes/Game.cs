using System;
using Godot;

public partial class Game : Node2D
{
    public override void _Ready()
    {
        var peer = new ENetMultiplayerPeer();

        if (Array.Exists(OS.GetCmdlineArgs(), arg => arg == "--host"))
        {
            peer.CreateServer(8910);
            SpawnPlayer(1); // Host player
        }
        else
        {
            peer.CreateClient("localhost", 8910);
        }
        // When a new player connects, create their player
        Multiplayer.PeerConnected += (long id) =>
        {
            GD.Print($"Peer {id} connected, spawning their player");
            SpawnPlayer(id);
        };
        // Start using this peer for networking
        Multiplayer.MultiplayerPeer = peer;
    }

    private void SpawnPlayer(long id)
    {
        var player = GD.Load<PackedScene>("res://scenes/player.tscn").Instantiate<Player>();
        player.Name = id.ToString();
        AddChild(player);
    }
}
