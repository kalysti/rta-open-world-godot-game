using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
[Tool]
public class RoadConnectorSide : Spatial
{

    [Export]
    public RoadConnectorType bottomType = RoadConnectorType.ROAD;

    [Export]
    public int bottomSideLevel = 0;

    [Export]
    public RoadConnectorType topType = RoadConnectorType.ROAD;

    [Export]
    public int topSideLevel = 0;


    [Export]
    public NodePath meshPath = null;
    public MeshInstance mesh = null; 
    
    [Export]
    public bool findMesh = false;


    [Export]
    public bool inUsage = false;
    [Export]
    public NodePath side_top_right_path;

    [Export]
    public NodePath side_top_left_path;


    [Export]
    public NodePath side_bottom_right_path;

    [Export]
    public NodePath side_bottom_left_path;

    public RoadConnectorPort portTopLeft { get; set; }
    public RoadConnectorPort portTopRight { get; set; }
    public RoadConnectorPort portBottomRight { get; set; }

    public RoadConnectorPort portBottomLeft { get; set; }
    public override void _Ready()
    {
        portTopLeft = ConnectPort(side_top_left_path);
        portTopRight = ConnectPort(side_top_right_path);
        portBottomLeft = ConnectPort(side_bottom_left_path);
        portBottomRight = ConnectPort(side_bottom_right_path);

        if (meshPath != null)
        {
            var node = GetNodeOrNull(meshPath);
            if (node != null && node is MeshInstance)
                mesh = (MeshInstance)node;
        }
    }

    private RoadConnectorPort ConnectPort(NodePath path)
    {
        if (path != null)
        {
            var node = GetNodeOrNull(path);
            if (node != null && node is RoadConnectorPort)
                return (RoadConnectorPort)node;
        }
        return null;
    }

    public RoadConnector GetConnector()
    {
        if (GetParent() is RoadConnector)
            return GetParent() as RoadConnector;
        else
            return null;
    }
    public List<RoadConnectorPort> GetTopPorts()
    {
        List<RoadConnectorPort> ports = new List<RoadConnectorPort>();
        if (portTopLeft != null)
            ports.Add(portTopLeft);
        if (portTopRight != null)
            ports.Add(portTopRight);
        return ports;
    }

    public List<RoadConnectorPort> GetBottomPorts()
    {
        List<RoadConnectorPort> ports = new List<RoadConnectorPort>();
        if (portBottomLeft != null)
            ports.Add(portBottomLeft);
        if (portBottomRight != null)
            ports.Add(portBottomRight);
        return ports;
    }

    public List<RoadConnectorPort> getPorts()
    {
        var list = new List<RoadConnectorPort>();
        foreach (var point in GetChildren())
        {
            if (point is RoadConnectorPort)
                list.Add((RoadConnectorPort)point);
        }

        return list;
    }


}