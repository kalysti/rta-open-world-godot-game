using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using Game;

namespace Game
{
    public abstract class BaseWorld : Node
    {
        [Export]
        public NodePath mapHolderPath;
        public BaseMap map = null;

        public Node mapHolder = null;
        public BackgroundLoader backgroundLoader = null;
        public Image baseMapImage = null;
        public NetworkPlayer player = null;


        public override void _Ready()
        {
            mapHolder = (Node)GetNode(mapHolderPath);
            backgroundLoader = GetTree().Root.GetNode("BackgroundLoader") as BackgroundLoader;
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
            if (map == null || map.terrain == null)
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

        protected void loadMapImage()
        {
            var data = map.terrain.Get("_data") as Resource;
            var tf = (AABB)data.Call("get_aabb");
            var texture = (StreamTexture)data.Call("get_texture", 5);
            baseMapImage = texture.GetData();
        }


    }
}