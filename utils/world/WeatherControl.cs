using Godot;
using System;
public class WeatherControl : Spatial
{
    [Export]
    public int clouds_resolution = 1024;

    [Export]
    public int sky_resolution = 2048;

    [Export]
    public GradientTexture sky_gradient_texture = null;

    [Export]
    public bool SCATERRING = false;
    //sky

    [Export(PropertyHint.ColorNoAlpha)]
    public Color color_sky = new Color(0.25f, 0.5f, 1.0f, 1.0f);

    [Export(PropertyHint.Range, "0.0,10.0,0.0001")]
    public float sky_tone = 3.0f;

    [Export(PropertyHint.Range, "0.0,2.0,0.0001")]
    public float sky_density = 0.75f;

    [Export(PropertyHint.Range, "0.0,10.0,0.0001")]
    public float sky_rayleig_coeff = 0.75f;

    [Export(PropertyHint.Range, "0.0,10.0,0.0001")]
    public float sky_mie_coeff = 2.0f;

    [Export(PropertyHint.Range, "0.0,2.0,0.0001")]
    public float multiScatterPhase = 0.0f;

    [Export(PropertyHint.Range, "-2.0,2.0,0.0001")]
    public float anisotropicIntensity = 0.0f;

    //clouds


    [Export(PropertyHint.Range, "0.0,1.0,0.0001")]
    public float clouds_coverage = 0.5f;


    [Export(PropertyHint.Range, "0.0,10.0,0.000001")]
    public float clouds_size = 2.0f;


    [Export(PropertyHint.Range, "0.0,10.0,0.0001")]
    public float clouds_softness = 1.0f;


    [Export(PropertyHint.Range, "0.0,1.0,0.0001")]
    public float clouds_dens = 0.07f;


    [Export(PropertyHint.Range, "0.0,1.0,0.0001")]
    public float clouds_height = 0.35f;


    [Export(PropertyHint.Range, "1,100")]
    public int clouds_quality = 25;


    //time of day
    [Export(PropertyHint.Range, "0.0,1.0,0.000001")]
    public float time_of_day_setup = 0.0f;


    public int _hours = 0;
    public int _minutes = 0;
    public int _seconds = 0;


    [Export(PropertyHint.Range, "0,23")]
    public int hours
    {
        get
        {
            return _hours;
        }
        set
        {
            setHours(value);
        }
    }

    [Export(PropertyHint.Range, "0,59")]
    public int minutes
    {
        get
        {
            return _minutes;
        }
        set
        {
            setSeconds(value);
        }
    }

    [Export(PropertyHint.Range, "0,59")]
    public int seconds
    {
        get
        {
            return _seconds;
        }
        set
        {
            setSeconds(value);
        }
    }

    //light

    [Export(PropertyHint.ColorNoAlpha)]
    public Color moon_light = new Color(0.6f, 0.6f, 0.8f, 1.0f);

    [Export(PropertyHint.ColorNoAlpha)]
    public Color sunset_light = new Color(1.0f, 0.7f, 0.55f, 1.0f);

    [Export(PropertyHint.ColorNoAlpha)]
    public Color day_light = new Color(1.0f, 1.0f, 1.0f, 1.0f);


    [Export(PropertyHint.ColorNoAlpha)]
    public Color moon_tint = new Color(1.0f, 0.7f, 0.35f, 1.0f);

    [Export(PropertyHint.ColorNoAlpha)]
    public Color clouds_tint = new Color(1.0f, 1.0f, 1.0f, 1.0f);


    //time of day
    [Export(PropertyHint.Range, "-0.3,0.3,0.000001")]
    public float sunset_offset = -0.1f;

    [Export(PropertyHint.Range, "0.0,0.3,0.000001")]
    public float sunset_range = 0.2f;

    //radius

    [Export(PropertyHint.Range, "0.0,1.0,0.0001")]
    public float sun_radius = 0.04f;

    [Export(PropertyHint.Range, "0.0,0.5,0.0001")]
    public float moon_radius = 0.1f;

    [Export(PropertyHint.Range, "-1.0,1.0,0.0001")]
    public float moon_phase = 0.0f;

    [Export]
    public float night_level_light = 0.05f;

    [Export]
    public Vector2 wind_dir = new Vector2(1.0f, 0.0f);

    [Export(PropertyHint.Range, "0.0,1.0,0.0001")]
    public float wind_strength = 0.1f;

    [Export]
    public Vector3 lighting_pos = new Vector3(0.0f, 1.0f, 1.0f);


    private Viewport sky_view = null;
    private Viewport clouds_view = null;
    private Sprite clouds_tex = null;
    private Sprite sky_tex = null;

    private Godot.Environment env = null;

    private float one_second = 1.0f / (24.0f * 60.0f * 60.0f);

    private float time_of_day = 0.0f;
    private Vector3 sun_pos = Vector3.Zero;
    private Vector3 moon_pos = Vector3.Zero;

    private DirectionalLight sun = null;
    private GodRays god_rays = null;
    private float iTime = 0f;

    /*




    var sun_pos: Vector3
    var moon_pos: Vector3
    var god_rays
    var iTime: float=0.0

    var time_of_day: float=0.0
    var one_second: float = 1.0/(24.0*60.0*60.0)#What part of a second takes in a day in the range from 0 to 1

    */

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        sun = GetNode("Sun_Moon") as DirectionalLight;
        sky_view = GetNode("sky_viewport") as Viewport;
        clouds_view = GetNode("clouds_viewport") as Viewport;
        clouds_tex = GetNode("clouds_viewport/clouds_texture") as Sprite;
        sky_tex = GetNode("sky_viewport/sky_texture") as Sprite;
        env = (GetNode("WorldEnvironment") as WorldEnvironment).Environment;

       // god_rays = GetNode("GodRays") as GodRays;
        return;
       // _set_god_rays(true);  
        set_lighting_strike(false);
        CallDeferred("_set_attenuation", 3.0);
        CallDeferred("_set_exposure", 1.0);
        CallDeferred("_set_light_size", 0.2);
        CallDeferred("set_color_sky", color_sky);
        CallDeferred("set_moon_tint", moon_tint);
        CallDeferred("set_clouds_tint", clouds_tint);
        CallDeferred("set_moon_phase", moon_phase);
        CallDeferred("set_moon_radius", moon_radius);
        CallDeferred("set_wind_strength", wind_strength);
        CallDeferred("set_wind_strength", wind_strength);
        CallDeferred("set_clouds_quality", clouds_quality);
        CallDeferred("set_clouds_height", clouds_height);
        CallDeferred("set_clouds_coverage", clouds_coverage);
        CallDeferred("set_time");
        CallDeferred("reflections_update");

        AddChild(RefreshTimer);
        RefreshTimer.Connect("timeout", this, "reflections_update");
        RefreshTimer.WaitTime = 4;
        RefreshTimer.Start();
    }

    private Timer RefreshTimer = new Timer();
    public override void _Process(float delta)
    {
        return;
        iTime += delta;

        var lighting_strength = Mathf.Clamp(Mathf.Sin(iTime * 20.0f), 0.0f, 1.0f);

        lighting_pos = lighting_pos.Normalized();

        sun.LightColor = day_light;
        sun.LightEnergy = lighting_strength * 2;

        //CallDeferred("set_call_deff_shader_params", sky_tex.Material, "shader_param/LIGHTTING_POS", lighting_pos);
       // CallDeferred("set_call_deff_shader_params", sky_tex.Material, "shader_param/LIGHTING_STRENGTH", new Vector3(lighting_strength, lighting_strength, lighting_strength));

        sun.LookAtFromPosition(lighting_pos, Vector3.Zero, Vector3.Up);
    }


    private void set_call_deff_shader_params(Material node, string param, object value)
    {
        node.Set(param, value);
    }

    private void set_clouds_resolution(int value)
    {
        clouds_resolution = value;
        if (IsInsideTree())
        {
            clouds_view.Size = new Vector2(clouds_resolution, clouds_resolution);
            (clouds_tex.Texture as ImageTexture).SetSizeOverride(new Vector2(clouds_resolution, clouds_resolution));
        }
    }

    private void set_sky_resolution(int value)
    {
        sky_resolution = value;
        if (IsInsideTree())
        {
            sky_view.Size = new Vector2(sky_resolution, sky_resolution);
            (sky_tex.Texture as ImageTexture).SetSizeOverride(new Vector2(sky_resolution, sky_resolution));
        }
    }


    private void reflections_update()
    {
        //(env.BackgroundSky as PanoramaSky).Panorama = sky_view.GetTexture();
    }

    private void set_SCATERRING(bool value)
    {
        SCATERRING = value;

       // if (IsInsideTree())
          //  CallDeferred("set_call_deff_shader_params", sky_tex.Material, "shader_param/SCATERRING", SCATERRING);
    }

    private void set_sky_gradient(GradientTexture value)
    {
        sky_gradient_texture = value;
       // if (IsInsideTree())
       //     CallDeferred("set_call_deff_shader_params", sky_tex.Material, "shader_param/sky_gradient_texture", sky_gradient_texture);
    }


    private void set_night_level_light(float value)
    {

        night_level_light = Mathf.Clamp(value, 0.0f, 1.0f);
        set_time();
    }

    private void setHours(int value)
    {

        _hours = Mathf.Clamp(value, 0, 23);
        set_time_of_day((_hours * 3600 + _minutes * 60 + _seconds) * one_second);
    }

    private void setMinutes(int value)
    {
        _minutes = Mathf.Clamp(value, 0, 59);
        set_time_of_day((_hours * 3600 + _minutes * 60 + _seconds) * one_second);
    }

    private void setSeconds(int value)
    {
        _seconds = Mathf.Clamp(value, 0, 59);
        set_time_of_day((_hours * 3600 + _minutes * 60 + _seconds) * one_second);
    }


    private void set_time_of_day(float value)
    {
        time_of_day_setup = value;
        var time = value / one_second;
        value -= 2.0f / 24.0f;

        if (value < 0.0)
            value = 1.0f + value;

        time_of_day = value;
        _hours = (int)Mathf.Clamp(time / 3600.0f, 0.0f, 23.0f);
        time -= _hours * 3600;

        _minutes = (int)Mathf.Clamp(time / 60f, 0.0f, 59.0f);
        time -= _minutes * 60;

        _seconds = (int)Mathf.Clamp(time, 0.0f, 59.0f);
        set_time();
    }

    private void set_time()
    {
        if (!IsInsideTree())
            return;

        Color light_color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        float phi = time_of_day * 2.0f * Mathf.Pi;
        sun_pos = new Vector3(0.0f, -1.0f, 0.0f).Normalized().Rotated(new Vector3(1.0f, 0.0f, 0.0f).Normalized(), phi);
        moon_pos = new Vector3(0.0f, 1.0f, 0.0f).Normalized().Rotated(new Vector3(1.0f, 0.0f, 0.0f).Normalized(), phi);
        Vector3 moon_tex_pos = new Vector3(0.0f, 1.0f, 0.0f).Normalized().Rotated(new Vector3(1.0f, 0.0f, 0.0f).Normalized(), (phi + Mathf.Pi) * 0.5f);
       // CallDeferred("set_call_deff_shader_params", sky_tex.Material, "shader_param/MOON_TEX_POS", moon_tex_pos);

        float light_energy = Mathf.SmoothStep(sunset_offset, 0.4f, sun_pos.y);

      //  CallDeferred("set_call_deff_shader_params", sky_tex.Material, "shader_param/SUN_POS", sun_pos);
       // CallDeferred("set_call_deff_shader_params", sky_tex.Material, "shader_param/MOON_POS", moon_pos);
       // CallDeferred("set_call_deff_shader_params", clouds_tex.Material, "shader_param/SUN_POS", -sun_pos);
     //   CallDeferred("set_call_deff_shader_params", sky_tex.Material, "shader_param/attenuation", Mathf.Clamp(light_energy, night_level_light * 0.25f, 1.00f));

        light_energy = Mathf.Clamp(light_energy, night_level_light, 1.00f);
        float sun_height = sun_pos.y - sunset_offset;

        if (sun_height < sunset_range)
            light_color = moon_light.LinearInterpolate(sunset_light, Mathf.Clamp(sun_height / sunset_range, 0.0f, 1.0f));
        else
            light_color = sunset_light.LinearInterpolate(day_light, Mathf.Clamp((sun_height - sunset_range) / sunset_range, 0.0f, 1.0f));

        if (sun_pos.y < 0.0)
        {
            if (moon_pos.Normalized() != Vector3.Up)
                sun.LookAtFromPosition(moon_pos, Vector3.Zero, Vector3.Up);
        }
        else if (sun_pos.Normalized() != Vector3.Up)
            sun.LookAtFromPosition(sun_pos, Vector3.Zero, Vector3.Up);

        set_clouds_tint(light_color);

        light_energy = light_energy * (1f - clouds_coverage * 0.5f);

        sun.LightEnergy = light_energy;

        sun.LightColor = light_color;

        env.AmbientLightEnergy = light_energy;

        env.AmbientLightColor = light_color;

        env.AdjustmentSaturation = 1f - clouds_coverage * 0.5f;

        env.FogColor = light_color;
    }

    private void set_clouds_height(float value)
    {

        clouds_height = Mathf.Clamp(value, 0.0f, 1.0f);
     //   if (IsInsideTree())
     //       CallDeferred("set_call_deff_shader_params", clouds_tex.Material, "shader_param/HEIGHT", clouds_height);
    }

    private void set_clouds_coverage(float value)
    {
        clouds_coverage = Mathf.Clamp(value, 0.0f, 1.0f);
        if (IsInsideTree())
        {
          //  CallDeferred("set_call_deff_shader_params", clouds_tex.Material, "shader_param/ABSORPTION", clouds_coverage + 0.75);
         //   CallDeferred("set_call_deff_shader_params", clouds_tex.Material, "shader_param/COVERAGE", 1.0 - (clouds_coverage * 0.7 + 0.1));
          //  CallDeferred("set_call_deff_shader_params", clouds_tex.Material, "shader_param/THICKNESS", clouds_coverage * 10.0 + 10.0);
          //  CallDeferred("set_time");
        }

    }
    private void set_clouds_size(float value)
    {
        clouds_size = Mathf.Clamp(value, 0.0f, 10.0f);
       // if (IsInsideTree())
        //    CallDeferred("set_call_deff_shader_params", clouds_tex.Material, "shader_param/SIZE", clouds_size);

    }

    private void set_clouds_softness(float value)
    {
        clouds_softness = Mathf.Clamp(value, 0.0f, 10.0f);
     //   if (IsInsideTree())
       //     CallDeferred("set_call_deff_shader_params", clouds_tex.Material, "shader_param/SOFTNESS", clouds_softness);
    }
    private void set_clouds_dens(float value)
    {
        clouds_dens = Mathf.Clamp(value, 0.0f, 1.0f);
      //  if (IsInsideTree())
      //      CallDeferred("set_call_deff_shader_params", clouds_tex.Material, "shader_param/DENS", clouds_dens);
    }

    private void set_clouds_quality(int value)
    {
        clouds_quality = value;
        //if (IsInsideTree())
        //    CallDeferred("set_call_deff_shader_params", clouds_tex.Material, "shader_param/STEPS", Mathf.Clamp(clouds_quality, 5, 100));
    }


    private void set_wind_dir(Vector2 value)
    {
        wind_dir = value.Normalized();
        set_wind_strength(wind_strength);
    }
    private void set_wind_strength(float value)
    {
        wind_strength = value;
       // if (IsInsideTree())
        //    CallDeferred("set_call_deff_shader_params", clouds_tex.Material, "shader_param/WIND", new Vector3(wind_dir.x, 0.0f, wind_dir.y) * wind_strength);
    }
    private void set_sun_radius(float value)
    {
        sun_radius = Mathf.Clamp(value, 0.0f, 1.0f);
       // if (IsInsideTree())
        //    CallDeferred("set_call_deff_shader_params", sky_tex.Material, "shader_param/sun_radius", value);
    }
    private void set_moon_radius(float value)
    {
        moon_radius = Mathf.Clamp(value, 0.0f, 1.0f);
       // if (IsInsideTree())
       //     CallDeferred("set_call_deff_shader_params", sky_tex.Material, "shader_param/moon_radius", value);
    }
    private void set_moon_phase(float value)
    {
        moon_phase = Mathf.Clamp(value, -1.0f, 1.0f);
       // if (IsInsideTree())
         //   CallDeferred("set_call_deff_shader_params", sky_tex.Material, "shader_param/MOON_PHASE", moon_phase);
    }
    private void set_sky_tone(float value)
    {
        sky_tone = value;
       // if (IsInsideTree())
        //    CallDeferred("set_call_deff_shader_params", sky_tex.Material, "shader_param/sky_tone", sky_tone);
    }
    private void set_sky_density(float value)
    {
        sky_density = value;
       // if (IsInsideTree())
         //   CallDeferred("set_call_deff_shader_params", sky_tex.Material, "shader_param/sky_density", sky_density);
    }
    private void set_sky_rayleig_coeff(float value)
    {
        sky_rayleig_coeff = value;
      //  if (IsInsideTree())
        //    CallDeferred("set_call_deff_shader_params", sky_tex.Material, "shader_param/sky_rayleig_coeff", sky_rayleig_coeff);
    }
    private void set_sky_mie_coeff(float value)
    {
        sky_mie_coeff = value;
       // if (IsInsideTree())
        //    CallDeferred("set_call_deff_shader_params", sky_tex.Material, "shader_param/sky_mie_coeff", sky_mie_coeff);
    }
    private void set_multiScatterPhase(float value)
    {
        multiScatterPhase = value;
      //  if (IsInsideTree())
       //     CallDeferred("set_call_deff_shader_params", sky_tex.Material, "shader_param/multiScatterPhase", multiScatterPhase);
    }
    private void set_anisotropicIntensity(float value)
    {
        anisotropicIntensity = value;
     //   if (IsInsideTree())
       //     CallDeferred("set_call_deff_shader_params", sky_tex.Material, "shader_param/anisotropicIntensity", anisotropicIntensity);
    }
    private void set_color_sky(Color value)
    {
        color_sky = value;
       // if (IsInsideTree())
        //    CallDeferred("set_call_deff_shader_params", sky_tex.Material, "shader_param/color_sky", color_sky);
    }
    private void set_moon_tint(Color value)
    {
        moon_tint = value;
        //if (IsInsideTree())
         //   CallDeferred("set_call_deff_shader_params", sky_tex.Material, "shader_param/moon_tint", moon_tint);
    }
    private void set_clouds_tint(Color value)
    {
        clouds_tint = value;
       // if (IsInsideTree())
         //   CallDeferred("set_call_deff_shader_params", sky_tex.Material, "shader_param/clouds_tint", clouds_tint);
    }



    private void set_lighting_strike(bool on)
    {
        if (on)
        {
         //   _set_god_rays(false);
            SetProcess(true);
        }
        else
        {
           // _set_god_rays(true);
            SetProcess(false);
            iTime = 0.0f;

           // if (sky_tex != null)
              //  CallDeferred("set_call_deff_shader_params", sky_tex.Material, "shader_param/LIGHTING_STRENGTH", new Vector3(0.0f, 0.0f, 0.0f));

            set_time();
        }
    }
    private void set_lighting_pos(Vector3 value)
    {
        lighting_pos = value;
       // if (IsInsideTree())
          //  CallDeferred("set_call_deff_shader_params", sky_tex.Material, "shader_param/LIGHTTING_POS", lighting_pos.Normalized());
    }

    private async void thunder()
    {
        AudioStreamPlayer thunder_sound = GetNode("thunder") as AudioStreamPlayer;

        if (thunder_sound.Playing)
            return;

        thunder_sound.Play();

        await ToSignal(GetTree().CreateTimer(0.3f), "timeout");
        set_lighting_strike(true);


        await ToSignal(GetTree().CreateTimer(0.8f), "timeout");
        set_lighting_strike(false);
    }
    private void _set_exposure(float value)
    {
      //  if (god_rays == null)
      //      god_rays.set_exposure(value);
    }
    private void _set_attenuation(float value)
    {
      //  if (god_rays == null)
       //     god_rays.set_attenuation(value);


    }
    private void _set_light_size(float value)
    {
      //  if (god_rays == null)
        //    god_rays.set_light_size(value);
    }
    private void _set_god_rays(bool on)
    {
        if (god_rays == null)
            return;

        if (on)
        {
            if (!god_rays.IsInsideTree())
                AddChild(god_rays);

            god_rays.light = sun;
            god_rays.set_clouds(clouds_view.GetTexture());
        }
        else
            RemoveChild(god_rays);
    }
}
