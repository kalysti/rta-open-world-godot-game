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
        GD.Print("on road map enter");
        map = new GridMap();

        AddChild(map);
    }
}
