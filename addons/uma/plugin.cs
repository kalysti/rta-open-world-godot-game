using Godot;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Reflection;

[Tool]
public class plugin : EditorPlugin
{
    VBoxContainer editDock;

    UMASkeleton skeleton = null;

    public UMAOverlayGizmoPlugin gizmoPlugin = new UMAOverlayGizmoPlugin();
    public override bool Handles(Godot.Object @object)
    {
        return @object != null && (@object is UMASkeleton);
    }
    public override void MakeVisible(bool visible)
    {
        if (!visible)
            Edit(null);
    }
    public override void Edit(Godot.Object @object)
    {
        if (@object != null && @object is UMASkeleton)
        {
            skeleton = @object as UMASkeleton;
            editDock.Visible = true;
        }
        else
        {
            skeleton = null;
            editDock.Visible = false;
        }
    }
    public override void _EnterTree()
    {
        GD.Print("register type");

        editDock = (VBoxContainer)GD.Load<PackedScene>("res://addons/uma/UmaSkeletonMenu.tscn").Instance();

        editDock.FindNode("save_pose").Connect("pressed", this, "openPoseSaveFile");
        (editDock.FindNode("save_pose_dialog") as FileDialog).Connect("file_selected", this, "savePose");

        editDock.FindNode("save_reciepe").Connect("pressed", this, "openReciepeSaveFile");
        (editDock.FindNode("save_reciepe_dialog") as FileDialog).Connect("file_selected", this, "saveReciepe");

        editDock.FindNode("load_reciepe").Connect("pressed", this, "openReciepeFile");
        (editDock.FindNode("load_reciepe_dialog") as FileDialog).Connect("file_selected", this, "loadReciepe");

        AddControlToContainer(CustomControlContainer.SpatialEditorSideLeft, editDock);
        editDock.Visible = false;

        AddCustomType("UMADnaHumanoid", "Resource", GD.Load<Script>("res://addons/uma/utils/UMADnaHumanoid.cs"), GD.Load<Texture>("res://addons/uma/icons/dna.png"));

        AddCustomType("UMASlotOverlayResource", "Resource", GD.Load<Script>("res://addons/uma/utils/UMASlotOverlayResource.cs"), GD.Load<Texture>("res://addons/uma/icons/slot.png"));
        AddCustomType("UMAOverlayResource", "Resource", GD.Load<Script>("res://addons/uma/utils/UMAOverlayResource.cs"), GD.Load<Texture>("res://addons/uma/icons/overlay.png"));
        AddCustomType("UMASkeleton", "Skeleton", GD.Load<Script>("res://addons/uma/utils/UMASkeleton.cs"), GD.Load<Texture>("res://addons/uma/icons/skeleton.png"));

        AddCustomType("UMAOverlay", "MeshInstance", GD.Load<Script>("res://addons/uma/utils/UMAOverlay.cs"), GD.Load<Texture>("res://addons/uma/icons/overlay.png"));

        AddSpatialGizmoPlugin(gizmoPlugin);
    }
    private void openPoseSaveFile()
    {
        if (skeleton != null)
        {
            (editDock.FindNode("save_pose_dialog") as FileDialog).PopupCentered();
        }
    }
    private void openReciepeSaveFile()
    {
        if (skeleton != null)
        {
            (editDock.FindNode("save_reciepe_dialog") as FileDialog).PopupCentered();
        }
    }
    private void openReciepeFile()
    {
        if (skeleton != null)
        {
            (editDock.FindNode("load_reciepe_dialog") as FileDialog).PopupCentered();
        }
    }


    private void savePose(string filePath)
    {
        if (skeleton != null)
        {
            var file = new File();
            file.Open(filePath, File.ModeFlags.Write);
            file.StoreString(JsonConvert.SerializeObject(skeleton.getBones()));
            file.Close();
        }
    }

    private void loadReciepe(string filePath)
    {
        if (skeleton != null)
        {
            var serStr = skeleton.getReciepeFromEdtior();
            var file = new File();

            if (file.FileExists(filePath))
            {
                file.Open(filePath, File.ModeFlags.Read);
                var converted = JsonConvert.DeserializeObject<UMAReciepe>(file.GetAsText());
                skeleton.loadReciepeToEdtior(converted);
                file.Close();
            }
        }
    }
    private void saveReciepe(string filePath)
    {
        if (skeleton != null)
        {
            var serStr = skeleton.getReciepeFromEdtior();
            var converted = JsonConvert.SerializeObject(serStr);


            var file = new File();
            file.Open(filePath, File.ModeFlags.Write);
            file.StoreString(converted);
            file.Close();
        }
    }
    public override void _ExitTree()
    {
        RemoveCustomType("UMADnaHumanoid");
        RemoveCustomType("UMASkeleton");
        RemoveCustomType("UMASlotOverlayResource");
        RemoveCustomType("UMAOverlayResource");
        RemoveCustomType("UMAOverlay");


        RemoveControlFromContainer(CustomControlContainer.SpatialEditorSideLeft, editDock);

        RemoveSpatialGizmoPlugin(gizmoPlugin);

    }
}
