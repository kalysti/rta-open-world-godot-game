using Godot;
using System;

public class wheel 
{
    public VehicleWheel node {get;set;}
    public float rpm = 0.0f;
    public float trans  = 0.0f;
    public float prev_trans  = 0.0f;
    public AudioStreamPlayer3D spring {get;set;}
    public AudioStreamPlayer3D contact {get;set;}
    public AudioStreamPlayer3D skid {get;set;}
}
