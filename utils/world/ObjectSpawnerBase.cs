using Godot;
using System.Collections.Generic;

namespace Game
{
    public class ObjectSpawnerBase : Spatial
    {

        protected Queue<WorldObject> _spawnQueue = new Queue<WorldObject>();
        protected Queue<WorldObjectNode> _nodeToDelete = new Queue<WorldObjectNode>();

        protected System.Threading.Thread spawnThread;

        protected bool spawnThreadRunning = true;

        protected Mutex mut = new Mutex();

        [Export]
        public float ObjectDistance = 100.0f;
        public Player player = null;


        public override void _ExitTree()
        {
            base._ExitTree();

            spawnThreadRunning = false;
            if (spawnThread != null)
                spawnThread.Abort();
        }

        public void SetPlayer(Player _p)
        {
            player = _p;
        }
        public override void _Ready()
        {
            base._Ready();

            spawnThread = new System.Threading.Thread(queueSyncThread);
            spawnThread.Start();
        }


        public void queueSyncThread()
        {
            while (spawnThreadRunning)
            {
                mut.Lock();

                while (_spawnQueue.Count > 0)
                {
                    SpawnObject(_spawnQueue.Dequeue());
                }
                while (_nodeToDelete.Count > 0)
                {
                    var obj = _nodeToDelete.Dequeue();

                    if (obj.worldObject != null && obj.worldObject.type == WorldObjectType.VEHICLE && obj.holdedObject != null)
                    {
                        if (player != null && player.vehicle != null && player.vehicle == obj.holdedObject)
                        {
                            //do nothing in case its the vehicle of the player
                        }
                        else
                        {
                            obj.QueueFree();
                        }
                    }
                    else
                    {
                        obj.QueueFree();
                    }
                }
                mut.Unlock();

                //wait before call nex time
                System.Threading.Thread.Sleep(100);
            }
        }

        public WorldObjectNode SpawnObject(WorldObject spawn)
        {
            var node = new WorldObjectNode();
            node.worldObject = spawn;
            node.Name = spawn.Id.ToString();
            CallDeferred("add_child", node);


            bool result = node.LoadObjectByFilePath();
            if (result)
            {
                return node;
            }
            else
                return null;
        }

    }
}