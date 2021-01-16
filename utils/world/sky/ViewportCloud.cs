using Godot;
using System;

[Tool]
public class ViewportCloud : Viewport
{
    [Export]
    public NodePath envPath;
    public WorldEnvironment environment { get; set; }
    public SpriteSky sky { get; set; }

    private float waitTime = 1;

    private bool nodeInit = false;

    public override void _Ready()
    {
        environment = GetNode(envPath) as WorldEnvironment;
        sky = GetNode("Sprite") as SpriteSky;
        (environment.Environment.BackgroundSky as PanoramaSky).Panorama = GetTexture();
    }
    public override void _Process(float delta)
    {
        //fix a bug with viewport after late init
        if (sky.nodeInit && !nodeInit)
        {
            (environment.Environment.BackgroundSky as PanoramaSky).Panorama = GetTexture();
            nodeInit = true;
        }
    }

}
