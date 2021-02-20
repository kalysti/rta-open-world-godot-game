using Godot;
using System;
using System.Linq;
using Newtonsoft.Json;


[Tool]
[JsonObject(MemberSerialization.OptIn)]
public class UMAOverlayResource : Resource
{
    private string _fabricPath = "res://addons/uma/Fabric/Materials";

    [Export(PropertyHint.Dir, "")]
    public string fabricPath { get { return _fabricPath; } set { _fabricPath = value; scanDir(); } }

    private Godot.Collections.Dictionary<string, bool> _allowedMaterials = new Godot.Collections.Dictionary<string, bool>();

    [Export]
    public Godot.Collections.Dictionary<string, bool> allowedMaterials { get { return _allowedMaterials; } set { _allowedMaterials = value; } }

    [Signal]
    public delegate void ValueChanged();
    [Signal]
    public delegate void ColorChanged();

    private bool _isEnable = false;

    private string _currentMaterial = "";

    [Export]
    [JsonProperty]
    public string currentMaterial { get { return _currentMaterial; } set { _currentMaterial = value; updateNotify(); } }

    public void setCurrentMaterial(string val)
    {
        _currentMaterial = val;

        if (!String.IsNullOrEmpty(val))
        {
            _isSkinable = true;
        }
    }
    public void setAlbedoColor(Color val)
    {
        _skinColor = val;
    }

    public void setEnabled(bool val)
    {
        _isEnable = val;
    }

    [Export]
    public bool isEnable
    {
        get
        {
            return _isEnable;
        }
        set
        {
            _isEnable = value;
            EmitSignal(nameof(ValueChanged));
        }
    }
    private Color _skinColor = new Color(1, 1, 1, 1);

    [Export(PropertyHint.ColorNoAlpha)]
    [JsonProperty]
    public Color skinColor
    {
        get
        {
            return _skinColor;
        }
        set
        {
            _skinColor = value;
            _skinColor.a = 1;
            if (_isEnable)
                EmitSignal(nameof(ColorChanged));
        }
    }

    private bool _useSkinColor = true;

    [Export]
    [JsonProperty]
    public bool useSkinColor
    {
        get
        {
            return _useSkinColor;
        }
        set
        {

            _useSkinColor = value;
            EmitSignal(nameof(ColorChanged));
        }
    }


    private bool _isSkinable = false;

    [Export]
    [JsonProperty]
    public bool isSkinable
    {
        get
        {
            return _isSkinable;
        }
        set
        {

            _isSkinable = value;
            EmitSignal(nameof(ColorChanged));
        }
    }

    public void updateNotify()
    {
        if (_allowedMaterials.ContainsKey(_currentMaterial) && isEnable)
            EmitSignal(nameof(ColorChanged));
    }


    public void scanDir()
    {
        UMA.Helper.ScanFolderUtility.scanDir<bool>(_fabricPath, "tres", ref _allowedMaterials, false);
        var firstMatCount = allowedMaterials.Count(df => df.Value == true);
    }


}
