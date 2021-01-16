
#if TOOLS

using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

[Tool]
public class CustomGrid : EditorPlugin
{
    VBoxContainer dock;
    VBoxContainer editDock;

    RoadGridMap roadGrid = null;
    RoadConnector editConnector = null;

    private RoadConnectorPort lastPort = null;


    public override bool Handles(Godot.Object @object)
    {
        return @object != null && (@object is RoadGridMap || @object is RoadConnector);
    }
    public override void MakeVisible(bool visible)
    {
        if (!visible)
            Edit(null);
    }
    public override void Edit(Godot.Object @object)
    {
        if (@object != null && @object is RoadGridMap)
        {
            roadGrid = (RoadGridMap)@object;
            dock.Visible = true;
            editDock.Visible = false;

            if (listInit != true)
                LoadList();
            else
                addButtons();
        }
        else if (@object != null && @object is RoadConnector)
        {
            dock.Visible = false;
            editDock.Visible = true;

            editConnector = (RoadConnector)@object;
            roadGrid = null;
        }
        else
        {
            roadGrid = null;
            editConnector = null;
            dock.Visible = false;
            editDock.Visible = false;
        }
    }
    public override void _EnterTree()
    {
        dock = (VBoxContainer)GD.Load<PackedScene>("res://addons/road_editor/CustomGridDock.tscn").Instance();
        editDock = (VBoxContainer)GD.Load<PackedScene>("res://addons/road_editor/CustomEditRoadDock.tscn").Instance();

        AddControlToContainer(CustomControlContainer.SpatialEditorSideLeft, dock);
        AddControlToContainer(CustomControlContainer.SpatialEditorSideLeft, editDock);

        var connectorPortScript = GD.Load<Script>("res://addons/road_editor/RoadConnectorPort.cs");
        var connectorSideScript = GD.Load<Script>("res://addons/road_editor/RoadConnectorSide.cs");
        var roadConnectorScript = GD.Load<Script>("res://addons/road_editor/RoadConnector.cs");

        var script = GD.Load<Script>("res://addons/road_editor/RoadGridMap.cs");
        var texture = GD.Load<Texture>("res://addons/road_editor/icons/road.png");
        var texture_connector = GD.Load<Texture>("res://addons/road_editor/icons/grid.png");
        var texture_point = GD.Load<Texture>("res://addons/road_editor/icons/point.png");
        var texture_side = GD.Load<Texture>("res://addons/road_editor/icons/side.png");

        dock.Visible = false;
        editDock.Visible = false;

        (editDock.GetNode("route_connector") as Button).Connect("pressed", this, "initConnector");

        AddCustomType("RoadGrid", "Spatial", script, texture);
        AddCustomType("RoadConnectorPort", "ImmediateGeometry", connectorPortScript, texture_point);
        AddCustomType("RoadConnectorSide", "Spatial", connectorSideScript, texture_side);
        AddCustomType("RoadConnector", "Spatial", roadConnectorScript, texture_connector);
    }

    private void initConnector()
    {
        if (editConnector != null)
        {
            var sideScene = GD.Load<PackedScene>("res://addons/road_editor/RoadConnectorSide.tscn");
            var tf = (RoadConnectorSide)sideScene.Instance();

            var connectorPortScript = GD.Load<Script>("res://addons/road_editor/RoadConnectorPort.cs");

            var topRight = new RoadConnectorPort();
            var topLeft = new RoadConnectorPort();

            var bottomRight = new RoadConnectorPort();
            var bottomLeft = new RoadConnectorPort();

            tf.Name = "port_1";

            tf.AddChild(bottomRight);
            tf.AddChild(bottomLeft);
            tf.AddChild(topRight);
            tf.AddChild(topLeft);

            editConnector.AddChild(tf);

            tf.Owner = GetEditorInterface().GetEditedSceneRoot();

            topRight.Owner = GetEditorInterface().GetEditedSceneRoot();
            topLeft.Owner = GetEditorInterface().GetEditedSceneRoot();
            bottomRight.Owner = GetEditorInterface().GetEditedSceneRoot();
            bottomLeft.Owner = GetEditorInterface().GetEditedSceneRoot();


            bottomRight.Name = "down_right";
            bottomLeft.Name = "down_left";

            topRight.Name = "up_right";
            topLeft.Name = "up_left";

            tf.side_top_right_path = topRight.GetPath();
            tf.side_top_left_path = topLeft.GetPath();
            tf.side_bottom_right_path = bottomRight.GetPath();
            tf.side_bottom_left_path = bottomLeft.GetPath();

            topRight.SetScript(connectorPortScript);
            topLeft.SetScript(connectorPortScript);
            bottomRight.SetScript(connectorPortScript);
            bottomLeft.SetScript(connectorPortScript);
        }
    }



    private System.Collections.Generic.List<string> getFiles(string dir)
    {
        var files = new System.Collections.Generic.List<string>();
        var directories = new Directory();
        directories.Open(dir);
        directories.ListDirBegin();
        while (true)
        {
            var file = directories.GetNext();
            if (string.IsNullOrEmpty(file))
                break;
            else if (!file.BeginsWith("."))
                files.Add(file);
        }

        directories.ListDirEnd();

        return files;
    }

    private System.Collections.Generic.List<RoadConnector> tempScenes = new System.Collections.Generic.List<RoadConnector>();

    private bool listInit = false;
    public void LoadList()
    {
        tempScenes = new System.Collections.Generic.List<RoadConnector>();

        foreach (var item in getFiles(roadGrid.assetPath))
        {
            var scene = GD.Load<PackedScene>(roadGrid.assetPath + "/" + item);
            var instance = scene.Instance();


            if (!(instance is RoadConnector))
            {
                continue;
            }

            bool usable = false;
            RoadConnector _tempConn = (RoadConnector)instance;
            foreach (var child in _tempConn.GetChildren())
            {
                if (child is RoadConnectorSide)
                {
                    var route_child = (RoadConnectorSide)child;
                    if (route_child.getPorts().Count >= 4)
                    {
                        usable = true;
                    }
                }
            }

            if (usable)

                tempScenes.Add((RoadConnector)instance);
        }

        listInit = true;
        addButtons();
    }
    private RoadConnector getLastConnector()
    {
        if (roadGrid.GetChildCount() == 0)
            return null;

        RoadConnector lastConnector = null;
        foreach (var item in roadGrid.GetChildren())
        {
            if (item is RoadConnector)
                lastConnector = item as RoadConnector;
        }

        return lastConnector;
    }
    private void addButtons()
    {

        foreach (Button button in dock.GetChildren())
        {
            dock.RemoveChild(button);
        }

        var lastConnector = getLastConnector();

        Button bt3 = new Button();
        bt3.Name = "Reload";
        bt3.Text = "Reload";
        bt3.Connect("pressed", this, "LoadList");

        Button bt4 = new Button();
        bt4.Name = "Swap";
        bt4.Text = "Swap";
        bt4.Connect("pressed", this, "swapLastNode");

        Button bt5 = new Button();
        bt5.Name = "Undo";
        bt5.Text = "Undo";
        bt5.Connect("pressed", this, "undoLastNode");

        dock.AddChild(bt3);
        dock.AddChild(bt4);
        dock.AddChild(bt5);

        //add extra nodes with swapping
        //compare levels
        var extraList = new List<RoadConnector>();
        if (lastConnector != null)
        {
            
            foreach (var scene in tempScenes)
            {
                int port = 0;
                foreach (var res in lastConnector.findFreeRoute(lastConnector, scene))
                {
                    Button bt = new Button();
                    bt.Name = scene.Name;
                    var name = scene.Name + ": Port " + port;

                    bt.Text = (res.needSwaped) ? name + " (Right) " : name;

                    dock.AddChild(bt);

                    var col = new Godot.Collections.Array();

                    col.Add(scene);
                    col.Add((res.needSwaped) ? 180 : 0);
                    col.Add(res.side);

                    bt.Connect("pressed", this, "addNode", col);
                    port++;
                }
            }
        }
        else
        {

            foreach (var scene in tempScenes)
            {
                Button bt = new Button();
                bt.Name = scene.Name;
                bt.Text = scene.Name;

                dock.AddChild(bt);

                var col = new Godot.Collections.Array();
                col.Add(scene);
                col.Add(0);
                col.Add(null);

                bt.Connect("pressed", this, "addNode", col);
            }

        }


    }

    private void swapLastNode()
    {
        var sourceConnector = getLastConnector();
        if (sourceConnector != null)
        {
            var swap = (sourceConnector.Swappded) ? 0 : 180;
            undoLastNode();

            var newNode = tempScenes.Where(tf => tf.Name == sourceConnector.modelName).FirstOrDefault();
            if (newNode != null)
            {
                addNode(newNode, swap);
            }
            else
            {
                GD.Print("Cant find model name");
            }
        }
    }

    private void undoLastNode()
    {
        var sourceConnector = getLastConnector();
        if (sourceConnector != null)
        {
            foreach (var port in sourceConnector.connectedPorts)
            {
                var node_from = GetEditorInterface().GetEditedSceneRoot().GetNode(port.Key);
                var node_to = GetEditorInterface().GetEditedSceneRoot().GetNode(port.Value);

                var side_from = (RoadConnectorSide)node_from;
                var side_to = (RoadConnectorSide)node_to;

                if (side_from != null && side_to != null)
                {
                    var connector = side_from.GetConnector();
                    if (connector != null)
                    {
                        connector.FreePort(side_to, side_from);
                    }
                }
            }
            roadGrid.RemoveChild(sourceConnector);
            addButtons();
        }
    }

    public RoadConnector createConnector(RoadConnector tempConnector)
    {

        RoadConnector road = (RoadConnector)tempConnector.Duplicate();
        //road.meshPath = tempConnector.meshPath;
        //  var instance = scene.Instance();
        //  var road = (RoadConnector)instance;

        roadGrid.AddChild(road);

        road.Translation = Vector3.Zero;
        road.Rotation = Vector3.Zero;
        road.Owner = GetEditorInterface().GetEditedSceneRoot();


        return road;
    }

    private void addNodes(Node child, Node target)
    {
        var node = child as Node;
        var newNode = (Node)node.Duplicate();
        newNode.Name = node.Name;
        target.AddChild(newNode);


        newNode.Owner = GetEditorInterface().GetEditedSceneRoot();

        foreach (var item in node.GetChildren())
        {
            if (item is Node)
                addNodes((Node)item, newNode);
        }
    }

    private FindTargetMesh findClosestMesh(RoadConnector connector, RoadConnectorPort port)
    {
        MeshInstance mesh = null;
        Vector3 currentFace = Vector3.Zero;
        var newMesh = port.GetSide().mesh;

        float oldDistance = 99999;

        if (port.GetSide().findMesh == false && newMesh != null && newMesh is MeshInstance)
        {
            GD.Print("use no mesh filter");
            var tempMesh = (MeshInstance)newMesh;
            foreach (var face2 in tempMesh.Mesh.GetFaces())
            {
                var face = tempMesh.Transform.Xform(face2);

                var newDistance = port.Translation.DistanceTo(face);
                if (newDistance <= oldDistance)
                {
                    currentFace = face;
                    mesh = tempMesh;
                    oldDistance = newDistance;
                }

            }
        }
        else if (port.GetSide().findMesh)
        {
            GD.Print("find by meshes");
            foreach (var tempMesh in connector.GetMeshes())
            {
                foreach (var face2 in tempMesh.Mesh.GetFaces())
                {
                    var face = tempMesh.Transform.Xform(face2);

                    var newDistance = port.Translation.DistanceTo(face);
                    if (newDistance <= oldDistance)
                    {
                        currentFace = face;
                        mesh = tempMesh;
                        oldDistance = newDistance;
                    }
                }
            }
        }
        else
        {
            GD.Print("cant find any mesh");
        }

        var tag = new FindTargetMesh { mesh = mesh, vec = connector.Transform.Xform(currentFace) };
        return tag;
    }

    private float calclateRadius(RoadConnector connector, RoadConnectorPort left, RoadConnectorPort right, RoadConnectorPort bottom_left, RoadConnectorPort top_right)
    {
        var source_ports_front_0_test = findClosestMesh(connector, left);
        var source_ports_front_1_test = findClosestMesh(connector, right);
        var source_ports_rears_0_test = findClosestMesh(connector, bottom_left);
        var source_ports_rears_1_test = findClosestMesh(connector, top_right);

        var v1 = source_ports_front_0_test.vec;
        var v2 = source_ports_front_1_test.vec;
        var v3 = source_ports_rears_0_test.vec;
        var v4 = source_ports_rears_1_test.vec;

        v1.y = 0;
        v2.y = 0;
        v3.y = 0;
        v4.y = 0;

        var totalRadiusSource = (v2 - v1).AngleTo((v4 - v3));
        return Mathf.Rad2Deg(totalRadiusSource);
    }

    public void addNode(RoadConnector connector, int degress = 0, RoadConnectorSide targetSideToBeUse = null)
    {
        var sourceConnector = getLastConnector();
        var targetConnector = createConnector(connector);
        targetConnector.modelName = connector.Name;

        var targetSide = targetConnector.findFreeSide();

        if (targetSideToBeUse != null)
        {
            targetSide = (RoadConnectorSide)targetConnector.FindNode(targetSideToBeUse.Name);
        }
        GD.Print("side: " + targetSide);

        targetConnector.currentSide = targetSide.GetPath();

        var target_top_left = targetSide.portTopLeft;
        var target_top_right = targetSide.portTopRight;
        var target_bottom_left = targetSide.portBottomLeft;
        var target_bottom_right = targetSide.portBottomRight;

        //check radius and set ip up
        var target_radius = calclateRadius(targetConnector, target_top_left, target_top_right, target_bottom_left, target_bottom_right);
        GD.Print("target radius:" + target_radius);

        if (degress > 0)
        {
            GD.Print("swapped or over 90");

            //swap ports
            var temp_top_left = targetSide.portTopLeft;
            var temp_top_right = targetSide.portTopRight;
            var temp_bottom_left = targetSide.portBottomLeft;
            var temp_bottom_right = targetSide.portBottomRight;

            target_top_left = temp_bottom_left;
            target_top_right = temp_bottom_right;
            target_bottom_left = temp_top_left;
            target_bottom_right = temp_top_right;


            targetConnector.RotationDegrees = new Vector3(0, -180, 0);
            targetConnector.RotationDegrees += new Vector3(0, target_radius, 0);

            targetConnector.Swappded = true;
        }

        if (sourceConnector != null)
        {

            var sourceSide = sourceConnector.GetCurrentSide();

            // var sourceSide = sourceConnector.getLastSide();
            // var sourceSideLast = sourceConnector.getFirstSide();

            var source_top_left = sourceSide.portTopLeft;
            var source_top_right = sourceSide.portTopRight;
            var source_bottom_left = sourceSide.portBottomLeft;
            var source_bottom_right = sourceSide.portBottomRight;

            var source_radius = calclateRadius(sourceConnector, source_top_left, source_top_right, source_bottom_left, source_bottom_right);

            Vector3 xf1_global = Vector3.Zero;
            Vector3 xf2_global = Vector3.Zero;

            if (sourceConnector.Swappded)
            {
                GD.Print("swapped");

                var temp_top_left = sourceSide.portTopLeft;
                var temp_top_right = sourceSide.portTopRight;
                var temp_bottom_left = sourceSide.portBottomLeft;
                var temp_bottom_right = sourceSide.portBottomRight;

                source_top_left = temp_bottom_left;
                source_top_right = temp_bottom_right;
                source_bottom_left = temp_top_left;
                source_bottom_right = temp_top_right;

                //one of this hav eto be disabled
                targetConnector.RotationDegrees += sourceConnector.RotationDegrees;
                targetConnector.RotationDegrees -= new Vector3(0, -180, 0);
                //targetConnector.RotationDegrees += new Vector3(0, source_radius * -1, 0);
            }
            else
            {
                targetConnector.RotationDegrees += sourceConnector.RotationDegrees;
                targetConnector.RotationDegrees += new Vector3(0, source_radius * -1, 0);
            }

            var ports_front_0 = findClosestMesh(targetConnector, target_bottom_left);
            var ports_front_1 = findClosestMesh(targetConnector, target_bottom_right);

            var source_ports_0 = findClosestMesh(sourceConnector, source_top_left);
            var source_ports_1 = findClosestMesh(sourceConnector, source_top_right);

            var v1 = ports_front_1.vec;
            var v2 = ports_front_0.vec;

            var v3 = source_ports_1.vec;
            var v4 = source_ports_0.vec;

            v1.y = 0;
            v2.y = 0;
            v3.y = 0;
            v4.y = 0;

            xf1_global = v1.LinearInterpolate(v2, 0.5f);
            xf2_global = v3.LinearInterpolate(v4, 0.5f);

            var orig_port_front_left = xf2_global - xf1_global;
            //   targetConnector.TranslateObjectLocal(orig_port_front_left);
            var gt2 = targetConnector.Transform;
            gt2.origin = orig_port_front_left;
            targetConnector.Transform = gt2;

            targetConnector.ClosePort(sourceSide, targetSide);
            sourceConnector.ClosePort(targetSide, sourceSide);
        }

        addButtons();
    }

    //only for testing
    private void createTestPoint(Vector3 transform)
    {
        var t = new RoadConnectorPort();
        var gt2 = t.Transform;
        gt2.origin = transform;
        t.Transform = gt2;
        t.test = true;

        roadGrid.AddChild(t);
        t.Owner = GetEditorInterface().GetEditedSceneRoot();
        t.Scale = new Vector3(0.25f, 0.25f, 0.25f);
    }

    public override void _ExitTree()
    {
        RemoveCustomType("RoadGrid");
        RemoveCustomType("RoadConnector");
        RemoveCustomType("RoadConnectorPort");
        RemoveCustomType("RoadConnectorSide");

        RemoveControlFromContainer(CustomControlContainer.SpatialEditorSideLeft, dock);
        RemoveControlFromContainer(CustomControlContainer.SpatialEditorSideLeft, editDock);

        dock.Free();
        editDock.Free();
    }
}
#endif