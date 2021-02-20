using System.Text;
using Godot;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using UMA;

[Tool]
public class UMASkeletonBase : Skeleton
{

    #region protected vars
    protected UMADnaHumanoid _dna = new UMADnaHumanoid();
    protected Godot.Collections.Dictionary<string, UMASlotOverlayResource> _slotList = new Godot.Collections.Dictionary<string, UMASlotOverlayResource>();
    protected Godot.Collections.Dictionary<string, Color> _skinGroup = new Godot.Collections.Dictionary<string, Color>();
    protected List<GodotBindPose> bones = new List<GodotBindPose>();

    protected string _defaultPose = "";
    protected string _slotDir = "";
    protected UMAReciepe _lastReciepe = null;
    protected MeshInstance meshin = null;
    public Queue<UMAReciepe> renderQueue = new Queue<UMAReciepe>();
    #endregion

    #region public vars
    [Export(PropertyHint.File, "*.pose")]
    public string defaultPose { get { return _defaultPose; } set { _defaultPose = value; } }

    [Export(PropertyHint.File, "*.reciepe")]
    public string defaultReciepe;

    [Export(PropertyHint.Dir, "")]
    public string slotDir { get { return _slotDir; } set { _slotDir = value; } }

    [Export]
    public bool isMale = true;

    [Export]
    public UMADnaHumanoid dna { get { return _dna; } set { _dna = value; } }

    [Export]
    public Godot.Collections.Dictionary<string, UMASlotOverlayResource> slotList { get { return _slotList; } set { _slotList = value; } }

    [Export]
    public Godot.Collections.Dictionary<string, Color> skinGroup { get { return _skinGroup; } set { _skinGroup = value; updateEditorColor(); } }
    #endregion

    #region editor update routine
    protected void editorUpdate()
    {
        if (!Engine.EditorHint)
            return;

        GD.Print("[UMA] Update recivied");

        var reciepe = getReciepeFromEdtior();
        renderQueue.Enqueue(reciepe);
    }

    protected void editorBoneUpdate()
    {
        if (!Engine.EditorHint)
            return;

        GD.Print("[UMA] Update bone recivied");

        generateDNA(_dna);

    }

    private void overlayListChanged()
    {
        if (!Engine.EditorHint)
            return;

        GD.Print("[UMA] Update recivied for overlay list");
        connectEvents();
    }

    protected void updateEditorColor()
    {
        if (!Engine.EditorHint)
            return;

        GD.Print("[UMA] Update recivied for color and mats");

        setMainSkinColor();
    }
    #endregion

    #region set material and colors
    public void setMainSkinColor(UMAReciepe reciepe = null)
    {
        if (meshin == null || meshin.Mesh == null)
            return;

        if (reciepe != null)
        {
            changeSkinColorSurface(meshin.Mesh as ArrayMesh, reciepe);
        }
        else
        {
            changeSkinColorSurface(meshin.Mesh as ArrayMesh, getReciepeFromEdtior());
        }
    }

    protected ArrayMesh changeSkinColorSurface(ArrayMesh oldMesh, UMAReciepe reciepe)
    {
        var skinableSlots = new Dictionary<string, string>();
        foreach (var slot in reciepe.slots)
        {
            var origSlot = _slotList[slot.Key];
            var slotName = slot.Key;
            float glow = origSlot.Glow;

            if (origSlot.skinGroup.ToString() != "None")
                skinableSlots.Add(slot.Key, origSlot.skinGroup.ToString());

            foreach (var overlay in slot.Value)
            {
                var overlayName = slotName + "/Overlay_" + overlay.Key;
                var origOverlay = origSlot.overlayList[overlay.Key];

                if (origOverlay != null)
                {
                    string materialPath = null;
                    if (!String.IsNullOrEmpty(overlay.Value.usedMaterial) && origOverlay.isSkinable)
                    {
                        materialPath = System.IO.Path.Combine(origOverlay.fabricPath, overlay.Value.usedMaterial + ".tres");
                    }
                    bool useAlbedoColor = origOverlay.useSkinColor;
                    Color albedoColor = overlay.Value.overlayAlbedoColor;

                    assignMaterialOverlay(oldMesh, overlayName, useAlbedoColor, albedoColor, materialPath, glow);
                }

            }
        }

        foreach (var skinableSlot in skinableSlots)
        {
            var color = reciepe.skinColor.FirstOrDefault(df => df.Key == skinableSlot.Value);
            if (color.Key == null)
                continue;

            var surface = oldMesh.SurfaceFindByName(skinableSlot.Key);
            if (surface >= 0)
            {
                var mat = oldMesh.SurfaceGetMaterial(surface);
                if (mat != null && mat is SpatialMaterial)
                {
                    (mat as SpatialMaterial).AlbedoColor = color.Value;
                }
            }
        }

        return oldMesh;
    }

    protected void assignMaterialOverlay(ArrayMesh oldMesh, string overlayName, bool useAlbedo, Color albedoColor, string materialPath, float glow)
    {
        if (oldMesh == null)
            return;

        for (int surface = 0; surface < oldMesh.GetSurfaceCount(); surface++)
        {
            var name = oldMesh.SurfaceGetName(surface);

            if (!name.Contains(overlayName))
                continue;

            var material = oldMesh.SurfaceGetMaterial(surface);
            if (!String.IsNullOrEmpty(materialPath))
            {
                //fix path combine (system.io)
                materialPath = materialPath.Replace(@"\", @"/");

                if ((material == null) || (material.ResourcePath != materialPath))
                {
                    var newMaterial = GD.Load<Material>(materialPath);

                    if (newMaterial is SpatialMaterial)
                    {
                        (newMaterial as SpatialMaterial).ParamsGrow = true;
                        (newMaterial as SpatialMaterial).ParamsGrowAmount = glow;
                    }

                    newMaterial.ResourceLocalToScene = true;
                    material = newMaterial;
                }
            }

            if (material != null)
            {
                if (useAlbedo)
                {
                    (material as SpatialMaterial).AlbedoColor = albedoColor;
                }

                (material as SpatialMaterial).ParamsGrow = true;
                (material as SpatialMaterial).ParamsGrowAmount = glow;

                oldMesh.SurfaceSetMaterial(surface, material);
            }

        }

    }
    #endregion

    #region load and set editor variables
    public UMAReciepe getReciepeFromEdtior()
    {
        var reciepe = new UMAReciepe();
        try
        {
            foreach (var slot in slotList)
            {
                if (!slot.Value.isEnable)
                    continue;

                var overlayList = new Dictionary<string, UMAReciepeOverlay>();
                foreach (var overlay in slot.Value.overlayList)
                {
                    if (overlay.Value.isEnable)
                    {
                        overlayList.Add(overlay.Key, new UMAReciepeOverlay { overlayAlbedoColor = overlay.Value.skinColor, usedMaterial = overlay.Value.currentMaterial });
                    }
                }

                reciepe.slots.Add(slot.Key, overlayList);
            }

            reciepe.skinColor = _skinGroup;
            reciepe.isMale = isMale;
            reciepe.dna = (UMADnaHumanoid)dna.Duplicate();
        }
        catch (Exception e)
        {
            GD.PrintErr("[UMA] Fails: " + e.Message + ", " + e.StackTrace);

        }

        return reciepe;
    }

    public UMAReciepe getDefaultReciepe()
    {
        return loadEditorReciepeByPath(defaultReciepe);
    }

    protected UMAReciepe loadEditorReciepeByPath(string filePath)
    {
        if (String.IsNullOrEmpty(filePath))
            return null;

        var file = new Godot.File();

        if (file.FileExists(filePath))
        {
            file.Open(filePath, Godot.File.ModeFlags.Read);
            var converted = JsonConvert.DeserializeObject<UMAReciepe>(file.GetAsText());
            file.Close();

            return converted;
        }
        return null;

    }

    public void loadReciepeToEdtior(UMAReciepe reciepe)
    {
        isMale = reciepe.isMale;
        _skinGroup = reciepe.skinColor;

        foreach (var slot in _slotList)
        {
            var isEnable = reciepe.slots.ContainsKey(slot.Key);
            slot.Value.setEnabled(isEnable);

            if (isEnable == false)
            {
                foreach (var overlay in slot.Value.overlayList)
                {
                    overlay.Value.setEnabled(isEnable);
                }
            }
            else
            {
                var reciepeSlot = reciepe.slots[slot.Key];
                foreach (var overlay in slot.Value.overlayList)
                {
                    var isOverlayEnabled = reciepeSlot.ContainsKey(overlay.Key);
                    overlay.Value.setEnabled(isOverlayEnabled);

                    if (isOverlayEnabled)
                    {
                        overlay.Value.setCurrentMaterial(reciepeSlot[overlay.Key].usedMaterial);
                        overlay.Value.setAlbedoColor(reciepeSlot[overlay.Key].overlayAlbedoColor);
                    }
                }
            }
        }
    }
    #endregion

    #region bone utility
    protected void loadDefaultBones()
    {
        if (!String.IsNullOrEmpty(_defaultPose))
        {
            var file = new Godot.File();
            file.Open(_defaultPose, Godot.File.ModeFlags.Read);
            var content = file.GetAsText();
            file.Close();

            bones = Newtonsoft.Json.JsonConvert.DeserializeObject<List<GodotBindPose>>(content);

            if (bones.Count <= 0)
            {
                GD.PrintErr("No bones found in default pose");
            }
        }
    }

    public void adjustBones(List<GodotBindPose> _bones)
    {
        foreach (var b in _bones)
        {
            var tf = new Transform();
            tf.origin = b.origin;
            tf.basis = new Basis(b.rotation);
            tf = tf.Scaled(b.scale);

            SetBoneRest(b.index, tf);
        }
    }

    public List<GodotBindPose> getBones()
    {
        var dic = new List<GodotBindPose>();
        for (int ib = 0; ib < GetBoneCount(); ib++)
        {
            var rest = GetBoneRest(ib);
            dic.Add(new GodotBindPose
            {
                origin = rest.origin,
                rotation = rest.basis.Quat(),
                scale = new Vector3(1, 1, 1),
                parent = GetBoneParent(ib),
                index = ib,
                name = GetBoneName(ib),
            });
        }

        return dic;
    }

    private object DeepClone(object obj)
    {
        object objResult = null;

        using (var ms = new MemoryStream())
        {
            var bf = new BinaryFormatter();
            bf.Serialize(ms, obj);

            ms.Position = 0;
            objResult = bf.Deserialize(ms);
        }

        return objResult;
    }

    public void generateDNA(UMADnaHumanoid dna)
    {
        if (dna == null)
            return;

        GD.Print("[UMA] Transform Mesh by DNA");
        adjustBones(bones);

        if (isMale)
        {
            var gen = new UMA.DNA.HumanMaleConverter();

            gen.poses = (List<GodotBindPose>)DeepClone(bones);
            gen.Adjust(dna);

            adjustBones(gen.poses);
        }
        else
        {

            var gen = new UMA.DNA.HumanFemaleConverter();

            gen.poses = (List<GodotBindPose>)DeepClone(bones);
            gen.Adjust(dna);

            adjustBones(gen.poses);
        }
    }
    #endregion

    #region scan slot folder and connect editor events
    protected void setSkinGroups()
    {
        foreach (UMASkinGroup sg in Enum.GetValues(typeof(UMASkinGroup)))
        {
            var key = sg.ToString();
            if (!skinGroup.ContainsKey(key))
            {
                if (key.ToLower() != "none")
                    _skinGroup.Add(key, new Color(1, 1, 1, 1));
            }
        }
    }

    protected void scanSlotFolder()
    {
        GD.Print("[UMA] Scan slot folder: " + slotDir);
        var res = new UMASlotOverlayResource();
        res.ResourceLocalToScene = true;
        res.ResourceName = "UMASlotOverlayResource";
        var path = GD.Load<CSharpScript>("res://addons/uma/utils/UMASlotOverlayResource.cs").New();

        UMA.Helper.ScanFolderUtility.scanDir<UMASlotOverlayResource>(_slotDir, "tscn", ref _slotList, path);
    }
    protected void connectEvents()
    {
        if (!Engine.EditorHint)
            return;

        GD.Print("[UMA] Connect events");
        _dna.Connect("ValueChanged", this, "editorBoneUpdate");

        foreach (var slot in slotList)
        {
            if (!slot.Value.IsConnected("ValueChanged", this, "editorUpdate"))
                slot.Value.Connect("ValueChanged", this, "editorUpdate");

            if (!slot.Value.IsConnected("OverlayListChanged", this, "overlayListChanged"))
                slot.Value.Connect("OverlayListChanged", this, "overlayListChanged");

            foreach (var ol in slot.Value.overlayList)
            {
                if (!ol.Value.IsConnected("ValueChanged", this, "editorUpdate"))
                    ol.Value.Connect("ValueChanged", this, "editorUpdate");

                if (!ol.Value.IsConnected("ColorChanged", this, "updateEditorColor"))
                    ol.Value.Connect("ColorChanged", this, "updateEditorColor");
            }
        }
    }

    protected void disconnectEvents()
    {
        if (!Engine.EditorHint)
            return;

        GD.Print("[UMA] Disconnect events");

        if (_dna != null)
        {
            if (_dna.IsConnected("ValueChanged", this, "editorBoneUpdate"))
            {
                _dna.Disconnect("ValueChanged", this, "editorBoneUpdate");
            }
        }

        if (slotList != null)
        {
            foreach (var slot in slotList)
            {
                if (slot.Value.IsConnected("ValueChanged", this, "editorUpdate"))
                    slot.Value.Disconnect("ValueChanged", this, "editorUpdate");

                if (slot.Value.IsConnected("OverlayListChanged", this, "overlayListChanged"))
                    slot.Value.Disconnect("OverlayListChanged", this, "overlayListChanged");

                foreach (var ol in slot.Value.overlayList)
                {
                    if (ol.Value.IsConnected("ValueChanged", this, "editorUpdate"))
                        ol.Value.Disconnect("ValueChanged", this, "editorUpdate");

                    if (ol.Value.IsConnected("ColorChanged", this, "updateEditorColor"))
                        ol.Value.Disconnect("ColorChanged", this, "updateEditorColor");
                }
            }
        }

    }
    #endregion

}
