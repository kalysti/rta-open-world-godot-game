using Godot;
using System.Linq;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Game
{
    public class ServerPlayer : NetworkPlayer
    {
        private uint lastProccesTimestamp = 0;

        public Queue<FrameSnapshot> snapshotQueue = new Queue<FrameSnapshot>();

        public override void _Ready()
        {
            InitPlayer();
            (GetNode("preview_camera") as Camera).Current = true;
        }

        public void Teleport(Vector3 vec)
        {
            GD.Print("[Server][" + networkId + "] Teleport to " + vec.ToString());
            var gt = GlobalTransform;
            gt.origin = vec;
            GlobalTransform = gt;

            RpcId(networkId, "onTeleport", vec);
        }
        public Vector3 velocity = Vector3.Zero;

        public override void _PhysicsProcess(float delta)
        {
            FrameSnapshot sendSnapshot = null;
            int movesMade = 0;

            while (snapshotQueue.Count > 0 && movesMade < 10)
            {
                var lastSnapshot = snapshotQueue.Dequeue();

                if (lastSnapshot != null)
                {
                    lastSnapshot.movementState.velocity = velocity;
                    lastProccesTimestamp = lastSnapshot.timestamp;

                    var newVelocity = CalculateVelocityByInput(lastSnapshot.movementState, delta);
                    velocity = DoWalk(newVelocity);

                    lastSnapshot.movementState.velocity = velocity;
                    sendSnapshot = lastSnapshot;
                }

                movesMade++;
            }

            if (sendSnapshot != null)
            {
                var newSnapshot = new FrameSnapshot();

                newSnapshot.origin = GetPlayerPosition();
                newSnapshot.rotation = GetPlayerRotation();

                newSnapshot.timestamp = sendSnapshot.timestamp;
                newSnapshot.serverTimestamp = OS.GetTicksMsec();
                newSnapshot.movementState = sendSnapshot.movementState;
                newSnapshot.movementState.velocity = sendSnapshot.movementState.velocity;

                var newState = Networking.NetworkCompressor.Compress(newSnapshot);
                RpcUnreliable("OnServerSnapshot", newState);

                sendSnapshot = null;
            }
        }

        [Remote]
        public void OnNewInputSnapshot(string stateJson)
        {
            var id = Multiplayer.GetRpcSenderId();
            var state = Networking.NetworkCompressor.Decompress<List<FrameSnapshot>>(stateJson);

            if (networkId == id)
            {
                foreach (var item in state)
                {
                    snapshotQueue.Enqueue(item);
                }
            }
        }
    }
}