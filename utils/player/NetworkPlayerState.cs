using Godot;
using System;
using ZeroFormatter;
namespace Game
{
    [ZeroFormattable]
    [Serializable]
    public class NetworkPlayerState
    {
        [Index(0)]
        public bool inVehicle = false;
        [Index(1)]
        public bool isSprinting = false;
        [Index(2)]
        public bool isAiming = false;
        [Index(3)]
        public bool onGround = false;
        [Index(4)]
        public bool isClimbing = false;
        [Index(5)]
        public bool weaponEquipped = false;


    }
}