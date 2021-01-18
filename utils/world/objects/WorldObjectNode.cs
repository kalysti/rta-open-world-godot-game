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

        [Signal]
        public delegate void objectCreated();

        public  void CreateChild(Node instance)
        {
              AddChild(instance);
              EmitSignal(nameof(objectCreated));
        }
        public override void _Ready()
        {
            SetObjectPosition();
            SetObjectRotation();
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

        private bool LoadScene(string path)
        {
            if (!ResourceLoader.Exists(path))
            {
                GD.PrintErr("[Spawner] Cant find: " + worldObject.modelName);
                return false;
            }

            var scene = (PackedScene)ResourceLoader.Load(path);

            if (worldObject.type == WorldObjectType.PROPERTY)
            {
                holdedObject = (Spatial)scene.Instance();

                CallDeferred("CreateChild", holdedObject);
            }
            else if (worldObject.type == WorldObjectType.VEHICLE)
            {
                holdedObject = (Vehicle)scene.Instance();

                CallDeferred("CreateChild", holdedObject);
            }
            else if (worldObject.type == WorldObjectType.MARKER)
            {
                holdedObject = (Spatial)scene.Instance();
                CallDeferred("CreateChild", holdedObject);
            }

            return true;
        }


        public bool LoadObjectByFilePath()
        {
            if (worldObject.type == WorldObjectType.PROPERTY)
            {
                return LoadScene("res://objects/" + worldObject.modelName + ".tscn");

            }
            else if (worldObject.type == WorldObjectType.VEHICLE)
            {
                return LoadScene("res://vehicles/" + worldObject.modelName + ".tscn");

            }
            else if (worldObject.type == WorldObjectType.MARKER)
            {
                return LoadScene("res://utils/world/objects/marker/marker.tscn");
            }
            else
            {
                GD.PrintErr("[Spawner] Cant find type: " + worldObject.type.ToString());
                return false;
            }


        }
    }
}

