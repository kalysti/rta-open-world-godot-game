using Godot;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Reflection;

[Tool]
public class UMAOverlayGizmoPlugin : EditorSpatialGizmoPlugin
{
    public override string GetName()
    {
        return "UMAOverlayGizmoSpatial";
    }

    public override bool HasGizmo(Spatial spatial)
    {
        return (spatial is UMAOverlay);
    }

    public UMAOverlayGizmoPlugin() : base()
    {
        CreateMaterial("main", new Color(1, 0, 0));
        CreateHandleMaterial("handles");
    }

    public override void Redraw(EditorSpatialGizmo gizmo)
    {
        gizmo.Clear();
        var spatial = gizmo.GetSpatialNode();

        var lines = new List<Vector3>();

        lines.Add(new Vector3(0, 1, 0));
        // lines.Add(new Vector3(0, spatial.my_custom_value, 0));

        var handles = new List<Vector3>();

        handles.Add(new Vector3(0, 1, 0));
        //handles.Add(new Vector3(0, spatial.my_custom_value, 0));

        gizmo.AddLines(lines.ToArray(), GetMaterial("main", gizmo), false);
        gizmo.AddHandles(handles.ToArray(), GetMaterial("handles", gizmo));
    }
}