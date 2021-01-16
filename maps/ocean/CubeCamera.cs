using System;
using Godot;
using System.Collections.Generic;
namespace Game
{

    [Tool]
    public class CubeCamera : Spatial
    {

        public override void _Ready()
        {
            foreach (Viewport i in GetChildren())
            {
                i.Size = new Vector2(256, 256);
                i.OwnWorld = true;
            }
        }
        public CubeMap UpdateMap()
        {
            Dictionary<string, Godot.CubeMap.Side> images = new Dictionary<string, Godot.CubeMap.Side>();
            images.Add("left", CubeMap.Side.Left);
            images.Add("right", CubeMap.Side.Right);
            images.Add("front", CubeMap.Side.Front);
            images.Add("back", CubeMap.Side.Back);
            images.Add("top", CubeMap.Side.Top);
            images.Add("bottom", CubeMap.Side.Bottom);

            var cube_map = new CubeMap();

            foreach (Viewport i in GetChildren())
            {
                if (images.ContainsKey(i.Name))
                {
                    var img = new Image();
                    img.CopyFrom(i.GetTexture().GetData());
                    cube_map.SetSide(images[i.Name], img);
                }
            }

            return cube_map;
        }
    }
}