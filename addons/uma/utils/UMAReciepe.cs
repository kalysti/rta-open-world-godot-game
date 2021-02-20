using Godot;
using System;
using UMA;
using System.Collections.Generic;
using Newtonsoft.Json;

[Serializable]
public class UMAReciepe
{
    public Dictionary<string, Dictionary<string, UMAReciepeOverlay>> slots = new Dictionary<string, Dictionary<string, UMAReciepeOverlay>>();

    public bool isMale { get; set; }

    public Godot.Collections.Dictionary<string, Color> skinColor { get; set; }

    public UMADnaHumanoid dna { get; set; }

    [JsonIgnore]
    public string slotPath { get; set; }

}
