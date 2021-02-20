
#if TOOLS

using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

[Tool]
public class CustomMeshTool : EditorPlugin
{
    VBoxContainer editDock;
    MeshLod meshLod = null;
    MeshInstance mesh = null;

    public List<MeshInstance> selectionList = new List<MeshInstance>();
    public List<MeshLod> selectionLodList = new List<MeshLod>();

    public override bool Handles(Godot.Object @object)
    {
        return @object != null && (@object is MeshInstance || @object is MeshLod);
    }
    public override void MakeVisible(bool visible)
    {
        if (!visible)
            Edit(null);
    }

    public override void Edit(Godot.Object @object)
    {
        selectionLodList.Clear();
        selectionList.Clear();
        if (@object != null && @object is MeshInstance)
        {
            meshLod = null;

            (editDock.GetNode("create_lod") as Button).Visible = true;
            (editDock.GetNode("create_food_collision") as Button).Visible = false;
            (editDock.GetNode("create_collision") as Button).Visible = false;
            (editDock.GetNode("destroy_mesh") as Button).Visible = false;
            (editDock.GetNode("redraw_mesh") as Button).Visible = false;

            (editDock.GetNode("destroy_mesh") as Button).Visible = false;
            (editDock.GetNode("redraw_mesh") as Button).Visible = false;

            (editDock.GetNode("lod2_quality") as Slider).Value = 0.7f;
            (editDock.GetNode("lod3_quality") as Slider).Value = 0.5f;

            if ((@object as MeshInstance).GetParent() is MeshLod)
            {
                mesh = null;
                editDock.Visible = false;
            }
            else
            {
                mesh = (MeshInstance)@object;
                editDock.Visible = true;
            }
        }
        else if (@object != null && @object is MeshLod)
        {
            mesh = null;
            editDock.Visible = true;
            meshLod = @object as MeshLod;

            (editDock.GetNode("create_lod") as Button).Visible = false;
            (editDock.GetNode("create_food_collision") as Button).Visible = true;
            (editDock.GetNode("create_collision") as Button).Visible = true;
            (editDock.GetNode("destroy_mesh") as Button).Visible = true;
            (editDock.GetNode("redraw_mesh") as Button).Visible = true;

            (editDock.GetNode("lod2_quality") as Slider).Value = meshLod.lod2Quality;
            (editDock.GetNode("lod3_quality") as Slider).Value = meshLod.lod3Quality;
        }
        else
        {
            mesh = null;
            meshLod = null;
            editDock.Visible = false;
        }
    }

    public override void _EnterTree()
    {
        editDock = (VBoxContainer)GD.Load<PackedScene>("res://addons/mesh_lod/CustomEditMeshDock.tscn").Instance();

        AddControlToContainer(CustomControlContainer.SpatialEditorSideLeft, editDock);

        var script = GD.Load<Script>("res://addons/mesh_lod/MeshLod.cs");
        var texture = GD.Load<Texture>("res://addons/mesh_lod/icons/mesh.png");

        editDock.Visible = false;

        (editDock.GetNode("create_lod") as Button).Connect("pressed", this, "createLod");
        (editDock.GetNode("create_food_collision") as Button).Connect("pressed", this, "createCollisionFootsteps");
        (editDock.GetNode("create_collision") as Button).Connect("pressed", this, "createCollision");
        (editDock.GetNode("destroy_mesh") as Button).Connect("pressed", this, "destroyMesh");
        (editDock.GetNode("redraw_mesh") as Button).Connect("pressed", this, "redrawMesh");

        AddCustomType("MeshLod", "Spatial", script, texture);
    }


    public override void _ExitTree()
    {
        RemoveCustomType("MeshLod");
        RemoveControlFromContainer(CustomControlContainer.SpatialEditorSideLeft, editDock);

        editDock.Free();
    }
    public void createCollisionFootsteps()
    {
        if (meshLod != null)
        {
            var script = GD.Load<Script>("res://addons/footsteps/FootstepStaticBody.cs");
            createCollision();

            foreach (var child in meshLod.GetChildren())
            {
                if (child is MeshInstance)
                {
                    foreach (var child2 in (child as MeshInstance).GetChildren())
                    {
                        if (child2 is StaticBody)
                        {
                            (child2 as StaticBody).SetScript(script);
                        }
                    }
                }
            }
        }
    }
    public void createCollision()
    {
        if (meshLod != null)
        {
            GD.Print("Create collision");

            foreach (var child in meshLod.GetChildren())
            {
                if (child is MeshInstance)
                {
                    var c = child as MeshInstance;

                    if (c.GetChildCount() > 0)
                        continue;

                    c.CreateTrimeshCollision();
                    GD.Print("set trimmish");

                }
            }
        }
    }
    public void createLod()
    {
        if (mesh != null)
        {
            createLodSys(mesh);
        }
        else if (selectionList.Count() > 0)
        {
            foreach (var ms in selectionList)
            {
                createLodSys(ms);
            }

            selectionList.Clear();
        }
    }
    public void redrawMesh()
    {
        if (meshLod != null)
        {
            var lod = destroyLodMesh(meshLod);
            createLodSys(lod);
        }
    }
    public void destroyMesh()
    {
        if (meshLod != null)
        {
            destroyLodMesh(meshLod);
        }
        else if (selectionLodList.Count() > 0)
        {
            foreach (var ms in selectionLodList)
            {
                destroyLodMesh(ms);
            }

            selectionLodList.Clear();
        }
    }

    private MeshInstance destroyLodMesh(MeshLod meshLod)
    {
        if (meshLod != null)
        {
            var name = meshLod.Name;

            var lod1 = meshLod.GetNodeOrNull<MeshInstance>("lod1");
            meshLod.RemoveChild(lod1);

            var parent = meshLod.GetParent();
            parent.RemoveChild(meshLod);
            lod1.Name = name;

            parent.AddChild(lod1);
            lod1.Owner = GetEditorInterface().GetEditedSceneRoot();

            lod1.Translation = meshLod.Translation;
            lod1.Rotation = meshLod.Rotation;
            lod1.Scale = meshLod.Scale;

            foreach (var item in lod1.GetChildren())
            {
                lod1.RemoveChild(item as Node);
            }

            return lod1;
        }
        else
            return null;
    }
    private void createLodSys(MeshInstance _mesh)
    {
        if (_mesh != null)
        {
            GD.Print("Create mesh");

            if (_mesh.GetChildCount() > 0)
            {
                GD.Print("Mesh needs to be clear. Remove childs at first");
                return;
            }

            var q2 = (float)(editDock.GetNode("lod2_quality") as Slider).Value;
            var q3 = (float)(editDock.GetNode("lod3_quality") as Slider).Value;

            var lod2Mesh = MeshDecimator.MeshDecimatorTool.generateMesh(q2, _mesh);
            var lod3Mesh = MeshDecimator.MeshDecimatorTool.generateMesh(q3, _mesh);

            //set scale to all other meshinstances
            var tf = (MeshLod)GD.Load<PackedScene>("res://addons/mesh_lod/MeshLod.tscn").Instance();
            _mesh.GetParent().AddChild(tf);
            tf.Translation = _mesh.Translation;
            tf.Rotation = _mesh.Rotation;
            tf.Scale = _mesh.Scale;
            tf.lod2Quality = q2;
            tf.lod3Quality = q3;
            tf.Owner = GetEditorInterface().GetEditedSceneRoot();
            _mesh.GetParent().RemoveChild(_mesh);
            tf.AddChild(_mesh);

            _mesh.Owner = GetEditorInterface().GetEditedSceneRoot();
            tf.Name = _mesh.Name;
            _mesh.Name = "lod1";

            _mesh.Translation = Vector3.Zero;
            _mesh.Rotation = Vector3.Zero;
            _mesh.Scale = new Vector3(1, 1, 1);

            var lod2 = createLodMesh(_mesh, lod2Mesh);
            lod2.Name = "lod2";

            var lod3 = createLodMesh(_mesh, lod3Mesh);
            lod3.Name = "lod3";

            tf.AddChild(lod2);
            tf.AddChild(lod3);

            lod2.Owner = GetEditorInterface().GetEditedSceneRoot();
            lod3.Owner = GetEditorInterface().GetEditedSceneRoot();

            tf.doLoding();
        }
    }

    private MeshInstance createLodMesh(MeshInstance origMesh, ArrayMesh mesh)
    {
        var newMesh = origMesh.Duplicate() as MeshInstance;
        newMesh.Mesh = mesh;

        for (int i = 0; i < newMesh.Mesh.GetSurfaceCount(); i++)
        {
            newMesh.Mesh.SurfaceSetMaterial(i, origMesh.Mesh.SurfaceGetMaterial(i));
        }

        for (int i = 0; i < origMesh.GetSurfaceMaterialCount(); i++)
        {
            newMesh.SetSurfaceMaterial(i, origMesh.GetSurfaceMaterial(i));
        }

        return newMesh;
    }

}
#endif