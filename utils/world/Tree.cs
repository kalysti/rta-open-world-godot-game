using Godot;
using System;

[Tool]
public class Tree : Spatial
{
    [Export]
    public NodePath meshPath = null;
    public MeshInstance mesh = null;

    public Area area = null;
    public CollisionShape shape = null;

    [Export]
    public float areaSize = 10.0f;


    [Export]
    public NodePath vertexShapePath = null;
    public CollisionShape vertexShape = null;

    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {

        if (meshPath != null)
        {
            var node = GetNodeOrNull(meshPath);
            if (node != null && node is MeshInstance)
                mesh = (MeshInstance)node;
        }

        if (meshPath != null)
        {
            var vertex_shape = GetNodeOrNull(vertexShapePath);
            if (vertex_shape != null && vertex_shape is CollisionShape)
                vertexShape = (CollisionShape)vertex_shape;
        }

        if (!Engine.IsEditorHint())
        {

            area = new Area();
            AddChild(area);

            //important for prevent overlapping
            area.SetCollisionMaskBit(0, false);
            area.SetCollisionMaskBit(19, true);
            area.SetCollisionLayerBit(0, false);
            area.SetCollisionLayerBit(19, true);


            shape = new CollisionShape();
            var boxShape = new BoxShape();
            boxShape.Extents = new Vector3(areaSize, areaSize, areaSize);
            shape.Shape = boxShape;

            area.AddChild(shape);
        }


    }

    public override void _Process(float delta)
    {
        //check for players and cars to enable or disable col mask
        if (area != null && shape != null && vertexShape != null)
        {
            if (area.GetOverlappingBodies().Count > 0)
            {

                vertexShape.Visible = true;
                vertexShape.Disabled = false;
                return;
            }
        }

        if (vertexShape != null)
        {
            vertexShape.Visible = false;
            vertexShape.Disabled = true;
        }

    }
}
