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

        public override void _Ready()
        {
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
                var thread = new System.Threading.Thread(new ThreadStart(() => showObject(item)));
                thread.Start();

                currentObjectName = item;
            }
        }
        public void onVehicleSelected(int selected)
        {
            var item = vehicleList.GetItemText(selected);

            if (currentVehicleName != item)
            {
                var thread = new System.Threading.Thread(new ThreadStart(() => showVehicle(item)));
                thread.Start();

                currentVehicleName = item;
            }
        }

        private void showVehicle(string name)
        {
            var oldObject = FindNode("car_camera_viewport").GetNodeOrNull("vehicle_holder");
            if (oldObject != null)
                oldObject.QueueFree();

            if (!ResourceLoader.Exists("res://vehicles/" + name + ".tscn"))
            {
                GD.PrintErr("[Spawner] Cant find: " + name);
            }
            else
            {
                var nodeScene = (PackedScene)ResourceLoader.Load("res://vehicles/" + name + ".tscn");
                var spat = (Vehicle)nodeScene.Instance();
                spat.Name = "vehicle_holder";
                spat.GravityScale = 0;
                
                FindNode("car_camera_viewport").CallDeferred("add_child", spat);
            }

        }

        private void showObject(string name)
        {
            var oldObject = FindNode("camera_viewport").GetNodeOrNull("object_holder");
            if (oldObject != null)
                oldObject.QueueFree();

            if (!ResourceLoader.Exists("res://objects/" + name + ".tscn"))
            {
                GD.PrintErr("[Spawner] Cant find: " + name);
            }
            else
            {
                var nodeScene = (PackedScene)ResourceLoader.Load("res://objects/" + name + ".tscn");
                var spat = (Spatial)nodeScene.Instance();
                spat.Name = "object_holder";
                FindNode("camera_viewport").CallDeferred("add_child", spat);
            }
        }


        public void addVehicle()
        {
            foreach (var i in vehicleList.GetSelectedItems())
            {
                var item = vehicleList.GetItemText(i);
                dialog.Hide();
                DrawVehicle(item);
                Input.SetMouseMode(Input.MouseMode.Captured);

                break;
            }
        }

        public void addObject()
        {
            foreach (var i in objectList.GetSelectedItems())
            {
                var item = objectList.GetItemText(i);
                dialog.Hide();
                DrawObject(item);
                Input.SetMouseMode(Input.MouseMode.Captured);

                break;
            }
        }

        private void DrawVehicle(string modelName)
        {
            if (newWorldObject != null)
                return;

            newWorldObject = new Game.WorldObjectNode
            {
                worldObject = new Game.WorldObject
                {
                    type = Game.WorldObjectType.VEHICLE,
                    modelName = modelName
                }
            };

            newWorldObject.Name = "temp_object";
            saveObject = false;
            world.AddChild(newWorldObject);
            newWorldObject.LoadObjectByFilePath();

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

        public override void _PhysicsProcess(float delta)
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
        private void DrawObject(string modelName)
        {
            if (newWorldObject != null)
                return;

            newWorldObject = new Game.WorldObjectNode
            {
                worldObject = new Game.WorldObject
                {
                    type = Game.WorldObjectType.PROPERTY,
                    modelName = modelName
                }
            };


            newWorldObject.Name = "temp_object";
            saveObject = false;
            world.AddChild(newWorldObject);
            newWorldObject.LoadObjectByFilePath();

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

                    world.spawner.AskToCreate(modelName, modelType, position, rotation);
                }
            }

            CancelRequest();
        }

        private void MoveObject()
        {
            player.onFocusing = true;

            if (player.rayDrag.IsColliding())
            {
                newWorldObject.Visible = true;
                var raycast_result = player.rayDrag.GetCollider();

                if (raycast_result is Spatial && (raycast_result as Spatial).Name == "terrain")
                {
                    var gt = newWorldObject.GlobalTransform;
                    gt.origin = player.rayDrag.GetCollisionPoint();
                    newWorldObject.GlobalTransform = gt;
                    newWorldObject.Visible = true;

                    saveObject = true;
                }
                else
                {
                    newWorldObject.Visible = false;
                    saveObject = false;
                }
            }
            else
            {
                newWorldObject.Visible = false;
                saveObject = false;
            }
        }

    }
}