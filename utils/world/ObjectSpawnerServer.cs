using Godot;
using System.Collections.Generic;
using System.Linq;

namespace Game
{
    public class ObjectSpawnerServer : ObjectSpawnerBase
    {
        [Export]
        public float MinDistanceToPlayer = 250f;

        [Export]
        public int scanTimeInMs = 100;

        public bool scanThreadIsRunning = false;

        public System.Threading.Thread scanThread = null;

        [Export]
        public bool refreshDatabase = true;

        public override void _ExitTree()
        {
            base._ExitTree();

            if (scanThread != null)
            {
                scanThreadIsRunning = false;
                scanThread.Abort();
            }
        }

        public void Init(BaseMap map)
        {
            if (Server.database != null)
            {
                if (refreshDatabase)
                {
                    GD.Print("[Server] Object database refreshed");
                    clearSystemObjects();
                    getSystemObjects(map);
                }

                clearMapObjects(map);
                tempObjects = Server.database.Table<WorldObject>().ToList();

                scanThreadIsRunning = true;
                scanThread = new System.Threading.Thread(scanPlayersInObjectArea);
                scanThread.Start();
            }
        }

        protected void scanPlayersInObjectArea()
        {
            while (scanThreadIsRunning)
            {
                //be sure that we can grad the next object snapshot
                if (deleteChildsQueue.Count <= 0 && createChildsQueue.Count <= 0 && creationInProgress.Count <= 0)
                {
                    //find all objects in close area
                    var currentSceneObjects = new List<WorldObject>();
                    foreach (var item in GetParent().GetNode("players").GetChildren())
                    {
                        if (item is ServerPlayer)
                        {
                            var p = item as ServerPlayer;
                            currentSceneObjects.AddRange(findObjectsInNearOfPlayer(p, MinDistanceToPlayer));
                        }
                    }

                    var needsToCreate = currentSceneObjects.Distinct().ToList();
                    var needsToDelete = new List<WorldObjectNode>();

                    foreach (var x in GetChildren())
                    {
                        if (x is WorldObjectNode)
                        {
                            var tf = x as WorldObjectNode;

                            if (currentSceneObjects.Count(df => df.Id == tf.worldObject.Id) <= 0)
                            {
                                needsToDelete.Add(tf);
                            }
                        }
                    }

                    foreach (var x in needsToCreate)
                    {
                        CreateWorldObject(x);
                    }

                    foreach (var x in needsToDelete)
                    {
                        deleteChildsQueue.Enqueue(x);
                    }
                }
                System.Threading.Thread.Sleep(scanTimeInMs);
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
        private void getSystemObjects(BaseMap map)
        {
            GD.Print("[Server] Init object list");

            foreach (var item in map.GetChildren())
            {
                if (item is RoadGridMap || item is VegetationSpawner)
                {
                    moveObjectsToDatabase((item as Node).GetChildren());
                }
            }
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

                var rot = item.GlobalTransform.basis.GetEuler();
                AddObjectToDatabase(modelpath, WorldObjectType.SYSTEM, item.GlobalTransform.origin, rot, item.Scale);
            }
        }

        [Remote]
        public void AddObject(string model, WorldObjectType type, Vector3 pos, Vector3 rot, Vector3 scale)
        {
            GD.Print("[Server][Object] Create " + model);

            var obj = AddObjectToDatabase(model, type, pos, rot, scale);
            var objectJson = Networking.NetworkCompressor.Compress(obj);
            tempObjects.Add(obj);

            Rpc("OnNewObject", objectJson);
        }

        private WorldObject AddObjectToDatabase(string model, WorldObjectType type, Vector3 pos, Vector3 rot, Vector3 scale)
        {
            var obj = new WorldObject
            {
                modelName = model,
                type = type
            };

            obj.SetPosition(pos);
            obj.SetScale(scale);
            obj.SetRotation(rot);

            Server.database.Insert(obj);

            return obj;
        }


    }
}

