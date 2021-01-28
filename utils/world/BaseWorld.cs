using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using Game;

namespace Game
{
    public class BaseWorld : Node
    {
        [Export]
        public NodePath mapHolderPath;

        public Node mapHolder = null;
        public BackgroundLoader backgroundLoader = null;

        public override void _Ready()
        {
            mapHolder = (Node)GetNode(mapHolderPath);
            backgroundLoader = GetTree().Root.GetNode("BackgroundLoader") as BackgroundLoader;
        }

    }
}