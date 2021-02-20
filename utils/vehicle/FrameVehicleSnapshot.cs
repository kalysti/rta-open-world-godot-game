using Godot;
using System;

namespace Game
{
    [Serializable]
    public class FrameVehicleSnapshot
    {
        public PlayerInput movementState;
        public uint timestamp;
        public uint serverTimestamp;

        public Vector3 origin;
        public Vector3 rotation;

        public bool sended = false;
      
    }
}