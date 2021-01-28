using Godot;
using System;

[Tool]
public class MeshLod : Spatial
{
    [Export(PropertyHint.Range, "0.0, 1000.0, 0.1")]
    public float lod_1_max_distance = 20;

    [Export(PropertyHint.Range, "0.0, 1000.0, 0.1")]
    public float lod_2_max_distance = 50;

    [Export]
    public bool enableLoding = true;

    private float lod_bias = 0.0f;

    private float timer = 0.0f;

    [Export]
    public float refreshRate = 0.25f;

    [Export]
    public float lod2Quality = 0.0f;

    [Export]
    public float lod3Quality = 0.0f;

    public override void _EnterTree()
    {
        var lod1 = GetNodeOrNull<MeshInstance>("lod1");
        var lod2 = GetNodeOrNull<MeshInstance>("lod2");
        var lod3 = GetNodeOrNull<MeshInstance>("lod3");

        if (lod1 != null)
        {
            lod1.Visible = false;
        }

        if (lod2 != null)
        {
            lod2.Visible = false;
        }

        if (lod3 != null)
        {
            lod3.Visible = false;
        }
    }


    public override void _Ready()
    {
        if (ProjectSettings.HasSetting("lod/spatial_bias"))
            lod_bias = (float)ProjectSettings.GetSetting("lod/spatial_bias");

        base._Ready();

        doLoding();

        var random = new RandomNumberGenerator();
        random.Randomize();

        timer += random.RandfRange(0, refreshRate);
    }
    private void disableLod()
    {
        var lod1 = GetNodeOrNull<MeshInstance>("lod1");
        var lod2 = GetNodeOrNull<MeshInstance>("lod2");
        var lod3 = GetNodeOrNull<MeshInstance>("lod3");

        if (lod1 != null)
        {
            lod1.Visible = true;
        }

        if (lod2 != null)
        {
            lod2.Visible = false;
        }

        if (lod3 != null)
        {
            lod3.Visible = false;
        }

        SetProcess(false);
        SetPhysicsProcess(false);
    }
    private void ServerLod()
    {
        var lod1 = GetNodeOrNull<MeshInstance>("lod1");
        var lod2 = GetNodeOrNull<MeshInstance>("lod2");
        var lod3 = GetNodeOrNull<MeshInstance>("lod3");

        if (lod1 != null)
        {
            lod1.Visible = false;
        }

        if (lod2 != null)
        {
            lod2.Visible = false;
        }

        if (lod3 != null)
        {
            lod3.Visible = true;
        }

    }
    private void activateLod()
    {

        var lod1 = GetNodeOrNull<MeshInstance>("lod1");
        var lod2 = GetNodeOrNull<MeshInstance>("lod2");
        var lod3 = GetNodeOrNull<MeshInstance>("lod3");

        var camera = GetViewport().GetCamera();
        if (camera == null)
            return;

        var distance = camera.GlobalTransform.origin.DistanceTo(GlobalTransform.origin) + lod_bias;

        if (distance < lod_1_max_distance && lod1 != null)
        {
            lod1.Visible = true;

            if (lod2 != null)
                lod2.Visible = false;

            if (lod3 != null)
                lod3.Visible = false;
        }
        else if (distance < lod_2_max_distance && lod2 != null)
        {

            if (lod1 != null)
                lod1.Visible = false;

            lod2.Visible = true;

            if (lod3 != null)
                lod3.Visible = false;
        }
        else if (distance > lod_2_max_distance && lod3 != null)
        {
            if (lod1 != null)
                lod1.Visible = false;

            if (lod2 != null)
                lod2.Visible = false;

            lod3.Visible = true;
        }
        else if (lod1 != null)
        {
            lod1.Visible = false;

            if (lod2 != null)
                lod2.Visible = false;

            if (lod3 != null)
                lod3.Visible = false;
        }
    }

    public void doLoding()
    {
        var lod1 = GetNodeOrNull<MeshInstance>("lod1");
        var lod2 = GetNodeOrNull<MeshInstance>("lod2");
        var lod3 = GetNodeOrNull<MeshInstance>("lod3");

        if (Engine.IsEditorHint())
        {
            disableLod();
        }
        else if (GetViewport().Name == "server_viewport")
        {
            ServerLod();
        }
        else
        {
            activateLod();
        }
    }

    public override void _Process(float delta)
    {
        if (!enableLoding)
        {
            var lod1 = GetNodeOrNull<MeshInstance>("lod3");
            
            if (lod1 != null)
                lod1.Visible = true;

            return;
        }

        if (timer <= refreshRate)
        {
            timer += delta;
            return;
        }

        timer = 0.0f;
        doLoding();
    }
}
