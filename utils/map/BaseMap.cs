using Godot;
using System;

public class BaseMap : Spatial
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        
    }

    public Vector3 GetSpawnPoint()
    {
        return (GetNode("spawn") as Position3D).GlobalTransform.origin;
    }

}
