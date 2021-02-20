
using Godot;
using System;
using Newtonsoft.Json;

[Serializable]
public class GodotBindPose
{
    public Quat rotation { get; set; }
    public Vector3 origin { get; set; }
    public Vector3 scale { get; set; }
    public Basis basis { get; set; }
    public int bone { get; set; }
    public int index { get; set; }
    public int parent { get; set; }
    public string name { get; set; }

    [JsonIgnore]
    public Transform tf
    {
        get { var t = new Transform(rotation, origin);  return t; }
        set
        {
            rotation = value.basis.Quat();
            origin = value.origin;
        }
    }

}