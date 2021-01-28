using Godot;
using System.Collections.Generic;
using System.Linq;
namespace Game
{
    public class ObjectSpawnerBase : Spatial
    {
        [Signal]
        public delegate void objectsCreated();
        public BackgroundLoader backgroundLoader = null;

        protected List<WorldObject> tempObjects = new List<WorldObject>();

        protected Queue<WorldObjectNode> createChildsQueue = new Queue<WorldObjectNode>();
        protected Queue<WorldObjectNode> deleteChildsQueue = new Queue<WorldObjectNode>();

        protected List<WorldObject> creationInProgress = new List<WorldObject>();

        public List<WorldObject> allObjects()
        {
            return tempObjects;
        }

        protected List<WorldObject> findObjectsInNearOfPlayer(NetworkPlayer player, float distance)
        {
            return tempObjects.Where(tf => tf.GetPosition().DistanceTo(player.GetPlayerPosition()) <= distance).ToList();
        }

        protected List<WorldObject> findObjectsByPosition(Vector3 playerPos, float distance)
        {
            return tempObjects.Where(tf => tf.GetPosition().DistanceTo(playerPos) <= distance).ToList();
        }

        public int objectsDrawin
        {
            get
            {
                return GetChildCount();
            }
        }

        public int totalObjects
        {
            get
            {
                return tempObjects.Count;
            }
        }

        [Export]
        public float ObjectDistance = 500.0f;

        public override void _Ready()
        {
            backgroundLoader = GetTree().Root.GetNode("BackgroundLoader") as BackgroundLoader;
        }

        public override void _Process(float delta)
        {
            if (deleteChildsQueue.Count > 0)
            {
                try
                {
                    var child = deleteChildsQueue.Dequeue();
                    child.QueueFree();
                }
                catch
                {

                }
            }

            if (createChildsQueue.Count > 0)
            {
                var child = createChildsQueue.Dequeue();
                if (child != null)
                {
                    var exist = GetNodeOrNull(child.worldObject.Id.ToString()) != null ? true : false;

                    if (!exist)
                    {
                        CallDeferred("add_child", child);
                        child.CallDeferred("set_visible", true);
                    }
                }
            }
        }

        public void CreateWorldObject(WorldObject item)
        {
            if (!creationInProgress.Contains(item))
            {
                var exist = GetNodeOrNull(item.Id.ToString()) != null ? true : false;
                if (!string.IsNullOrEmpty(item.getResourcePath()) && !exist)
                {
                    var bgLoader = new BackgroundLoaderObjectItem(item);
                    bgLoader.OnLoaderComplete += onWorldObjectLoaded;
                    backgroundLoader.Load(bgLoader);
                }
            }
        }
        protected void onWorldObjectLoaded(Resource resource, WorldObject worldObject)
        {
            var scene = GD.Load<PackedScene>("res://utils/world/objects/WorldObjectNode.tscn");
            var node = (WorldObjectNode)scene.Instance();
            
            node.worldObject = worldObject;
            node.Visible = false;
            node.Name = worldObject.Id.ToString();

            if (node.CreateScene(resource))
            {
                creationInProgress.Remove(worldObject);
                createChildsQueue.Enqueue(node);
            }
        }

        public void clearMapObjects(BaseMap map)
        {
            GD.Print("[Server/Client] Clear exist scenes");

            foreach (var item in map.GetChildren())
            {
                if (item is RoadGridMap || item is VegetationSpawner)
                {
                    (item as Node).QueueFree();
                }
            }
        }

        public override void _ExitTree()
        {
            if (createChildsQueue.Count > 0)
                createChildsQueue.Clear();

            if (deleteChildsQueue.Count > 0)
                deleteChildsQueue.Clear();
        }
    }
}