using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game
{
    public class ServerVehicle : BaseVehicle
    {


        public float prev_engine_RPM = 0.0f;


        AudioStreamSample skidSound = null;
        AudioStreamSample slideSound = null;
        AudioStreamSample tireSound = null;

        private float throttle_val = 0.0f;
        private float throttle_val_target = 0.0f;
        private float steer_val = 0.0f;
        private float brake_val = 0.0f;
        private int current_gear = 0;

        private float steer_target = 0.0f;
        private float steer_angle = 0.0f;
        private float lvl = 0.0f;
        private float prev_lvl = 0.0f;
        private float current_speed_mps = 0.0f;
        private Vector3 prev_pos = Vector3.Zero;

        private PackedScene skidScene = null;

        private float slide_player_unit_db = 0.0f;
        private float slide_player_unit_db_target = 0.0f;

        Godot.Collections.Array<AudioStreamSample> spring_sound = new Godot.Collections.Array<AudioStreamSample>();
        Godot.Collections.Array<AudioStreamSample> collision_sound = new Godot.Collections.Array<AudioStreamSample>();

        public LinkedList<FrameVehicleSnapshot> stateBuffer = new LinkedList<FrameVehicleSnapshot>();

        [Export]
        public uint InterpolationDelay = 0;

        public bool init = false;

        public int getCurrentGear()
        {
            return current_gear;
        }
        public float getKmPerHour()
        {
            return current_speed_mps * 3600.0f / 1000.0f;
        }
        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {

            base._Ready();

            skidScene = GD.Load<PackedScene>("res://utils/vehicle/Skid.tscn");

            spring_sound.Add(ResourceLoader.Load("res://vehicles/AudiRS7_2020_fbx/audio/spring_1.wav") as AudioStreamSample);
            spring_sound.Add(ResourceLoader.Load("res://vehicles/AudiRS7_2020_fbx/audio/spring_2.wav") as AudioStreamSample);
            spring_sound.Add(ResourceLoader.Load("res://vehicles/AudiRS7_2020_fbx/audio/spring_3.wav") as AudioStreamSample);
            spring_sound.Add(ResourceLoader.Load("res://vehicles/AudiRS7_2020_fbx/audio/spring_4.wav") as AudioStreamSample);

            collision_sound.Add(ResourceLoader.Load("res://vehicles/AudiRS7_2020_fbx/audio/collision.wav") as AudioStreamSample);
            slideSound = ResourceLoader.Load("res://vehicles/AudiRS7_2020_fbx/audio/body_slide.wav") as AudioStreamSample;
            tireSound = ResourceLoader.Load("res://vehicles/AudiRS7_2020_fbx/audio/tires.wav") as AudioStreamSample;
            skidSound = ResourceLoader.Load("res://vehicles/AudiRS7_2020_fbx/audio/skid.wav") as AudioStreamSample;
        }


        public void AddState(FrameVehicleSnapshot frame)
        {
            if (stateBuffer.Count > 0 && stateBuffer.Last.Value.timestamp > frame.timestamp)
            {
                return;
            }

            stateBuffer.AddLast(frame);
        }

        public Vector3 Drive(PlayerInput input, float delta)
        {
            if (engineStarted)
            {
                steer_val = steering_mult * Input.GetJoyAxis(0, (int)joy_steering);
                brake_val = brake_mult * Input.GetJoyAxis(0, (int)joy_brake);

                throttle_val_target = 0.0f;

                if (throttle_val_target < 0.0f)
                    throttle_val_target = 0.0f;

                if (brake_val < 0.0f)
                    brake_val = 0.0f;

                throttle_val_target = input.movement_direction.y;
                steer_val = input.movement_direction.x * -1;

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

                if (steerNode != null)
                {
                    var steerRot = steerNode.Rotation;
                    steerRot.z = steer_angle;
                    steerNode.Rotation = steerRot;
                }

                lvl = input.velocity.Length();
                current_speed_mps = (Translation - prev_pos).Length() / delta;

                throttle_val += (throttle_val_target - throttle_val) * 10 * delta;

                //wheels
                foreach (var wl in wheels.Values)
                {
                    
                    wl.rpm = (lvl / (wl.node.WheelRadius * Mathf.Tau)) * 300;
                }

                engine_RPM = Mathf.Clamp(((wheels["FL"].rpm + wheels["FR"].rpm)) / 2 * gear_ratio[current_gear], min_engine_RPM, max_engine_RPM);

                prev_lvl = lvl;
                prev_pos = Translation;
                prev_engine_RPM = engine_RPM;

                ShiftGears();

                EngineForce = MAX_ENGINE_FORCE / gear_ratio[current_gear] * throttle_val;
                Brake = brake_val * MAX_BRAKE_FORCE;
            }
            else
            {
                current_gear = 0;
                EngineForce = 0f;
                prev_engine_RPM = 0f;

                prev_lvl = input.velocity.Length();
                prev_pos = Translation;
            }

            return LinearVelocity;
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

                var gear_ratio_inverted = new List<float>(gear_ratio.ToList());
                gear_ratio_inverted.Reverse();

                int t = 0;
                foreach (var j in gear_ratio_inverted)
                {
                    var fl = wheels["FL"];
                    if (fl.rpm * j > min_engine_RPM)
                    {
                        appropriate_gear = t;
                        break;
                    }
                    t++;
                }

                current_gear = appropriate_gear;
            }
        }

    }
}