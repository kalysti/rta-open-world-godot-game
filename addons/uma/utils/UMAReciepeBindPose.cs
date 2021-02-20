using Godot;
using System;
using UMA;
using System.Collections.Generic;
using Newtonsoft.Json;

public class UMAReciepeBindPose
{

    public Transform transform { get; set; }
    public string boneName { get; set; }
    public int boneIndex { get; set; }

    public string getSelector()
    {
        return boneName + "_" + transform.origin.ToString() + transform.basis.GetEuler().ToString();
    }

}
