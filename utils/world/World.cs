using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using Game;
using Newtonsoft.Json;

namespace Game
{
    public class World : BaseWorld
    {
        [Signal]
        public delegate void onMapLoadComplete();

        [Export]
        public NodePath objectSpawnerPath;
        public ObjectSpawner spawner = null;

        [Export]
        public string mapScenePath = "res://maps/TestMapDev.tscn"; // res://maps/TestMap.tscn


        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            base._Ready();
            spawner = (ObjectSpawner)GetNode(objectSpawnerPath);
        }

        public void LoadMap()
        {
            var loadingElement = new BackgroundLoaderItem(mapScenePath);
            loadingElement.Connect("CompleteLoadEvent", this, "InitMap");
            backgroundLoader.Load(loadingElement);
        }

        private void InitMap(Resource res)
        {
            GD.Print("[Client] Map completly loaded");

            var scene = (PackedScene)res;
            map = (BaseMap)scene.Instance();
            map.Connect("MapLoadedComplete", this, "MapLoaded");
            map.Name = "map";

            spawner.clearMapObjects(map);
            mapHolder.CallDeferred("add_child", map);
        }

        public void MapLoaded()
        {
            loadMapImage();
            EmitSignal(nameof(onMapLoadComplete));
        }

        public void CreateLocalPlayer(int id, Vector3 spawnPoint, Vector3 spawnRot, string characterJson, bool inputEnabled = true)
        {
            GD.Print("[Client] Create player at " + spawnPoint);
            var playerScene = (PackedScene)ResourceLoader.Load("res://utils/player/Player.tscn");
            var p = (Player)playerScene.Instance();
            p.inputEnabled = inputEnabled;
            player = p;
            player.Name = id.ToString();
            player.world = this;

            var character = JsonConvert.DeserializeObject<OnlineCharacter>(characterJson);
            if (character == null)
                return;

            player.setCharacter(character);

            GetNode("players").AddChild(player);


            player.SetPlayerPosition(spawnPoint);
            player.SetPlayerRotation(spawnRot);

            spawner.player = player;
            spawner.startScanThread();

            //enableAudio on map
            (GetNode("map_holder/map") as BaseMap).enableAudio();
        }

        public void setSSAO(bool ssao_on)
        {
            (GetNode("WorldEnvironment") as WorldEnvironment).Environment.SsaoEnabled = ssao_on;
        }

        public void CreatePuppet(int networkId, uint timestamp, Vector3 pos, Vector3 rot, bool inputEnabled = true)
        {
            var puppet = GetNode("players").GetNodeOrNull(networkId.ToString()) as Puppet;

            if (puppet == null)
            {
                var playerScene = (PackedScene)ResourceLoader.Load("res://utils/player/Puppet.tscn");
                puppet = (Puppet)playerScene.Instance();
                puppet.Name = networkId.ToString();
                puppet.world = this;

                GetNode("players").AddChild(puppet);
                puppet.PosUpdate(timestamp, pos, rot);
            }
        }
    }
}