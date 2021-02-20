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

        public bool isInitalized = false;


        public ServerVehicle vehicle = null;

        public int vehicleId = -1;
        

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

        [Remote]
        public void requestStartStopEngine()
        {
            if (vehicle != null)
                RpcId(networkId, "setVehicleEngine", vehicle.StartEngine());
        }

        [Remote]
        public void requestVehicle(int _vehicleId)
        {

            if (vehicle != null)
            {
                disableVehicleMode(vehicle);

                vehicle = null;
                vehicleId = -1;

                RpcId(networkId, "leaveVehicle");
            }
            else
            {
                var vehicleNode = (world as ServerWorld).spawner.GetNodeOrNull<WorldObjectNode>(_vehicleId.ToString());
                if (vehicleNode == null)
                    return;

                vehicleId = _vehicleId;
                vehicle = vehicleNode.holdedObject as ServerVehicle;
                vehicle.driver = this;
                shape.Disabled = true;
                playerState.inVehicle = true;

                RpcId(networkId, "enterVehicle", vehicleId);
                vehicle.Rpc("PosUpdate", OS.GetTicksMsec(), vehicle.GetVehiclePosition(), vehicle.GetVehicleRotation());
            }
        }


        public override void _PhysicsProcess(float delta)
        {
            if (!isInitalized)
                return;

            FrameSnapshot sendSnapshot = null;
            int movesMade = 0;

            while (snapshotQueue.Count > 0 && movesMade < 10)
            {
                var lastSnapshot = snapshotQueue.Dequeue();

                if (lastSnapshot != null)
                {
                    lastProccesTimestamp = lastSnapshot.timestamp;

                    if (vehicle != null)
                    {
                        lastSnapshot.movementState.velocity = velocity;
                        velocity = vehicle.Drive(lastSnapshot.movementState, delta);
                        lastSnapshot.movementState.velocity = velocity;

                    }
                    else
                    {
                        lastSnapshot.movementState.velocity = velocity;
                        velocity = DoWalk(CalculateVelocityByInput(lastSnapshot.movementState, delta));
                        lastSnapshot.movementState.velocity = velocity;
                    }

                    sendSnapshot = lastSnapshot;
                }

                movesMade++;
            }

            if (sendSnapshot != null)
            {

                if (vehicle != null)
                {
                    var newSnapshot = new FrameVehicleSnapshot();

                    newSnapshot.origin = vehicle.GetVehiclePosition();
                    newSnapshot.rotation = vehicle.GetVehicleRotation();
                    newSnapshot.timestamp = sendSnapshot.timestamp;
                    newSnapshot.serverTimestamp = OS.GetTicksMsec();
                    newSnapshot.movementState = sendSnapshot.movementState;
                    newSnapshot.movementState.velocity = vehicle.LinearVelocity;
                    newSnapshot.movementState.angular_velocity = vehicle.AngularVelocity;
                    newSnapshot.movementState.brake = vehicle.Brake;
                    newSnapshot.movementState.engineForce = vehicle.EngineForce;
                    newSnapshot.movementState.steering = vehicle.Steering;
                    newSnapshot.movementState.engineRpm = vehicle.engine_RPM;
                    newSnapshot.movementState.currentGear = vehicle.getCurrentGear();


                    var newState = Networking.NetworkCompressor.Compress(newSnapshot);
                    vehicle.RpcUnreliable("OnNewServerVehicleSnapshot", newState);

                }
                else
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