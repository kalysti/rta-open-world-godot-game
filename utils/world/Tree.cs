using Godot;
using System;

[Tool]
public class Tree : Spatial
{


    public Area area = null;
    public CollisionShape shape = null;

    [Export]
    public float areaSize = 10.0f;

    public int isEnabled = -1;

    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        if (!Engine.EditorHint)
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
        else
        {
            SetPhysicsProcess(false);
        }
    }

    public override void _Process(float delta)
    {
        return;
        //check for players and cars to enable or disable col mask
        if (area != null)
        {
            if (area.GetOverlappingBodies().Count > 0)
            {
                enableOrDisable(1);
            }
            else
            {
                enableOrDisable(0);
            }
        }
    }

    private void shapeHelper(Node node, bool enable)
    {
        //main level
        foreach (var child in node.GetChildren())
        {
            if (child is CollisionShape && child != shape)
            {
                (child as CollisionShape).Visible = enable;
                (child as CollisionShape).Disabled = !enable;

                //do something
                continue;
            }

            if (child is Node)
            {
                if ((child as Node).GetChildCount() > 0)
                    shapeHelper((child as Node), enable);
            }
        }
    }

    public void enableOrDisable(int enable)
    {
        if (isEnabled != enable)
        {
            GD.Print("Do something because changed to " + enable);
            isEnabled = enable;
            shapeHelper(this, (enable == 1) ? true : false);
        }
    }
}
