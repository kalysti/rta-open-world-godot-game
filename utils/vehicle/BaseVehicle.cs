
using Godot;
using System;
using System.Collections.Generic;
using Game;

public class BaseVehicle : VehicleBody
{

    public float engine_RPM = 0.0f;

    [Export] public float MAX_ENGINE_FORCE = 200f;
    [Export] public float MAX_BRAKE_FORCE = 5.0f;
    [Export] public float MAX_STEER_ANGLE = 0.5f;
    [Export] public JoystickList joy_steering = JoystickList.AnalogLx;
    [Export] public float steering_mult = -1.0f;
    [Export] public JoystickList joy_throttle = JoystickList.AnalogR2;
    [Export] public float throttle_mult = 1.0f;
    [Export] public JoystickList joy_brake = JoystickList.AnalogL2;
    [Export] public float brake_mult = 1.0f;

    [Export] public float max_engine_RPM = 5000.0f;
    [Export] public float min_engine_RPM = 1000.0f;


    [Export] public float steer_speed = 5.0f;

    [Export]
    public NodePath steerNodePath = null;
    public Spatial steerNode = null;
    public NetworkPlayer driver { get; set; }

    [Export]
    public NodePath engineSmokePath = null;
    public Spatial engineSmoke = null;

    public bool engineStarted = false;
    public int vehicleId = -1;


    [Export]
    public Godot.Collections.Array<float> gear_ratio = new Godot.Collections.Array<float>();

    protected Dictionary<string, wheel> wheels = new Dictionary<string, wheel>();

    public override void _Ready()
    {

        if (engineSmokePath != null)
        {
            var engineSmokeNode = GetNodeOrNull(engineSmokePath);
            if (engineSmokeNode != null && engineSmokeNode is Spatial)
                engineSmoke = (Spatial)engineSmokeNode;
        }

        if (steerNodePath != null)
        {
            var node = GetNodeOrNull(steerNodePath);
            if (node != null && node is Spatial)
                steerNode = (Spatial)node;
        }

        if (engineSmoke != null)
            engineSmoke.Visible = false;

        InitWheel("FL");
        InitWheel("FR");
        InitWheel("BL");
        InitWheel("BR");
    }

    public float getSpeed()
    {
        // return current_speed_mps * 3600.0f / 1000.0f;
        return LinearVelocity.Length();
    }

    public override void _IntegrateForces(PhysicsDirectBodyState state)
    {
        if (driver != null)
        {
            var chair = GetNode("points/driver_inside") as ImmediateGeometry;
            driver.SetPlayerPosition(chair.GlobalTransform.origin);
            driver.Rotation = GlobalTransform.basis.GetEuler();
            driver.shape.Rotation = chair.Rotation;
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        if (driver == null)
            engineStarted = false;
    }


    public void SetVehicleRotation(Vector3 rotation)
    {
        Rotation = rotation;
    }

    public Vector3 GetVehicleRotation()
    {
        return Rotation;
    }

    public Vector3 GetVehiclePosition()
    {
        return GlobalTransform.origin;
    }
    public void SetVehiclePosition(Vector3 vec)
    {
        var gt = GlobalTransform;
        gt.origin = vec;

        GlobalTransform = gt;
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


    public bool StartEngine()
    {
        if (!engineStarted)
        {
            engineStarted = true;

            if (engineSmoke != null)
                engineSmoke.Visible = true;
        }
        else
        {
            engineStarted = false;

            if (engineSmoke != null)
                engineSmoke.Visible = false;
        }

        return engineStarted;
    }


}