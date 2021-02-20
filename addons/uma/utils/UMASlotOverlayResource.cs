using Godot;
using System.Collections.Generic;
using System.Linq;
using System;
using Newtonsoft.Json;

[Tool]
[JsonObject(MemberSerialization.OptIn)]

public class UMASlotOverlayResource : Resource
{

    private bool _isEnable = false;
    private Godot.Collections.Dictionary<string, UMAOverlayResource> _overlayList = new Godot.Collections.Dictionary<string, UMAOverlayResource>();
    private string _overlayDir = "";

    [Export]
    public UMASlotCategory category = UMASlotCategory.Head;
    private UMASkinGroup _skinGroup = UMASkinGroup.None;

    [Export]
    public UMASkinGroup skinGroup
    {
        get
        {
            return _skinGroup;
        }
        set
        {

            _skinGroup = value;
            EmitSignal(nameof(ValueChanged));
        }
    }

    [Export]
    public string BodyName = "";

    [Signal]
    public delegate void ValueChanged();

    [Signal]
    public delegate void OverlayListChanged();

    private float _glow = 0f;

    [JsonProperty]
    [Export]
    public float Glow
    {
        get
        {
            return _glow;
        }
        set
        {
            _glow = value;
            if (_isEnable)
                EmitSignal(nameof(ValueChanged));
        }
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


    public void setEnabled(bool val)
    {
        _isEnable = val;
    }

    [Export]
    [JsonProperty]
    public Godot.Collections.Dictionary<string, UMAOverlayResource> overlayList { get { return _overlayList; } set { _overlayList = value; } }


    [Export(PropertyHint.Dir, "")]
    public string overlayDir { get { return _overlayDir; } set { _overlayDir = value; scanDir(); } }

    public void scanDir()
    {
        var res = new UMAOverlayResource();
        res.ResourceLocalToScene = true;
        res.ResourceName = "UMAOverlayResource";
        var path = GD.Load<CSharpScript>("res://addons/uma/utils/UMAOverlayResource.cs").New();

        UMA.Helper.ScanFolderUtility.scanDir<UMAOverlayResource>(_overlayDir, "tscn", ref _overlayList, path);
        EmitSignal(nameof(OverlayListChanged));
    }



}
