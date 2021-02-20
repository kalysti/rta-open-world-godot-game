using Godot;
using System;
using Newtonsoft.Json;

[Serializable]
public class UMAReciepeOverlay
{
    public Color overlayAlbedoColor { get; set; }
    public string usedMaterial { get; set; }

    
    [JsonIgnore]
    public string overlayPath { get; set; }
}
