using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using Game;

namespace Game
{
    public class World : Node
    {
        public Player player = null;

        [Export]
        public NodePath objectSpawnerPath;

        public ObjectSpawner spawner = null;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            spawner = (ObjectSpawner) GetNode(objectSpawnerPath);
        }

        public void CreateLocalPlayer(int id, Vector3 spawnPoint, Vector3 spawnRot, bool inputEnabled = true)
        {
            var playerScene = (PackedScene)ResourceLoader.Load("res://utils/player/Player.tscn");
            player = (Player)playerScene.Instance();
            player.Name = id.ToString();
            player.inputEnabled = inputEnabled;

            GetNode("players").AddChild(player);

            player.SetPlayerPosition(spawnPoint);
            player.SetPlayerRotation(spawnRot);

            spawner.setInit(true);
        }

        public void CreatePuppet(int networkId, uint timestamp, Vector3 pos, Vector3 rot, bool inputEnabled = true)
        {
            var puppet = GetNode("players").GetNodeOrNull(networkId.ToString()) as Puppet;

            if (puppet == null)
            {
                var playerScene = (PackedScene)ResourceLoader.Load("res://utils/player/Puppet.tscn");
                puppet = (Puppet)playerScene.Instance();
                puppet.Name = networkId.ToString();

                GetNode("players").AddChild(puppet);
                puppet.PosUpdate(timestamp, pos, rot);
            }
        }
    }
}