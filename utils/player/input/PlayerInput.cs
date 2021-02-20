using Godot;
using System;
using ZeroFormatter;
namespace Game
{
    [Serializable]
    public class PlayerInput
    {
        public Vector2 movement_direction = Vector2.Zero;
        public bool isSprinting = false;
        public bool isJumping = false;
        public bool isAiming = false;
        public float engineForce = 0f;
        public float engineRpm = 0f;
        public int currentGear = 0;
        public float brake = 0f;
        public float steering = 0f;
        public Vector3 velocity = Vector3.Zero;
        public Vector3 angular_velocity = Vector3.Zero;
        public Vector3 cam_direction = Vector3.Zero;
    }
}