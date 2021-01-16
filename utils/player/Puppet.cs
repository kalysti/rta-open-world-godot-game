using Godot;
using System;
using System.Collections.Generic;

namespace Game
{
    public class Puppet : NetworkPlayer
    {
        [Export]
        public uint InterpolationDelay = 0;

        public uint clientTick = 0;

        public Vector3 velcoity = new Vector3();

        public override void _Ready()
        {
            InitPlayer();
        }

        public LinkedList<FrameSnapshot> stateBuffer = new LinkedList<FrameSnapshot>();

        public override void _PhysicsProcess(float delta)
        {
            clientTick++;
        }

        public void PosUpdate(uint timetamp, Vector3 pos, Vector3 rot)
        {
            SetPlayerPosition(pos);
            SetPlayerRotation(rot);

            AddState(new FrameSnapshot
            {
                origin = pos,
                rotation = rot,
                timestamp = 0
            });
        }

        public override void _Process(float delta)
        {
            uint pastTick = clientTick - InterpolationDelay;
            var fromNode = stateBuffer.First;
            var toNode = fromNode.Next;

            while (toNode != null && toNode.Value.timestamp <= pastTick)
            {
                fromNode = toNode;
                toNode = fromNode.Next;
                stateBuffer.RemoveFirst();
            }

            FrameSnapshot newValue = null;
            if (toNode != null)
                newValue = Interpolate(fromNode.Value, toNode.Value, pastTick);
            else
                newValue = fromNode.Value;


            // SetPlayerPosition(newValue.origin);
            SetPlayerRotation(newValue.rotation);

            var pos = GlobalTransform.origin;
            DoWalk(newValue.origin - pos);
        }

        public static FrameSnapshot Interpolate(FrameSnapshot from, FrameSnapshot to, uint clientTick)
        {
            float t = ((float)(clientTick - from.timestamp)) / (to.timestamp - from.timestamp);

            return new FrameSnapshot
            {
                origin = from.origin.LinearInterpolate(to.origin, t),
                rotation = from.rotation.LinearInterpolate(to.rotation, t),
                timestamp = 0
            };
        }



        [Puppet]
        public void OnServerSnapshot(string correctedSnapshotJson)
        {
            var frame = Game.Networking.NetworkCompressor.Decompress<FrameSnapshot>(correctedSnapshotJson);
            clientTick = frame.timestamp;
            AddState(frame);
        }

        public void AddState(FrameSnapshot frame)
        {
            if (stateBuffer.Count > 0 && stateBuffer.Last.Value.timestamp > frame.timestamp)
            {
                return;
            }

            stateBuffer.AddLast(frame);
        }


    }
}