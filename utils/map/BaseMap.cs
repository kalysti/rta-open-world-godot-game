using Godot;
using System;

public class BaseMap : Spatial
{
    [Export]
    public NodePath terrainPath;

    public Spatial terrain = null;

    [Signal]
    public delegate void MapLoadedComplete();

    public override void _Ready()
    {
        terrain = (Spatial)GetNode(terrainPath);
        EmitSignal(nameof(MapLoadedComplete));
    }

    public Vector3 GetSpawnPoint()
    {
        return (GetNode("spawn") as Position3D).GlobalTransform.origin;
    }
    public void enableAudio()
    {
        foreach (var item in GetNode("audio").GetChildren())
        {
            if (item is AudioStreamPlayer3D)
            {
                (item as AudioStreamPlayer3D).Autoplay = true;
                (item as AudioStreamPlayer3D).Playing = true;
            }

            if (item is AudioStreamPlayer)
            {
                (item as AudioStreamPlayer).Autoplay = true;
                (item as AudioStreamPlayer).Playing = true;
            }
        }
    }

}
