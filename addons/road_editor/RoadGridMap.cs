using Godot;
using System;

[Tool]
public class RoadGridMap : Spatial
{
    GridMap map;

    [Export(PropertyHint.Dir)]
    public string assetPath = "res://assets/world/roads/scenes/";

    public override void _EnterTree()
    {
        base._EnterTree();

        if (!Engine.EditorHint)
            Visible = false;
        else
        {
            base._EnterTree();
            map = new GridMap();

            AddChild(map);
        }
    }
}
