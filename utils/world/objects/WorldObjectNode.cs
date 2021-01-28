using System;
using Godot;
using SQLite;

namespace Game
{

    public class WorldObjectNode : Spatial
    {
        public WorldObject worldObject = null;

        private ResourceInteractiveLoader loader = null;

        private PackedScene packedLoadedScene = null;

        public override void _Ready()
        {
            SetObjectRotation();
            SetObjectScale();
            SetObjectPosition();

            if (GetNodeOrNull("label") != null && worldObject != null)
                GetNode("label").Call("set_text", worldObject.modelName + " - " + worldObject.Id);
        }

        public void disableLoding()
        {
            findLods(this);
        }

        private void findLods(Node n)
        {
            foreach (var item in n.GetChildren())
            {
                if (item is Node)
                {
                    if ((item as Node).GetChildCount() > 0)
                    {
                        findLods(item as Node);
                    }
                }

                if (item is MeshLod)
                {
                    (item as MeshLod).enableLoding = false;
                }

            }
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

        public bool CreateScene(Resource res)
        {
            var scene = (PackedScene)res;

            if (worldObject.type == WorldObjectType.PROPERTY)
            {
                var loadedScene = (Spatial)scene.Instance();
                loadedScene.Translation = Vector3.Zero;
                loadedScene.Rotation = Vector3.Zero;
                loadedScene.Scale = new Vector3(1, 1, 1);
                holdedObject = loadedScene;
                CallDeferred("add_child", holdedObject);
            }
            else if (worldObject.type == WorldObjectType.VEHICLE)
            {
                var loadedScene = (Vehicle)scene.Instance();
                loadedScene.Translation = Vector3.Zero;
                loadedScene.Rotation = Vector3.Zero;
                loadedScene.Scale = new Vector3(1, 1, 1);
                holdedObject = loadedScene;

                CallDeferred("add_child", holdedObject);
            }
            else if (worldObject.type == WorldObjectType.MARKER)
            {
                var loadedScene = (Spatial)scene.Instance();
                loadedScene.Translation = Vector3.Zero;
                loadedScene.Rotation = Vector3.Zero;
                loadedScene.Scale = new Vector3(1, 1, 1);
                holdedObject = loadedScene;

                CallDeferred("add_child", holdedObject);
            }
            else if (worldObject.type == WorldObjectType.SYSTEM)
            {

                var loadedScene = (Spatial)scene.Instance();
                loadedScene.Translation = Vector3.Zero;
                loadedScene.Rotation = Vector3.Zero;
                loadedScene.Scale = new Vector3(1, 1, 1);
                holdedObject = loadedScene;

                CallDeferred("add_child", holdedObject);
            }
            else
            {
                return false;
            }

            return true;
        }

    }
}

