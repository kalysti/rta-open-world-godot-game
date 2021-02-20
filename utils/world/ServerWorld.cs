using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using Game;

namespace Game
{
    public class ServerWorld : BaseWorld
    {
        [Export]
        public NodePath objectSpawnerPath;

        [Export]
        public string mapPath = "res://maps/TestMapDev.tscn"; // res://maps/TestMap.tscn

        [Signal]
        public delegate void mapLoaded();

        public ObjectSpawnerServer spawner = null;

        public override void _Ready()
        {
            base._Ready();
            spawner = (ObjectSpawnerServer)GetNode(objectSpawnerPath);

            LoadMap();
        }
        public void LoadMap()
        {
            var loadingElement = new BackgroundLoaderItem(mapPath);
            loadingElement.Connect("CompleteLoadEvent", this, "InitMap");
            backgroundLoader.Load(loadingElement);
        }

        private void InitMap(Resource res)
        {
            GD.Print("[Server] Map completly loaded");

            var scene = (PackedScene)res;
            map = (BaseMap)scene.Instance();
            map.Name = "map";
            map.Connect("MapLoadedComplete", this, "MapLoaded");

            mapHolder.CallDeferred("add_child", map);
        }

        public void MapLoaded()
        {
            EmitSignal(nameof(mapLoaded));
            spawner.Init(GetNode("map_holder/map") as BaseMap);
        }

    }
}
