using Godot;
using System;

namespace Game
{
    public class NetworkPlayer : KinematicBody
    {
        public NetworkPlayerState playerState = new NetworkPlayerState();

        protected const float JUMP_POWER = 15.0f;
        protected const float JUMP_DURATION = 1f;
        protected const float AIR_SPEED = 6.0f;

        protected const float MAX_SPRINT_SPEED = 10.0f;
        protected const float MAX_SPEED = 3.0f;
        protected const float AIR_CONTROL = 2.0f;
        protected const float FRICTION = 1.0f;
        protected const float FRICTION_LIMITER = 1.5f;

        protected const float GRAVITY = 35f;

        protected const float ACCEL = 5f;
        protected const float DEACCEL = 5f;
        protected const float SPRINT_ACCEL = 3.0f;

        protected const float GRAB_DISTANCE = 50f;
        protected const float THROW_FORCE = 100f;
        protected const float MAX_SLOPE_ANGLE = 45.0f;

        protected Transform shape_orientation = new Transform();


        private bool canJump = true;

        public int networkId { get; set; }
        public int authId { get; set; }
        public RayCast rayGround { get; set; }
        public CollisionShape shape { get; set; }

        public Networking.NetworkStats networkStats = new Networking.NetworkStats();

        [Export]
        public NodePath rayGroundPath;

        [Export]
        public NodePath shapePath;

        private Timer jumpTimer = new Timer();
        private Timer pingTimer = new Timer();

        public void InitPlayer()
        {
            rayGround = (RayCast)GetNode(rayGroundPath);
            shape = (CollisionShape)GetNode(shapePath);
            shape_orientation = shape.GlobalTransform;

            jumpTimer.Name = "jump_timer";
            jumpTimer.WaitTime = JUMP_DURATION;

            AddChild(jumpTimer);

            jumpTimer.Connect("timeout", this, "onJumpTimerTimeout");




            if (Multiplayer.IsNetworkServer())
            {
                pingTimer.Name = "ping_timer";
                pingTimer.WaitTime = 1;
                AddChild(pingTimer);

                pingTimer.Connect("timeout", this, "GetPings");
                pingTimer.Start();
                pingTimer.Autostart = true;
            }

        }

        public void onJumpTimerTimeout()
        {
            canJump = true;
        }

        [PuppetSync]
        public void onTeleport(Vector3 vec)
        {
            GD.Print("[Client] Teleport to " + vec.ToString());
            SetPlayerPosition(vec);
        }

        public Vector3 GetPlayerPosition()
        {
            return GlobalTransform.origin;
        }

        public Vector3 GetPlayerRotation()
        {
            return shape.RotationDegrees;
        }

        public void SetPlayerRotation(Vector3 rotation)
        {
            shape.RotationDegrees = rotation;
        }

        [Puppet]
        public void RequestPingPackage(uint time)
        {
            RpcUnreliableId(1, "ReceivePingPackage", time);
        }

        [Puppet]
        public void IncomingServerPing(uint ping)
        {
            networkStats.pingMs = ping;
        }

        [Remote]
        public void ReceivePingPackage(uint time)
        {
            var id = Multiplayer.GetRpcSenderId();
            networkStats.pingMs = OS.GetTicksMsec() - time;

            RpcUnreliableId(id, "IncomingServerPing", networkStats.pingMs);
        }

        public void GetPings()
        {
            //send request ping to all clients by rpc
            RpcUnreliable("RequestPingPackage", OS.GetTicksMsec());
        }

        public void SetPlayerPosition(Vector3 vec)
        {
            var gt = GlobalTransform;
            gt.origin = vec;

            GlobalTransform = gt;
        }

        public Vector3 DoWalk(Vector3 velocity)
        {
            return MoveAndSlide(velocity, Vector3.Up, true, 4, Mathf.Deg2Rad(MAX_SLOPE_ANGLE), false);
        }

        public Vector3 CalculateVelocityByInput(PlayerInput movementState, float delta)
        {
            if (rayGround.IsColliding() == true)
            {
                playerState.onGround = true;
            }
            else
                playerState.onGround = false;


            movementState.velocity.y -= GRAVITY * delta;

            var target = movementState.cam_direction;
            if (movementState.isSprinting)
                target *= MAX_SPRINT_SPEED;
            else
                target *= MAX_SPEED;

            var accel = 0.0f;
            if ((movementState.cam_direction).Dot(movementState.velocity) > 0)
            {
                if (movementState.isSprinting)
                    accel = SPRINT_ACCEL;
                else
                    accel = ACCEL;
            }
            else
            {
                accel = DEACCEL;
            }



            var tempValue = movementState.velocity.LinearInterpolate(target, accel * delta);
            tempValue.y = 0;


            //friction
            //todo: need a better solution

            if (tempValue.Length() < FRICTION_LIMITER && movementState.movement_direction.Length() == 0)
            {


                tempValue = movementState.velocity.LinearInterpolate(Vector3.Zero, FRICTION);
                movementState.velocity.z = tempValue.z;
                movementState.velocity.x = tempValue.x;
            }
            else
            {
                movementState.velocity.z = tempValue.z;
                movementState.velocity.x = tempValue.x;
            }






            //want jump
            if (playerState.onGround && movementState.isJumping && canJump)
            {
                movementState.velocity.y = JUMP_POWER;
                canJump = false;
                jumpTimer.Start();
            }

            //face moving dir
            doCameraRotation(movementState, delta);



            return movementState.velocity;
        }

        private void doCameraRotation(PlayerInput movementState, float delta)
        {
            if ((movementState.cam_direction).Dot(movementState.velocity) > 0)
            {
                var quat_from = new Quat(shape_orientation.basis);
                var tf = new Transform();
                var quat_to = new Quat(tf.LookingAt(-movementState.cam_direction, Vector3.Up).basis);

                shape_orientation.basis = new Basis(quat_from.Slerp(quat_to, delta * 10f));
                var srp = shape.Rotation;
                srp.y = shape_orientation.basis.GetEuler().y;
                shape.Rotation = srp;
            }
        }
    }
}
