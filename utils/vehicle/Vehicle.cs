using Godot;
using System;
using System.Collections.Generic;

namespace Game
{
    public class Vehicle : VehicleBody
    {

        [Export] public float MAX_ENGINE_FORCE = 200f;
        [Export] public float MAX_BRAKE_FORCE = 5.0f;
        [Export] public float MAX_STEER_ANGLE = 0.5f;
        [Export] public float steer_speed = 5.0f;


        [Export] public JoystickList joy_steering = JoystickList.AnalogLx;
        [Export] public float steering_mult = -1.0f;
        [Export] public JoystickList joy_throttle = JoystickList.AnalogR2;
        [Export] public float throttle_mult = 1.0f;
        [Export] public JoystickList joy_brake = JoystickList.AnalogL2;
        [Export] public float brake_mult = 1.0f;

        [Export] public float max_engine_RPM = 5000.0f;
        [Export] public float min_engine_RPM = 1000.0f;
        public float engine_RPM = 0.0f;
        public float prev_engine_RPM = 0.0f;

        public NetworkPlayer driver { get; set; }

        private bool engineStarted = false;

        AudioStreamSample skidSound = null;
        AudioStreamSample engineSound = null;
        AudioStreamSample slideSound = null;
        AudioStreamSample tireSound = null;

        private float throttle_val = 0.0f;
        private float throttle_val_target = 0.0f;
        private float steer_val = 0.0f;
        private float brake_val = 0.0f;
        private int current_gear = 0;

        List<float> gear_ratio = new List<float>();
        Dictionary<string, wheel> wheels = new Dictionary<string, wheel>();

        private float steer_target = 0.0f;
        private float steer_angle = 0.0f;
        private float lvl = 0.0f;
        private float prev_lvl = 0.0f;
        private float current_speed_mps = 0.0f;
        private Vector3 prev_pos = Vector3.Zero;

        Godot.Collections.Array<AudioStreamSample> spring_sound = new Godot.Collections.Array<AudioStreamSample>();
        Godot.Collections.Array<AudioStreamSample> collision_sound = new Godot.Collections.Array<AudioStreamSample>();


        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {

            spring_sound.Add(ResourceLoader.Load("res://vehicles/nissan_gt-r_2008_red/audio/spring_1.wav") as AudioStreamSample);
            spring_sound.Add(ResourceLoader.Load("res://vehicles/nissan_gt-r_2008_red/audio/spring_2.wav") as AudioStreamSample);
            spring_sound.Add(ResourceLoader.Load("res://vehicles/nissan_gt-r_2008_red/audio/spring_3.wav") as AudioStreamSample);
            spring_sound.Add(ResourceLoader.Load("res://vehicles/nissan_gt-r_2008_red/audio/spring_4.wav") as AudioStreamSample);
            collision_sound.Add(ResourceLoader.Load("res://vehicles/nissan_gt-r_2008_red/audio/collision.wav") as AudioStreamSample);
            slideSound = ResourceLoader.Load("res://vehicles/nissan_gt-r_2008_red/audio/body_slide.wav") as AudioStreamSample;
            tireSound = ResourceLoader.Load("res://vehicles/nissan_gt-r_2008_red/audio/tires.wav") as AudioStreamSample;
            skidSound = ResourceLoader.Load("res://vehicles/nissan_gt-r_2008_red/audio/skid.wav") as AudioStreamSample;

            engineSound = ResourceLoader.Load("res://vehicles/nissan_gt-r_2008_red/audio/engine.wav") as AudioStreamSample;
            engineSound.LoopMode = AudioStreamSample.LoopModeEnum.Forward;
            engineSound.LoopEnd = 14985;

            gear_ratio.Add(3.380f / 1);
            gear_ratio.Add(2.000f / 1);
            gear_ratio.Add(1.325f / 1);
            gear_ratio.Add(1.000f / 1);

            InitWheel("FL");
            InitWheel("FR");
            InitWheel("BL");
            InitWheel("BR");



        }


        private void InitWheel(string name)
        {
            var fl_wheel = new wheel();
            fl_wheel.node = GetNode(name) as VehicleWheel;
            fl_wheel.spring = fl_wheel.node.GetNode("spring") as AudioStreamPlayer3D;
            fl_wheel.contact = fl_wheel.node.GetNode("contact") as AudioStreamPlayer3D;
            fl_wheel.skid = fl_wheel.node.GetNode("skid") as AudioStreamPlayer3D;
            wheels.Add(name, fl_wheel);
        }
  
        public override void _PhysicsProcess(float delta)
        {

            return;

            if (driver != null)
                ProcessInput(delta);
            else
                engineStarted = false;

            ProcessSound();
        }

        private void ProcessInput(float delta)
        {



            if (Input.IsActionJustPressed("start_engine"))
                StartEngine();

            if (engineStarted)
            {


                steer_val = steering_mult * Input.GetJoyAxis(0, (int)joy_steering);
                brake_val = brake_mult * Input.GetJoyAxis(0, (int)joy_brake);

                throttle_val_target = 0.0f;

                if (throttle_val_target < 0.0f)
                    throttle_val_target = 0.0f;

                if (brake_val < 0.0f)
                    brake_val = 0.0f;

                if (Input.IsActionPressed("move_forward") && Input.GetMouseMode() != Input.MouseMode.Visible)
                    throttle_val_target = 1.0f;
                if (Input.IsActionPressed("move_backward") && Input.GetMouseMode() != Input.MouseMode.Visible)
                    throttle_val_target = -1.0f;

                if (Input.IsActionPressed("move_left") && Input.GetMouseMode() != Input.MouseMode.Visible)
                    steer_val = 1.0f;
                else if (Input.IsActionPressed("move_right") && Input.GetMouseMode() != Input.MouseMode.Visible)
                    steer_val = -1.0f;

                steer_target = steer_val * MAX_STEER_ANGLE;


                if (steer_target < steer_angle)
                {
                    steer_angle -= steer_speed * delta;

                    if (steer_target > steer_angle)
                        steer_angle = steer_target;
                }

                else if (steer_target > steer_angle)
                {
                    steer_angle += steer_speed * delta;

                    if (steer_target < steer_angle)
                        steer_angle = steer_target;
                }

                Steering = steer_angle;

                lvl = LinearVelocity.Length();
                current_speed_mps = (Translation - prev_pos).Length() / delta;
                throttle_val += (throttle_val_target - throttle_val) * 10 * delta;
                var rand = new RandomNumberGenerator();
                //wheels
                foreach (var wl in wheels.Values)
                {
                    if (wl.node.IsInContact())
                    {
                        wl.trans = wl.node.Translation.y;
                        if ((wl.trans - wl.prev_trans) > 0.02)
                        {
                            wl.spring.Stream = spring_sound[rand.RandiRange(0, spring_sound.Count - 1)];
                            wl.spring.Play();

                            rand.Randomize();
                        }

                        wl.contact.UnitSize = lvl / 50;
                        if (!wl.contact.Playing)
                        {
                            wl.contact.Stream = tireSound;
                            wl.contact.Play();
                        }
                        wl.prev_trans = wl.trans;
                    }
                    else
                        wl.contact.Stop();

                    if (wl.node.GetSkidinfo() < 0.05)
                    {
                        if (!wl.skid.Playing)
                        {
                            wl.skid.Stream = skidSound;
                            wl.skid.Play();
                        }
                    }

                    float skid_unit_size = 0.0f;
                    float skid_unit_size_target = (1 - wl.node.GetSkidinfo()) * 5;
                    skid_unit_size += (skid_unit_size_target - skid_unit_size) * 0.25f;
                    wl.skid.UnitSize = skid_unit_size;

                    if (wl.node.IsInContact() && wl.node.GetSkidinfo() < 0.15)
                    {
                        //  var skid = skid_scn.instance();

                        // main_scn.add_child(skid);

                        //   skid.global_transform.origin = wl.node.GlobalTransform.origin + new Vector3(0, -wl.node.WheelRadius + 0.15f, 0);
                    }

                    wl.rpm = (lvl / (wl.node.WheelRadius * Mathf.Tau)) * 300;
                }

                engine_RPM = Mathf.Clamp(((wheels["FL"].rpm + wheels["FR"].rpm)) / 2 * gear_ratio[current_gear], min_engine_RPM, max_engine_RPM);

                prev_lvl = lvl;
                prev_pos = Translation;
                prev_engine_RPM = engine_RPM;

                // EngineForce = MAX_ENGINE_FORCE / gear_ratio[current_gear] * throttle_val;
                EngineForce = MAX_ENGINE_FORCE * throttle_val;
                Brake = brake_val * MAX_BRAKE_FORCE;


                ShiftGears();
            }
            else
            {
                current_gear = 0;
                EngineForce = 0f;
            }
        }

        private void ShiftGears()
        {
            int appropriate_gear = 0;

            if (engine_RPM >= max_engine_RPM)
            {
                appropriate_gear = current_gear;

                int d = 0;
                foreach (var i in gear_ratio)
                {
                    var fl = wheels["FL"];
                    if (fl.rpm * i < max_engine_RPM)
                    {
                        appropriate_gear = d;
                        break;
                    }
                    d++;
                }

                current_gear = appropriate_gear;
            }

            if (engine_RPM <= min_engine_RPM)
            {
                appropriate_gear = current_gear;

                var gear_ratio_inverted = gear_ratio;
                gear_ratio_inverted.Reverse();

                int t = 0;
                foreach (var i in gear_ratio_inverted)
                {
                    var fl = wheels["FL"];
                    if (fl.rpm * t > min_engine_RPM)
                    {
                        appropriate_gear = t;
                        break;
                    }
                    t++;
                }

                current_gear = appropriate_gear;
            }
        }

        private void ProcessSound()
        {
            var enginePlayer = GetNode("audio").GetNode("engine") as AudioStreamPlayer3D;

            if (engineStarted)
            {
                if (!enginePlayer.Playing)
                {
                    enginePlayer.Stream = engineSound;
                    enginePlayer.Play();
                }

                enginePlayer.PitchScale = Mathf.Clamp(Mathf.Abs(engine_RPM / 1000f) * 1.0f, 1.0f, 5.0f);
            }
            else
            {
                if (enginePlayer.Playing)
                {
                    enginePlayer.Stop();
                }
            }
        }
        private void StartEngine()
        {
            if (!engineStarted)
            {
                engineStarted = true;
            }
            else
            {
                engineStarted = false;

            }
        }
    }
}