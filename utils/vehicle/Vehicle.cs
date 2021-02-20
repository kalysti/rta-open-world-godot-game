using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game
{
    public class Vehicle : BaseVehicle
    {



        AudioStreamSample skidSound = null;
        AudioStreamSample slideSound = null;
        AudioStreamSample tireSound = null;

        private float throttle_val = 0.0f;
        private float throttle_val_target = 0.0f;
        private float steer_val = 0.0f;
        private float brake_val = 0.0f;
        private int current_gear = 0;


        private Vector3 prev_pos = Vector3.Zero;

        private PackedScene skidScene = null;

        private float slide_player_unit_db = 0.0f;
        private float slide_player_unit_db_target = 0.0f;

        Godot.Collections.Array<AudioStreamSample> spring_sound = new Godot.Collections.Array<AudioStreamSample>();
        Godot.Collections.Array<AudioStreamSample> collision_sound = new Godot.Collections.Array<AudioStreamSample>();

        public LinkedList<FrameVehicleSnapshot> stateBuffer = new LinkedList<FrameVehicleSnapshot>();

        [Export]
        public uint InterpolationDelay = 0;

        public uint clientTick = 0;

        public bool init = false;

        public int getCurrentGear()
        {
            return current_gear;
        }
        public float getKmPerHour()
        {
            return getSpeed() * 3600.0f / 1000.0f;
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

        protected void setSmokeSpeed()
        {
            var speed = Mathf.Clamp(Mathf.Abs(engine_RPM / 1000f) * 1.0f, 0.5f, 5.0f);

            if (engineSmoke != null)
            {
                foreach (var item in engineSmoke.GetChildren())
                {
                    if (item is Particles)
                    {
                        var part = item as Particles;
                        part.SpeedScale = speed;
                    }
                }
            }

        }


        [Puppet]
        public void PosUpdate(uint timetamp, Vector3 pos, Vector3 rot)
        {
            GD.Print("Init Vehicle on Client");

            SetVehiclePosition(pos);
            SetVehicleRotation(rot);

            AddState(new FrameVehicleSnapshot
            {
                origin = pos,
                rotation = rot,
                timestamp = 0
            });

            init = true;
        }


        public override void _Process(float delta)
        {
            if (!init)
                return;

            // CustomIntegrator = false;

            uint pastTick = clientTick - InterpolationDelay;

            var fromNode = stateBuffer.First;
            var toNode = fromNode.Next;

            while (toNode != null && toNode.Value.timestamp <= pastTick)
            {
                fromNode = toNode;
                toNode = fromNode.Next;
                stateBuffer.RemoveFirst();
            }

            FrameVehicleSnapshot newValue = null;

            if (toNode != null)
                newValue = Interpolate(fromNode.Value, toNode.Value, pastTick);
            else
                newValue = fromNode.Value;


            var pos = GlobalTransform.origin;
            var rot = Rotation;

            if (newValue.movementState != null)
            {
                EngineForce = newValue.movementState.engineForce;
                Brake = newValue.movementState.brake;
                Steering = newValue.movementState.steering;
                current_gear = newValue.movementState.currentGear;
                engine_RPM = newValue.movementState.engineRpm;
            }

            LinearVelocity = (newValue.origin - pos);
            AngularVelocity = (newValue.rotation - rot);

            //DoWalk(newValue.origin - pos);
        }

        public static FrameVehicleSnapshot Interpolate(FrameVehicleSnapshot from, FrameVehicleSnapshot to, uint clientTick)
        {
            GD.Print("interpolating");
            float t = ((float)(clientTick - from.timestamp)) / (to.timestamp - from.timestamp);

            var snap = new FrameVehicleSnapshot
            {
                origin = from.origin.LinearInterpolate(to.origin, t),
                rotation = from.rotation.LinearInterpolate(to.rotation, t),
                movementState = from.movementState,
                timestamp = 0
            };

            snap.movementState.engineForce = Mathf.Lerp(from.movementState.engineForce, to.movementState.engineForce, t);
            snap.movementState.brake = Mathf.Lerp(from.movementState.brake, to.movementState.brake, t);
            snap.movementState.steering = Mathf.Lerp(from.movementState.steering, to.movementState.steering, t);
            snap.movementState.engineRpm = Mathf.Lerp(from.movementState.engineRpm, to.movementState.engineRpm, t);
            snap.movementState.currentGear = to.movementState.currentGear;

            return snap;
        }

        [Puppet]
        public void OnNewServerVehicleSnapshot(string correctedSnapshotJson)
        {
            driver.networkStats.AddInPackage(correctedSnapshotJson);

            var frame = Game.Networking.NetworkCompressor.Decompress<FrameVehicleSnapshot>(correctedSnapshotJson);
            clientTick = frame.timestamp;
            AddState(frame);
        }

        public void AddState(FrameVehicleSnapshot frame)
        {
            if (stateBuffer.Count > 0 && stateBuffer.Last.Value.timestamp > frame.timestamp)
            {
                return;
            }

            stateBuffer.AddLast(frame);
        }


        public override void _PhysicsProcess(float delta)
        {
            clientTick++;

            if (driver == null)
                engineStarted = false;

            ProcessSound();
            setSmokeSpeed();

            (GetNode("lights/break_r") as OmniLight).Visible = (brake_val > 0.1 && engineStarted);
            (GetNode("lights/break_r") as OmniLight).Visible = (brake_val > 0.1 && engineStarted);

            (GetNode("lights/reverse_r") as OmniLight).Visible = (current_gear <= -1 && engineStarted);
            (GetNode("lights/reverse_l") as OmniLight).Visible = (current_gear <= -1 && engineStarted);

            var rand = new RandomNumberGenerator();

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

                    wl.contact.UnitSize = LinearVelocity.Length() / 50;
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
                    var skid = (Spatial)skidScene.Instance();
                    var node = GetTree().Root.GetNodeOrNull<Spatial>("entry/vbox/vbox_client/client_viewport/client/world/particles");
                    if (node != null)
                    {
                        node.AddChild(skid);

                        var trans = skid.GlobalTransform;
                        trans.origin = wl.node.GlobalTransform.origin + new Vector3(0, -wl.node.WheelRadius + 0.15f, 0);
                        skid.GlobalTransform = trans;
                    }
                }
            }

        }

        private float getVolumeWeight(float currentValue, float minValue, float totalMax)
        {
            //current: 1.8 => 36%
            var maxPerecent = (minValue / totalMax) * 100;
            var currentPerecent = (currentValue / totalMax) * 100;

            var p = (minValue / 5) * 100;

            //vol1:  min: 0 to 1 => until 0% => 
            //vol1:  min: 1 to 2 => until 20% => 
            //vol2:  min: 2 to 4 => until 40% => 
            //vol3:  min: 4 to 5 => until 80% => 

            if (currentPerecent > p)
            {
                //rest
                return currentPerecent - p;
            }
            else
            {
                return 0f;
            }

        }

        private void ProcessSound()
        {

            var collsionPlayer = GetNode("audio").GetNode("collision") as AudioStreamPlayer3D;
            var slidePlayer = GetNode("audio").GetNode("slide") as AudioStreamPlayer3D;
            var idlePlayer = GetNode("audio").GetNode("idle") as AudioStreamPlayer3D;
            var lowRpmPlayer = GetNode("audio").GetNode("low_rpm") as AudioStreamPlayer3D;
            var medRpmPlayer = GetNode("audio").GetNode("med_rpm") as AudioStreamPlayer3D;
            var highRpmPlayer = GetNode("audio").GetNode("high_rpm") as AudioStreamPlayer3D;

            if (engineStarted)
            {
                var scale = Mathf.Clamp(Mathf.Abs(engine_RPM / 1000f) * 1.0f, 1.0f, 5.0f);
                var gear_radio_list = new List<float>(gear_ratio.ToList());
                gear_radio_list.Reverse();


                if (scale <= gear_radio_list[0])
                {
                    idlePlayer.UnitDb = 0;
                    lowRpmPlayer.UnitDb = -80;
                    medRpmPlayer.UnitDb = -80;
                    highRpmPlayer.UnitDb = -80;
                }
                else if (scale > gear_radio_list[0] && scale < gear_radio_list[1])
                {
                    var inPercent2 = ((scale / gear_radio_list[1]) * 100) / 100;
                    var inPercent1 = (1 - inPercent2);

                    idlePlayer.UnitDb = Mathf.Lerp(-80, 0, inPercent1);
                    lowRpmPlayer.UnitDb = Mathf.Lerp(-80, 0, inPercent2);
                    medRpmPlayer.UnitDb = -80;
                    highRpmPlayer.UnitDb = -80;
                }
                else if (scale > gear_radio_list[1] && scale < gear_radio_list[2])
                {
                    var inPercent2 = ((scale / gear_radio_list[2]) * 100) / 100;
                    var inPercent1 = (1 - inPercent2);

                    idlePlayer.UnitDb = -80;
                    lowRpmPlayer.UnitDb = Mathf.Lerp(-80, 0, inPercent1);
                    medRpmPlayer.UnitDb = Mathf.Lerp(-80, 0, inPercent2);
                    highRpmPlayer.UnitDb = -80;
                }
                else if (scale > gear_radio_list[2] && scale < gear_radio_list[3])
                {
                    var inPercent2 = ((scale / gear_radio_list[3]) * 100) / 100;
                    var inPercent1 = (1 - inPercent2);

                    idlePlayer.UnitDb = -80;
                    lowRpmPlayer.UnitDb = -80;
                    medRpmPlayer.UnitDb = Mathf.Lerp(-80, 0, inPercent1);
                    highRpmPlayer.UnitDb = Mathf.Lerp(-80, 0, inPercent2);
                }
                else
                {
                    idlePlayer.UnitDb = -80;
                    lowRpmPlayer.UnitDb = -80;
                    medRpmPlayer.UnitDb = -80;
                    highRpmPlayer.UnitDb = 0;
                }

                GD.Print("idle:" + idlePlayer.UnitDb + " low:" + lowRpmPlayer.UnitDb + " low:" + medRpmPlayer.UnitDb + " high:" + highRpmPlayer.UnitDb);

            }
            else
            {
                idlePlayer.UnitDb = -80;
                lowRpmPlayer.UnitDb = -80;
                medRpmPlayer.UnitDb = -80;
                highRpmPlayer.UnitDb = -80;
            }



            var bodies = GetCollidingBodies();


            /*
                    //collisions
                    if (bodies.Count > 0 && Mathf.Abs(prev_lvl - lvl) > 0.5)
                    {
                        if (!collsionPlayer.Playing)
                        {
                            var rand = new RandomNumberGenerator();
                            rand.Randomize();

                            collsionPlayer.PitchScale = rand.Randf() * 2 + 1;
                            rand.Randomize();

                            collsionPlayer.Stream = collision_sound[rand.RandiRange(0, collision_sound.Count - 1)];
                            collsionPlayer.Play();
                        }

                    }
                    // Sliding
                    if (!slidePlayer.Playing)
                    {
                        slidePlayer.Stream = slideSound;
                        slidePlayer.Play();
                    }
                    if (bodies.Count > 0 && lvl > 0.3)
                        slide_player_unit_db_target = 2;
                    else
                        slide_player_unit_db_target = -80;


                    slide_player_unit_db += (slide_player_unit_db_target - slide_player_unit_db) * 0.9f;
                    slidePlayer.UnitDb = slide_player_unit_db;
                    */

        }

    }
}