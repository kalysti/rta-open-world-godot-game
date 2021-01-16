using Godot;
using System;
using ZeroFormatter;

namespace Game
{
    [Serializable]
    public class NetworkPlayerSync 
    {

        public Vector3 position  = Vector3.Zero;

        public Vector3 rotation   = Vector3.Zero;
        public Vector3 velocity   = Vector3.Zero;

        public int networkId  = 0;
    }
}