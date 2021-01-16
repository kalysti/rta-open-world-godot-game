using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[Tool]
public class RoadConnector : Spatial
{

    [Export]
    public bool Swappded = false;

    [Export]
    public Godot.Collections.Dictionary<string, string> connectedPorts = new Godot.Collections.Dictionary<string, string>();

    [Export]
    public string modelName = "";

    [Export]
    public NodePath currentSide { get; set; }

    public override void _Ready()
    {

    }

    public void ClosePort(RoadConnectorSide side_from, RoadConnectorSide side_to)
    {
        if (!connectedPorts.ContainsKey(side_from.GetPath()))
            connectedPorts.Add(side_from.GetPath(), side_to.GetPath());
    }

    public void FreePort(RoadConnectorSide side_from, RoadConnectorSide side_to)
    {
        foreach (var item in connectedPorts.Where(tf => tf.Key == side_from.GetPath() && tf.Value == side_to.GetPath()))
        {
            connectedPorts.Remove(item);
        }
    }


    public List<RoadConnectorSide> GetSides()
    {
        List<RoadConnectorSide> sides = new List<RoadConnectorSide>();
        foreach (var item in GetChildren())
        {
            if (item is RoadConnectorSide)
            {
                sides.Add((RoadConnectorSide)item);
            }
        }
        return sides;
    }

    public List<MeshInstance> GetMeshes()
    {
        List<MeshInstance> sides = new List<MeshInstance>();
        foreach (var item in GetChildren())
        {
            if (item is MeshInstance)
            {
                sides.Add((MeshInstance)item);
            }
        }
        return sides;
    }

    public List<RoadConnectorSide> findFreeSides(RoadConnectorSide currentSideInUse = null)
    {
        List<RoadConnectorSide> sides = new List<RoadConnectorSide>();

        foreach (var side in GetSides())
        {
            if (connectedPorts == null)
            {
                sides.Add(side);
            }
            else
            {
                var amount = connectedPorts.Count(tf => tf.Value == side.GetPath());

                if (amount < 2)
                    sides.Add(side);
            }
        }
        return sides;
    }

    public List<RoadConnectorRouteResult> findFreeRoute(RoadConnector lastConnector, RoadConnector toConnector)
    {
        var result = new List<RoadConnectorRouteResult>();
        var currentSide = lastConnector.GetCurrentSide();

        var type = lastConnector.Swappded ? currentSide.bottomType : currentSide.topType;
        var level = lastConnector.Swappded ? currentSide.bottomSideLevel : currentSide.topSideLevel;


        foreach (var item in toConnector.GetSides().Where(tf => tf.topSideLevel == level && tf.topType == type))
        {
            result.Add(new RoadConnectorRouteResult { side = item, needSwaped = true });
        }

        foreach (var item in toConnector.GetSides().Where(tf => tf.bottomSideLevel == level && tf.bottomType == type))
        {
            result.Add(new RoadConnectorRouteResult { side = item, needSwaped = false });
        }


        return result;
    }

    public RoadConnectorSide GetCurrentSide()
    {

        if (currentSide != null)
        {
            var node = GetNodeOrNull(currentSide);
            if (node != null && node is RoadConnectorSide)
                return (RoadConnectorSide)node;
        }

        return null;
    }

    public RoadConnectorSide findFreeSide(RoadConnectorSide side = null)
    {
        return findFreeSides(side).FirstOrDefault();
    }

}