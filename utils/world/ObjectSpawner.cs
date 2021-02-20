using Godot;
using System.Collections.Generic;
using System.Linq;

namespace Game
{
    public class ObjectSpawner : ObjectSpawnerBase
    {
        [Export]
        public float MinDistanceToPlayer = 350f;

        [Export]
        public int scanTimeInMs = 100;

        public bool scanThreadIsRunning = false;

        public System.Threading.Thread scanThread = null;


        public NetworkPlayer player = null;

        public bool isObjectDatabaseSynced = false;


        public override void _ExitTree()
        {
            base._ExitTree();

            if (scanThread != null)
            {
                scanThreadIsRunning = false;
                scanThread.Abort();
            }
        }

        public void InitObjectList(List<WorldObject> objects, Vector3 spawnPoint)
        {
            GD.Print("[Client] Creating world objects");

            isObjectDatabaseSynced = false;
            tempObjects = objects;

            var objectsInSpawnArea = findObjectsByPosition(spawnPoint, MinDistanceToPlayer);

            foreach (var x in objectsInSpawnArea)
            {
                CreateWorldObject(x);
            }

            isObjectDatabaseSynced = true;
        }
        public void startScanThread()
        {
            scanThreadIsRunning = true;
            scanThread = new System.Threading.Thread(scanPlayersInObjectArea);
            scanThread.Start();
        }

        protected void scanPlayersInObjectArea()
        {
            while (scanThreadIsRunning)
            {
                //be sure that we can grad the next object snapshot
                if (deleteChildsQueue.Count <= 0 && createChildsQueue.Count <= 0 && creationInProgress.Count <= 0)
                {
                    //find all objects in close area
                    var currentSceneObjects = findObjectsInNearOfPlayer(player, MinDistanceToPlayer);


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

        public bool allInitObjectsCreated()
        {
            return (createChildsQueue.Count == 0 && creationInProgress.Count == 0 && isObjectDatabaseSynced == true);
        }

        public void AskToCreate(string model, WorldObjectType type, Vector3 pos, Vector3 rot, Vector3 scale)
        {
            RpcId(1, "AddObject", model, type, pos, rot, scale);
        }

        [Puppet]
        private void OnNewObject(string objectJson)
        {
            var obj = Networking.NetworkCompressor.Decompress<WorldObject>(objectJson);
            tempObjects.Add(obj);

            //create object synced
        }
    }
}

