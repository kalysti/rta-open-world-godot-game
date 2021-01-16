using Godot;
using System;

public class GodRays : Spatial
{
    public Light light;

    [Export(PropertyHint.Range, "0,2")]
    public float exposure = 0.5f;

    [Export(PropertyHint.Range, "EASE")]
    public float attenuation = 2.0f;

    [Export(PropertyHint.Range, "0,2")]
    public float light_size = 0.5f;

    private Texture clouds;

    private ShaderMaterial material = null;

    private MeshInstance canvas = null;
    //var material := preload("GodRays.tres").duplicate() as ShaderMaterial

    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        material = GD.Load<ShaderMaterial>("res://utils/world/sky/GodRays.tres");

        if (GetChildCount() > 0 && GetChild(0).Owner == null)
            RemoveChild(GetChild(0));

        var mesh = new QuadMesh();

        mesh.Size = new Vector2(2, 2);

        mesh.CustomAabb = new AABB(new Vector3(1, 1, 1) * -300000, new Vector3(1, 1, 1) * 600000);


        canvas = new MeshInstance();

        canvas.Name = "GodRay";
        canvas.Mesh = mesh;
        canvas.MaterialOverride = material;

        AddChild(canvas);

        material.SetupLocalToScene();

        set_exposure(exposure);
        set_attenuation(attenuation);
        set_light_size(light_size);
        set_clouds(null);
    }

    public override void _Notification(int what)
    {
        if (what == NotificationParented)
        {
            if (GetParent() is Light)
                light = GetParent() as Light;
        }

        else if (what == NotificationUnparented)
        {
            light = null;
            set_clouds(null);
        }
    }

    public override void _Process(float delta)
    {

        if (light == null)
        {
            material.SetShaderParam("light_type", 0);
            material.SetShaderParam("light_pos", new Vector3());
            return;
        }

        bool is_directional = (light is DirectionalLight);

        material.SetShaderParam("light_type", !is_directional);
        material.SetShaderParam("light_color", light.LightColor * light.LightEnergy);


        if (is_directional)
        {
            var direction = light.GlobalTransform.basis.z;
            material.SetShaderParam("light_pos", direction);
            material.SetShaderParam("size", light_size);
        }
        else
        {
            var position = light.GlobalTransform.origin;
            material.SetShaderParam("light_pos", position);
            material.SetShaderParam("size", light_size * (light as OmniLight).OmniRange);
        }

        material.SetShaderParam("num_samples", ProjectSettings.GetSetting("rendering/quality/godrays/sample_number"));
        material.SetShaderParam("use_pcf5", ProjectSettings.GetSetting("rendering/quality/godrays/use_pcf5"));
        material.SetShaderParam("dither", ProjectSettings.GetSetting("rendering/quality/godrays/dither_amount"));
    }


    public void set_exposure(float value)
    {
        exposure = value;
        material.SetShaderParam("exposure", exposure);
        if (canvas != null)
            canvas.Visible = exposure != 0;
    }

    public void set_attenuation(float value)
    {
        attenuation = value;
        material.SetShaderParam("attenuate", attenuation);
        if (canvas != null)
            canvas.Visible = attenuation != 0;
    }

    public void set_light_size(float value)
    {
        light_size = value;
        if (canvas != null)
            canvas.Visible = light_size != 0;
    }

    public void set_clouds(Texture value)
    {
        clouds = value;
        material.SetShaderParam("clouds", value);
        material.SetShaderParam("use_clouds", value != null);
    }


    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
