using System;
using Godot;
using SQLite;
using System.Linq;
using System.Reflection;

namespace Game
{

    public class WorldObjectNode : Spatial
    {
        public WorldObject worldObject = null;

        private ResourceInteractiveLoader loader = null;

        private PackedScene packedLoadedScene = null;

        [Export]
        public bool enableLoding = true;

        [Export]
        public bool showName = false;

        [Signal]
        public delegate void ObjectCreated();

        public bool isServer = false;



        public override void _Ready()
        {
            SetObjectRotation();
            SetObjectScale();
            SetObjectPosition();

            if (GetNodeOrNull("label") != null && worldObject != null)
            {
                GetNode("label").Call("set_text", worldObject.modelName + " - " + worldObject.Id);
                GetNode("label").Set("visible", showName);
            }
        }

        private void findLods(Node n)
        {
            foreach (var item in n.GetChildren())
            {
                if (item is MeshLod)
                {
                    (item as MeshLod).enableLoding = false;
                }
                else if (item is Node)
                {
                    if ((item as Node).GetChildCount() > 0)
                    {
                        findLods(item as Node);
                    }
                }
            }
        }

        public void AddObjectToScene(Node n)
        {
            if (!enableLoding)
            {
                findLods(n);
            }
            AddChild(n);
            EmitSignal(nameof(ObjectCreated));
        }


        public Node holdedObject = null;

        public string getUniqIdent()
        {
            return worldObject.modelName + "_" + GlobalTransform.origin.x + "_" + GlobalTransform.origin.y + "_" + GlobalTransform.origin.z;
        }

        public void SetObjectPosition()
        {
            var gt = GlobalTransform;
            gt.origin = worldObject.GetPosition();
            GlobalTransform = gt;
        }

        public void SetObjectRotation()
        {
            Rotation = worldObject.GetRotation();
        }

        public void SetObjectScale()
        {
            Scale = worldObject.GetScale();
        }

        private void copyArgs(BaseVehicle _in, Node _out)
        {
            var properties = _in.GetPropertyList();

            foreach (Godot.Collections.Dictionary prop in properties)
            {
                var type = (PropertyUsageFlags)prop["usage"];
                if ((int)type == 8199) //key of exported vars
                {
                    _out.Set(prop["name"].ToString(), _in.Get(prop["name"].ToString()));
                }
            }
        }

        public bool CreateScene(Resource res, bool setDefaultScale = false)
        {
            var scene = (PackedScene)res;

            if (worldObject.type == WorldObjectType.PROPERTY)
            {
                var loadedScene = (Spatial)scene.Instance();
                loadedScene.Translation = Vector3.Zero;
                loadedScene.Rotation = Vector3.Zero;
                if (!setDefaultScale)
                    loadedScene.Scale = new Vector3(1, 1, 1);
                else
                {
                    worldObject.SetScale(loadedScene.Scale);
                    loadedScene.Scale = new Vector3(1, 1, 1);
                    Scale = worldObject.GetScale();
                }
                holdedObject = loadedScene;

                CallDeferred("AddObjectToScene", holdedObject);
            }
            else if (worldObject.type == WorldObjectType.VEHICLE)
            {
                Node baseScene = (BaseVehicle)scene.Instance();
                BaseVehicle baseScene2 = (BaseVehicle)scene.Instance();
                ulong objId = baseScene.GetInstanceId();

                BaseVehicle vehicle = null;

                if (isServer)
                {
                    baseScene.SetScript(GD.Load<Script>("res://utils/vehicle/ServerVehicle.cs"));
                    vehicle = (ServerVehicle)GD.InstanceFromId(objId);
                    copyArgs(baseScene2, vehicle);
                    baseScene2.Dispose();
                    baseScene2 = null;
                }
                else
                {
                    vehicle = (Vehicle)GD.InstanceFromId(objId);
                }

                vehicle.vehicleId = worldObject.Id;
                vehicle.Translation = Vector3.Zero;
                vehicle.Rotation = Vector3.Zero;

                if (!setDefaultScale)
                    vehicle.Scale = new Vector3(1, 1, 1);
                else
                {
                    worldObject.SetScale(vehicle.Scale);
                    vehicle.Scale = new Vector3(1, 1, 1);
                    Scale = worldObject.GetScale();
                }

                holdedObject = vehicle;

                CallDeferred("AddObjectToScene", holdedObject);
            }
            else if (worldObject.type == WorldObjectType.MARKER)
            {
                var loadedScene = (Spatial)scene.Instance();
                loadedScene.Translation = Vector3.Zero;
                loadedScene.Rotation = Vector3.Zero;
                if (!setDefaultScale)
                    loadedScene.Scale = new Vector3(1, 1, 1);
                else
                {
                    worldObject.SetScale(loadedScene.Scale);
                    loadedScene.Scale = new Vector3(1, 1, 1);
                    Scale = worldObject.GetScale();
                }
                holdedObject = loadedScene;

                CallDeferred("AddObjectToScene", holdedObject);
            }
            else if (worldObject.type == WorldObjectType.SYSTEM)
            {

                var loadedScene = (Spatial)scene.Instance();
                loadedScene.Translation = Vector3.Zero;
                loadedScene.Rotation = Vector3.Zero;
                if (!setDefaultScale)
                    loadedScene.Scale = new Vector3(1, 1, 1);
                else
                {
                    worldObject.SetScale(loadedScene.Scale);
                    loadedScene.Scale = new Vector3(1, 1, 1);
                    Scale = worldObject.GetScale();
                }
                holdedObject = loadedScene;

                CallDeferred("AddObjectToScene", holdedObject);
            }
            else
            {
                return false;
            }

            return true;
        }

    }
}

