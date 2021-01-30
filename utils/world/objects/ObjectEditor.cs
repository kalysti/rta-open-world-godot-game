using Godot;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace Game
{
    public class ObjectEditor : CanvasLayer
    {
        private ItemList objectList = null;
        private ItemList vehicleList = null;
        private WindowDialog dialog = null;

        private Game.WorldObjectNode newWorldObject = null;
        private bool saveObject = false;
        private World world = null;
        private Player player = null;

        [Export]
        private float rotationMultiplier = 1.0f;

        private List<string> tempItemList = new List<string>();
        private List<string> tempVehicleList = new List<string>();

        private string currentObjectName = null;
        private string currentVehicleName = null;

        private BackgroundLoader backgroundLoader;

        public override void _Ready()
        {

            backgroundLoader = GetTree().Root.GetNode("BackgroundLoader") as BackgroundLoader;

            dialog = GetNode("dialog") as WindowDialog;
            world = GetParent().GetParent().GetParent() as World;
            player = GetParent() as Player;

            dialog.Connect("popup_hide", this, "onClose");
            dialog.GetCloseButton().Connect("pressed", this, "onClose");
            dialog.FindNode("add_object").Connect("pressed", this, "addObject");
            dialog.FindNode("add_vehicle").Connect("pressed", this, "addVehicle");

            objectList = dialog.FindNode("object_list") as ItemList;
            objectList.Connect("item_selected", this, "onObjectSelected");

            vehicleList = dialog.FindNode("vehicle_list") as ItemList;
            vehicleList.Connect("item_selected", this, "onVehicleSelected");

            (dialog.FindNode("vehicle_search_box") as LineEdit).Connect("text_changed", this, "onVehicleSearch");
            (dialog.FindNode("object_search_box") as LineEdit).Connect("text_changed", this, "onObjectSearch");

            tempItemList = scanDir("res://objects/");
            tempItemList.Sort();

            foreach (var item in tempItemList)
            {
                objectList.AddItem(item.Replace("res://objects/", "").TrimStart('/'));
            }

            tempVehicleList = scanDir("res://vehicles/");
            tempVehicleList.Sort();

            foreach (var item in tempVehicleList)
            {
                vehicleList.AddItem(item.Replace("res://vehicles/", "").TrimStart('/'));
            }
        }

        public void onVehicleSearch(string search)
        {
            vehicleList.Clear();
            if (string.IsNullOrEmpty(search))
            {
                foreach (var item in tempVehicleList)
                {
                    vehicleList.AddItem(item.Replace("res://vehicles/", "").TrimStart('/'));
                }
            }
            else
            {
                foreach (var item in tempItemList.Where(tf => tf.ToLower().Contains(search.ToLower())))
                {
                    vehicleList.AddItem(item.Replace("res://vehicles/", "").TrimStart('/'));
                }
            }
        }
        public void onObjectSearch(string search)
        {
            objectList.Clear();
            if (string.IsNullOrEmpty(search))
            {
                foreach (var item in tempItemList)
                {
                    objectList.AddItem(item.Replace("res://objects/", "").TrimStart('/'));
                }
            }
            else
            {
                foreach (var item in tempItemList.Where(tf => tf.ToLower().Contains(search.ToLower())))
                {
                    objectList.AddItem(item.Replace("res://objects/", "").TrimStart('/'));
                }
            }
        }
        public void onObjectSelected(int selected)
        {
            var item = objectList.GetItemText(selected);

            if (currentObjectName != item)
            {
                currentObjectName = item;
                showObject(item, WorldObjectType.PROPERTY);
            }
        }

        public void onVehicleSelected(int selected)
        {
            var item = vehicleList.GetItemText(selected);

            if (currentObjectName != item)
            {
                currentObjectName = item;
                showObject(item, WorldObjectType.VEHICLE);
            }
        }

        private void showObject(string name, WorldObjectType type)
        {
            var wo = new WorldObject
            {
                type = type,
                modelName = name,
            };

            wo.SetScale(new Vector3(1, 1, 1));
            var loader = new BackgroundLoaderObjectItem(wo);

            loader.OnLoaderComplete += OnShowObjectIsReady;
            backgroundLoader.Load(loader);
        }

        public void OnShowObjectIsReady(Resource resource, WorldObject worldObject)
        {

            var oldObject = FindNode("object_holder");
            if (oldObject != null)
            {
                foreach (var x in oldObject.GetChildren())
                {
                    oldObject.RemoveChild(x as Node);
                }
            }

            var scene = GD.Load<PackedScene>("res://utils/world/objects/WorldObjectNode.tscn");
            var node = (WorldObjectNode)scene.Instance();
            node.enableLoding = false;
            node.worldObject = worldObject;
            node.Visible = true;
            node.Name = worldObject.Id.ToString();

            var arr = new Godot.Collections.Array();
            arr.Add(node);

            node.Connect("ObjectCreated", this, "previewObjectGenerated", arr);

            if (node.CreateScene(resource))
            {
                FindNode("object_holder").CallDeferred("add_child", node);
            }
        }

        public void previewObjectGenerated(WorldObjectNode node)
        {
            if(node.holdedObject is Vehicle)
            {
                (node.holdedObject as Vehicle).GravityScale = 0;
                (node.holdedObject as Vehicle).Mode = RigidBody.ModeEnum.Static;
            }
        }

        public void addObject()
        {
            foreach (var i in objectList.GetSelectedItems())
            {
                var item = objectList.GetItemText(i);
                dialog.Hide();
                DrawObject(item, WorldObjectType.PROPERTY);
                Input.SetMouseMode(Input.MouseMode.Captured);

                break;
            }
        }

        public void addVehicle()
        {
            foreach (var i in vehicleList.GetSelectedItems())
            {
                var item = vehicleList.GetItemText(i);
                dialog.Hide();
                DrawObject(item, WorldObjectType.VEHICLE);
                Input.SetMouseMode(Input.MouseMode.Captured);

                break;
            }
        }

        private bool objectLoaded = false;
       private void CreateException()
        {
            if (newWorldObject != null)
            {
                previewObjectGenerated(newWorldObject);
                AddException(newWorldObject);
                objectLoaded = true;
            }
        }
        private void AddException(Node newWorldObject)
        {
            foreach (var child in newWorldObject.GetChildren())
            {
                if (child is Node)
                {
                    var node = child as Node;
                    player.rayDrag.AddException(node);

                    if (node.GetChildCount() > 0)
                    {
                        AddException(node);
                    }
                }
            }
        }

        private void ClearException(Node newWorldObject)
        {
            foreach (var child in newWorldObject.GetChildren())
            {
                if (child is Node)
                {
                    var node = child as Node;

                    player.rayDrag.RemoveException(node);

                    if (node.GetChildCount() > 0)
                    {
                        ClearException(node);
                    }
                }
            }
        }

        public void Show()
        {
            Input.SetMouseMode(Input.MouseMode.Visible);
            (GetNode("dialog") as WindowDialog).Show();
        }

        public void onClose()
        {
            Input.SetMouseMode(Input.MouseMode.Captured);
        }

        public override void _Process(float delta)
        {
            if (newWorldObject == null || !newWorldObject.IsInsideTree())
                return;

            if (Input.GetMouseMode() != Input.MouseMode.Visible)
                MoveObject();

            if (Input.IsActionJustPressed("lmb") && Input.GetMouseMode() != Input.MouseMode.Visible)
                RequestAddObject();

            if (Input.IsActionJustPressed("rmb") && Input.GetMouseMode() != Input.MouseMode.Visible)
                CancelRequest();
        }

        private List<string> scanDir(string path)
        {
            string file_name = null;
            var files = new List<string>();
            var dir = new Directory();
            var error = dir.Open(path);
            if (error != Error.Ok)
            {
                GD.PrintErr("Can't open " + path + "!");
                return new List<string>();
            }

            dir.ListDirBegin(true);
            file_name = dir.GetNext();
            while (file_name != "")
            {
                if (dir.CurrentIsDir())
                {
                    var new_path = path + "/" + file_name;
                    files.AddRange(scanDir(new_path));
                }
                else
                {
                    var name = path + "/" + file_name;
                    if (file_name.Contains(".tscn"))
                        files.Add(name.Replace(".tscn", ""));
                }
                file_name = dir.GetNext();
            }

            dir.ListDirEnd();
            return files;
        }
        private void DrawObject(string modelName, Game.WorldObjectType type)
        {
            if (newWorldObject != null)
                return;

            objectLoaded = false;

            var obj = new Game.WorldObject
            {
                type = type,
                modelName = modelName
            };

            var bgloader = new BackgroundLoaderObjectItem(obj);
            bgloader.OnLoaderComplete += onDrawObjectFinish;
            backgroundLoader.Load(bgloader);
        }
        public void onDrawObjectFinish(Resource res, WorldObject obj)
        {
            newWorldObject = new Game.WorldObjectNode
            {
                worldObject = obj
            };

            newWorldObject.enableLoding = false;
            newWorldObject.Visible = false;
            newWorldObject.Connect("ObjectCreated", this, "CreateException");
            newWorldObject.CreateScene(res, true);
            newWorldObject.Name = "temp_object";
            
            saveObject = false;
            world.AddChild(newWorldObject);
            
            objectLoaded = true;
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventMouseButton)
            {
                InputEventMouseButton emb = (InputEventMouseButton)@event;
                if (emb.IsPressed())
                {
                    if (emb.ButtonIndex == (int)ButtonList.WheelUp)
                    {
                        scrollObject(-1);
                    }
                    if (emb.ButtonIndex == (int)ButtonList.WheelDown)
                    {
                        scrollObject(1);
                    }
                }
            }
        }

        private void scrollObject(int rorator)
        {
            if (newWorldObject == null || !newWorldObject.IsInsideTree())
                return;

            var rot = newWorldObject.RotationDegrees;
            rot.y += rorator * rotationMultiplier;
            newWorldObject.RotationDegrees = rot;
        }
        private void CancelRequest()
        {
            if (newWorldObject != null)
            {
                newWorldObject.QueueFree();
            }

            ClearException(newWorldObject);
            newWorldObject = null;
            saveObject = false;
            player.onFocusing = false;

            //go back to dialog
            Input.SetMouseMode(Input.MouseMode.Visible);
            dialog.Show();
        }
        private void RequestAddObject()
        {
            GD.Print("press release");
            if (newWorldObject != null)
            {
                if (saveObject)
                {
                    var modelName = newWorldObject.worldObject.modelName;
                    var modelType = newWorldObject.worldObject.type;
                    var position = newWorldObject.GlobalTransform.origin;
                    var rotation = newWorldObject.Rotation;
                    var scale = newWorldObject.Scale;

                    world.spawner.AskToCreate(modelName, modelType, position, rotation, scale);
                }
            }

            CancelRequest();
        }

        private void MoveObject()
        {
            player.onFocusing = true;

            if (player.rayDrag.IsColliding() && objectLoaded)
            {
                var raycast_result = player.rayDrag.GetCollider();
                if (raycast_result != newWorldObject)
                {
                    if ((raycast_result is Spatial && (raycast_result as Spatial).Name == "terrain") || raycast_result is StaticBody)
                    {
                        var gt = newWorldObject.GlobalTransform;
                        gt.origin = player.rayDrag.GetCollisionPoint();
                        newWorldObject.GlobalTransform = gt;
                        newWorldObject.Visible = true;
                        saveObject = true;

                        return;
                    }
                }

            }

            newWorldObject.Visible = false;
            saveObject = false;
        }

    }
}