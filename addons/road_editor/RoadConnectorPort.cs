using Godot;
using System;

[Tool]
public class RoadConnectorPort : ImmediateGeometry
{
    private bool _up = false;
    [Export]
    public bool up
    {
        get { return _up; }
        set
        {
            _up = value;
            SetMaterial();
        }
    }
    private bool _test = false;

    [Export]
    public bool test
    {
        get { return _test; }
        set
        {
            _test = value;
            SetMaterial();
        }
    }

    public RoadConnectorSide GetSide()
    {
        if (GetParent() is RoadConnectorSide)
            return GetParent() as RoadConnectorSide;
        else
            return null;
    }
    public override void _Ready()
    {
        if (Engine.EditorHint)
        {
            SetMaterial();
        }
        else
            SetProcess(false);
    }

    private void SetMaterial()
    {
        if (Engine.EditorHint)
        {
            if (test)
                MaterialOverride = GD.Load<SpatialMaterial>("res://addons/road_editor/up.material");
            else if (up)
                MaterialOverride = GD.Load<SpatialMaterial>("res://addons/road_editor/down.material");
            else
            {
                MaterialOverride = GD.Load<SpatialMaterial>("res://addons/road_editor/correction.material");
            }
        }
    }

    public override void _Process(float delta)
    {
        Clear();
        Begin(Mesh.PrimitiveType.Triangles, null);
        AddSphere(15, 15, 10f, false);
        End();
    }


}