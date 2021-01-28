using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using Game;

namespace Game
{
    public class World : BaseWorld
    {
        [Signal]
        public delegate void onMapLoadComplete();

        [Export]
        public NodePath objectSpawnerPath;
        public ObjectSpawner spawner = null;

        public Player player = null;

        [Export]
        public NodePath mapPath;

        public BaseMap map = null;

        public Image baseMapImage = null;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            base._Ready();

            spawner = (ObjectSpawner)GetNode(objectSpawnerPath);
        }


        public void LoadMap()
        {
            var loadingElement = new BackgroundLoaderItem("res://maps/TestMap.tscn");
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
            EmitSignal(nameof(onMapLoadComplete));
        }

        public void CreateLocalPlayer(int id, Vector3 spawnPoint, Vector3 spawnRot, bool inputEnabled = true)
        {
            GD.Print("[Client] Create player at " + spawnPoint);
            var playerScene = (PackedScene)ResourceLoader.Load("res://utils/player/Player.tscn");
            player = (Player)playerScene.Instance();
            player.Name = id.ToString();
            player.inputEnabled = inputEnabled;
            player.world = this;

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
        public Spatial getMapTerrain()
        {
            if (map != null && map.terrain != null)
                return map.terrain;
            else
                return null;
        }

        public Vector3 getMapImagePosition()
        {
            if (map.terrain == null)
                return Vector3.Zero;

            var scale = (Vector3)map.terrain.Get("map_scale");

            return player.GetPlayerPosition() / scale;
        }

        public Color getMapColorByPosition(float x, float z)
        {
            if (baseMapImage != null)
            {
                baseMapImage.Lock();
                var color = baseMapImage.GetPixel((int)x, (int)z);
                baseMapImage.Unlock();

                return color;
            }
            else
                return new Color(0, 0, 0);
        }

        private void loadMapImage()
        {
            var data = map.terrain.Get("_data") as Resource;
            var tf = (AABB)data.Call("get_aabb");
            var texture = (StreamTexture)data.Call("get_texture", 5);
            baseMapImage = texture.GetData();
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