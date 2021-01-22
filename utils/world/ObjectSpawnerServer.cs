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
                clearSystemObjects();

                //detect veg and grid spawner
                foreach (var item in Server.database.Table<WorldObject>().ToList())
                {
                    _spawnQueue.Enqueue(item);
                }

                getSystemObjects();
            }
        }

        /**
            Clearing system objects
        */
        private void clearSystemObjects()
        {
            Server.database.Table<WorldObject>().Where(tf => tf.type == WorldObjectType.SYSTEM).Delete();
        }

        /**
            Create system objects from terrain generators
        */
        private void getSystemObjects()
        {
            GD.Print("[Server] Init object list");

            foreach (var item in GetParent().GetNode("map_holder/map").GetChildren())
            {
                if (item is RoadGridMap || item is VegetationSpawner)
                {
                    moveObjectsToDatabase((item as Node).GetChildren());
                }
            }

            //clear exist map objects
            clearMapObjects();
        }

        /**
            Helper function for terrain generator storage
        */
        private void moveObjectsToDatabase(Godot.Collections.Array childs)
        {
            foreach (var child in childs)
            {
                if (!(child is Spatial))
                    continue;

                var item = child as Spatial;
                var modelpath = item.Filename.Replace(".tscn", "").Replace("res://", "");
                GD.Print("[Server][Objects] Create" + modelpath);

                AddObjectToDatabase(modelpath, WorldObjectType.SYSTEM, item.GlobalTransform.origin, item.Rotation);
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
        public void AddObject(string model, WorldObjectType type, Vector3 pos, Vector3 rot)
        {
            GD.Print("[Server][Object] Create " + model);

            var obj = AddObjectToDatabase(model, type, pos, rot);
            var objectJson = Networking.NetworkCompressor.Compress(obj);

            Rpc("OnNewObject", objectJson);
        }

        private WorldObject AddObjectToDatabase(string model, WorldObjectType type, Vector3 pos, Vector3 rot)
        {
            var obj = new WorldObject
            {
                modelName = model,
                type = type
            };

            obj.SetPosition(pos);
            obj.SetRotation(rot);

            Server.database.Insert(obj);
            _spawnQueue.Enqueue(obj);

            return obj;
        }


    }
}

