using System;
using Godot;
using System.Collections.Generic;
namespace Game
{

    [Tool]
    public class Ocean : ImmediateGeometry
    {

        const int NUMBER_OF_WAVES = 10;


        private float _speed = 10.0f;
        private float _n_max = 1.0f;
        private bool _noise_enabled = true;
        private float _noise_amplitude = 0.28f;
        private float _noise_frequency = 0.065f;
        private float _noise_speed = 0.48f;
        private ulong _seed_value = 0;

        private Godot.Collections.Array<float> _wave_directions = new Godot.Collections.Array<float>();
        private Godot.Collections.Array<Vector3> _waves = new Godot.Collections.Array<Vector3>();

        [Export]
        float speed
        {
            get
            {
                return _speed;
            }
            set
            {
                setSpeed(value);
            }
        }
        [Export]
        Godot.Collections.Array<float> wave_directions
        {
            get
            {
                return _wave_directions;
            }
            set
            {
                setWaveDirection(value);
            }
        }
        [Export]
        Godot.Collections.Array<Vector3> waves
        {
            get
            {
                return _waves;
            }
            set
            {
                setWaves(value);
            }
        }

        [Export]
        float n_max
        {
            get
            {
                return _n_max;
            }
            set
            {
                setNMax(value);
            }
        }

        [Export]
        bool noise_enabled
        {
            get
            {
                return _noise_enabled;
            }
            set
            {
                setNoiseEnabled(value);
            }
        }

        [Export]
        float noise_amplitude
        {
            get
            {
                return _noise_amplitude;
            }
            set
            {
                setNoiseAmplitude(value);
            }
        }

        [Export]
        float noise_frequency
        {
            get
            {
                return _noise_frequency;
            }
            set
            {
                setNoiseFrequency(value);
            }
        }

        [Export]
        float noise_speed
        {
            get
            {
                return _noise_speed;
            }
            set
            {
                setNoiseSpeed(value);
            }
        }

        [Export]
        ulong seed_value
        {
            get
            {
                return _seed_value;
            }
            set
            {
                setSeed(value);
            }
        }

        private bool initialized = false;

        private float counter = 0.5f;


        private ImageTexture waves_in_tex = new ImageTexture();

        private CubeCamera cube_cam = null;

        private float res = 100.0f;

        public override void _Ready()
        {

            for (int j = 0; j < res; j++)
            {
                var y = j / res - 0.5f;
                var n_y = (j + 1) / res - 0.5f;

                Begin(Mesh.PrimitiveType.TriangleStrip);
                for (int i = 0; i < res; i++)
                {
                    var x = i / res - 0.5f;

                    AddVertex(new Vector3(x * 2, 0, -y * 2));
                    AddVertex(new Vector3(x * 2, 0, -n_y * 2));
                }

                End();
            }

            Begin(Mesh.PrimitiveType.Points);
            AddVertex(-(new Vector3(1, 1, 1)) * Mathf.Pow(2, 32));
            AddVertex((new Vector3(1, 1, 1)) * Mathf.Pow(2, 32));
            End();

            cube_cam = GetNode("cube_cam") as CubeCamera;

            (MaterialOverride as ShaderMaterial).SetShaderParam("resolution", res);
            update_waves();
        }

        public override void _Process(float delta)
        {
            counter -= delta;
            if (counter <= 0)
            {
                if (cube_cam != null)
                {
                    var cube_map = cube_cam.UpdateMap();

                    var obj = (MaterialOverride as ShaderMaterial).GetShaderParam("environment");
                    (MaterialOverride as ShaderMaterial).SetShaderParam("environment", cube_map);
                }
                counter = Mathf.Inf;
            }

            (MaterialOverride as ShaderMaterial).SetShaderParam("time_offset", OS.GetTicksMsec() / 1000.0f * speed);
            initialized = true;
        }

        private void setSeed(ulong value)
        {
            _seed_value = value;
            if (initialized)
                update_waves();
        }

        private void setSpeed(float value)
        {

            _speed = value;
            (MaterialOverride as ShaderMaterial).SetShaderParam("speed", value);
        }

        private void setNoiseEnabled(bool value)
        {
            _noise_enabled = value;
            Godot.Plane old_noise_params = (Godot.Plane)(MaterialOverride as ShaderMaterial).GetShaderParam("noise_params");

            old_noise_params.D = value ? 1 : 0;
            (MaterialOverride as ShaderMaterial).SetShaderParam("noise_params", old_noise_params);
        }

        private void setNoiseAmplitude(float value)
        {
            _noise_amplitude = value;
            Godot.Plane old_noise_params = (Godot.Plane)(MaterialOverride as ShaderMaterial).GetShaderParam("noise_params");
            old_noise_params.x = value;
            (MaterialOverride as ShaderMaterial).SetShaderParam("noise_params", old_noise_params);
        }

        private void setNoiseFrequency(float value)
        {
            _noise_frequency = value;
            Godot.Plane old_noise_params = (Godot.Plane)(MaterialOverride as ShaderMaterial).GetShaderParam("noise_params");
            old_noise_params.y = value;
            (MaterialOverride as ShaderMaterial).SetShaderParam("noise_params", old_noise_params);
        }

        private void setNoiseSpeed(float value)
        {
            _noise_speed = value;
            Godot.Plane old_noise_params = (Godot.Plane)(MaterialOverride as ShaderMaterial).GetShaderParam("noise_params");
            old_noise_params.z = value;
            (MaterialOverride as ShaderMaterial).SetShaderParam("noise_params", old_noise_params);
        }


        private RandomNumberGenerator generator = new RandomNumberGenerator();


        private void setNMax(float new_n_max)
        {
            _n_max = new_n_max;
            (MaterialOverride as ShaderMaterial).SetShaderParam("n_max", new_n_max);

        }

        private void setWaves(Godot.Collections.Array<Vector3> new_waves)
        {
            _waves = new_waves;
            if (waves.Count != wave_directions.Count)
                GD.PrintErr("new waves size not equal to wave directions size");
            else
                update_waves();
        }
        private void setWaveDirection(Godot.Collections.Array<float> new_wave_directions)
        {
            _wave_directions = new_wave_directions;
            if (waves.Count != wave_directions.Count)
                GD.PrintErr("new wave directions size not equal to waves size");
            else
                update_waves();
        }


        private void update_waves()
        {
            generator.Seed = seed_value;
            waves_in_tex = new ImageTexture();

            var img = new Image();


            img.Create(5, NUMBER_OF_WAVES, false, Image.Format.Rf);
            img.Lock();


            for (int i = 0; i < NUMBER_OF_WAVES; i++)
            {
                var w = waves[i];

                var _wind_direction = (new Vector2(1.0f, 1.0f)).Rotated(Mathf.Deg2Rad(wave_directions[i]));

                img.SetPixel(0, i, new Color(w[0] / 100.0f, 0, 0, 0));
                img.SetPixel(1, i, new Color(w[1] / 100.0f, 0, 0, 0));
                img.SetPixel(2, i, new Color(_wind_direction.x, 0, 0, 0));
                img.SetPixel(3, i, new Color(_wind_direction.y, 0, 0, 0));
                img.SetPixel(4, i, new Color((Mathf.Tau) / w[2], 0, 0, 0));

            }

            img.Unlock();

            waves_in_tex.CreateFromImage(img, 0);
            (MaterialOverride as ShaderMaterial).SetShaderParam("waves", waves_in_tex);
        }

    }
}