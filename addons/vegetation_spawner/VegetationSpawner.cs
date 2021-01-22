using Godot;
using System;

[Tool]
public class VegetationSpawner : Spatial
{
    [Export]
    public Godot.Collections.Array<PackedScene> scenes = new Godot.Collections.Array<PackedScene>();

    [Export]
    public float radius = 10.0f;

    [Export]
    public float spread = 5.0f;

    [Export]
    public int amount = 10;

    [Export]
    public float minScale = 0.8f;

    private NodePath _terrainPath = null;

    [Export]
    public NodePath terrainPath
    {
        get
        {

            return _terrainPath;
        }
        set
        {
            _terrainPath = value;
            setTerrain(value);
        }
    }

    public Node terrain = null;

    public override void _Ready()
    {
        setTerrain(terrainPath);
    }

    public void setTerrain(string path)
    {
        if (!String.IsNullOrEmpty(path))
        {
            terrain = GetNodeOrNull(terrainPath);
        }
    }
}
