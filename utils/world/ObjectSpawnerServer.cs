using Godot;
using System.Collections.Generic;

namespace Game
{
    public class ObjectSpawnerServer : ObjectSpawnerBase
    {
        public override void _Ready()
        {
            base._Ready();
            if (Server.database != null)
            {
                foreach (var item in Server.database.Table<WorldObject>().ToList())
                {
                    _spawnQueue.Enqueue(item);
                }
            }
        }

        [Remote]
        public void GetObjectList(Vector3 position, float distance)
        {
            var list = new List<WorldObject>();
            foreach (WorldObjectNode x in GetChildren())
            {
                if (x.worldObject != null && (x.worldObject.GetPosition() - position).Length() <= distance)
                {
                    list.Add(x.worldObject);
                }
            }

            var id = Multiplayer.GetRpcSenderId();
            var objectJson = Networking.NetworkCompressor.Compress(list);

            RpcId(id, "UpdateClientObjectList", objectJson, GetChildCount());
        }

        [Remote]
        public void AddObject(string model,  WorldObjectType type, Vector3 pos, Vector3 rot)
        {
            GD.Print("[Server][Object] Create " + model);

            var obj = new WorldObject
            {
                modelName = model,
                type = type
            };

            obj.SetPosition(pos);
            obj.SetRotation(rot);

            Server.database.Insert(obj);
            _spawnQueue.Enqueue(obj);

            var objectJson = Networking.NetworkCompressor.Compress(obj);
            Rpc("OnNewObject", objectJson);
        }


    }
}

