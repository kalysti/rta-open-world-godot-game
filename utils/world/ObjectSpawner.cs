using Godot;
using System.Collections.Generic;
using System.Linq;

namespace Game
{
    public class ObjectSpawner : ObjectSpawnerBase
    {


        [Export]
        public float SyncTime = 0.5f;

        private float syncedTime = 0.0f;

        private bool onUpdate = false;
        private bool initalize = false;

        public int totalObjects = 0;

        public int objectsDrawin
        {
            get
            {
                return GetChildCount();
            }
        }

        public void setInit(bool _init)
        {
            initalize = _init;
        }

        public override void _Process(float delta)
        {
            base._Process(delta);

            if (!initalize)
                return;

            if (syncedTime >= SyncTime)
            {
                if (!onUpdate)
                    RpcUnreliableId(1, "GetObjectList", (GetParent() as World).player.GetPlayerPosition(), ObjectDistance);
                syncedTime = 0.0f;
            }

            syncedTime += delta;
        }

        public void AskToCreate(string model, WorldObjectType type, Vector3 pos, Vector3 rot)
        {
            RpcId(1, "AddObject", model, type, pos, rot);
        }

        [Puppet]
        private void UpdateClientObjectList(string json, int _totalObjects)
        {
            if (onUpdate)
                return;

            onUpdate = true;
            var objects = Networking.NetworkCompressor.Decompress<List<WorldObject>>(json);
            CreateObjectsFromList(objects);
            totalObjects = _totalObjects;
            onUpdate = false;
        }

        private void CreateObjectsFromList(List<WorldObject> _list)
        {
            var ids = _list.Select(c => c.Id.ToString()).ToList();
            _spawnQueue.Clear();

            foreach (var obj in _list)
            {
                var exist = GetNodeOrNull(obj.Id.ToString());
                if (exist != null)
                    continue;
                else
                {
                    _spawnQueue.Enqueue(obj);
                }
            }

            foreach (WorldObjectNode existObj in GetChildren())
            {
                if (!ids.Contains(existObj.Name))
                {
                    _nodeToDelete.Enqueue(existObj);
                }
            }
        }

        [Puppet]
        private void OnNewObject(string objectJson)
        {
            var obj = Networking.NetworkCompressor.Decompress<WorldObject>(objectJson);
            _spawnQueue.Enqueue(obj);
        }
    }
}

