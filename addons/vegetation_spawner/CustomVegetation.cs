
#if TOOLS

using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

[Tool]
public class CustomVegetation : EditorPlugin
{
    VBoxContainer dock;

    VegetationSpawner vegGrid = null;


    public override bool Handles(Godot.Object @object)
    {
        return @object != null && (@object is VegetationSpawner);
    }

    public override void MakeVisible(bool visible)
    {
        if (!visible)
            Edit(null);
    }
    private bool mousePressed = false;
    private Vector2 mousePosition = new Vector2();

    private Camera editorCamera = null;

    public override bool ForwardSpatialGuiInput(Camera camera, InputEvent @event)
    {
        mousePressed = false;

        var captured_event = false;

        if (@event is InputEventMouseButton)
        {
            var mb = @event as InputEventMouseButton;

            if (mb.ButtonIndex == (int)ButtonList.Left)
            {
                captured_event = true;

                if (!mb.Pressed)
                    mousePressed = false;
                else if (!mb.Control && !mb.Alt)
                    mousePressed = true;
            }
        }
        else if (@event is InputEventMouseMotion)
        {
            var motion = @event as InputEventMouseMotion;
            mousePosition = motion.Position;
        }

        editorCamera = camera;

        return captured_event;
    }

    private Node get_instance_root(Node node)
    {
        while (node != null && node.Filename == "")
            node = node.GetParent();
        return node;
    }


    public override void _PhysicsProcess(float delta)
    {
        if (vegGrid == null)
            return;

        if (editorCamera == null)
            return;

        if (!IsInstanceValid(editorCamera))
        {
            editorCamera = null;
            return;
        }

        if (!mousePressed)
            return;


        var ran = new RandomNumberGenerator();
        var objects = vegGrid.amount;


        if (mousePressed)
        {

            while (objects > 0)
            {
                ran.Randomize();

                if (vegGrid.scenes.Count > 0)
                {
                    var randomScene = vegGrid.scenes[ran.RandiRange(0, vegGrid.scenes.Count - 1)];

                    var ranX = ran.RandfRange(-1, 1);
                    ran.Randomize();
                    var ranY = ran.RandfRange(-1, 1);
                    ran.Randomize();

                    var mp = mousePosition;
                    mp.x += ranX * vegGrid.spread;
                    mp.y += ranY * vegGrid.spread;

                    var ray_origin = editorCamera.ProjectRayOrigin(mp);
                    var ray_dir = editorCamera.ProjectRayNormal(mp);
                    var ray_distance = editorCamera.Far;

                    //  var newPos = ray_origin;
                    var getPos = GetPosition(ray_origin, ray_dir, ray_distance);

                    if (getPos != Vector3.Zero)
                    {
                        CreatePoint(randomScene, getPos);
                    }
                    else
                    {
                        GD.PrintErr("Cant find terrain:" + getPos);
                    }
                }
                objects--;
            }
        }

        mousePressed = false;
    }


    public Vector3 GetPosition(Vector3 ray_origin, Vector3 ray_dir, float ray_distance)
    {
        var space_state = GetViewport().World.DirectSpaceState;
        var hit = space_state.IntersectRay(ray_origin, ray_origin + ray_dir * ray_distance, new Godot.Collections.Array(), 1);
        if (hit != null && hit.Count > 0)
        {
            Node hit_instance_root = null;
            if (hit["collider"] != null)
            {
                hit_instance_root = get_instance_root((Node)hit["collider"]);
            }

            if (hit["collider"] == null || !(hit_instance_root.GetParent() is VegetationSpawner))
            {
                var node = (Node)hit["collider"];
                if (node == vegGrid.terrain)
                    return (Vector3)hit["position"];
            }
        }

        return Vector3.Zero;
    }


    public void CreatePoint(PackedScene scene, Vector3 pos)
    {
        var mat = (Spatial)scene.Instance();

        vegGrid.AddChild(mat);
        mat.Translation = pos;
        //  mat.Name = scene.ResourceName;
        var rand = new RandomNumberGenerator();
        rand.Randomize();
        mat.RotateY(rand.RandfRange(-Mathf.Pi, Mathf.Pi));

        rand.Randomize();

        var randScale = rand.RandfRange(vegGrid.minScale, 1.0f);
        mat.Scale = new Vector3(randScale, randScale, randScale);
        mat.Owner = GetEditorInterface().GetEditedSceneRoot();
    }


    public override void Edit(Godot.Object @object)
    {
        if (@object != null && @object is VegetationSpawner)
        {
            vegGrid = (VegetationSpawner)@object;
            dock.Visible = true;

        }
        else
        {
            vegGrid = null;
            dock.Visible = false;
        }
    }

    public override void _EnterTree()
    {
        var texture = GD.Load<Texture>("res://addons/vegetation_spawner/icons/grid.png");

        dock = (VBoxContainer)GD.Load<PackedScene>("res://addons/vegetation_spawner/CustomVegetationGridDock.tscn").Instance();
        AddControlToContainer(CustomControlContainer.SpatialEditorSideLeft, dock);

        var script = GD.Load<Script>("res://addons/vegetation_spawner/VegetationSpawner.cs");
        dock.Visible = false;

        AddCustomType("VegetationSpawner", "Spatial", script, texture);
    }


    public override void _ExitTree()
    {
        RemoveCustomType("VegetationSpawner");

        RemoveControlFromContainer(CustomControlContainer.SpatialEditorSideLeft, dock);

        dock.Free();
    }

}
#endif