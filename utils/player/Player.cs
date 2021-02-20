using Godot;
using System;
using System.Linq;
using Game;
using Newtonsoft.Json;

using System.Collections.Generic;
namespace Game
{
    public class Player : NetworkPlayer
    {
        const float MOTION_INTERPOLATE_SPEED = 15.0f;
        const float CAMERA_ROTATION_SPEED = 0.001f;
        const float CAMERA_X_ROT_MIN = -80;
        const float CAMERA_X_ROT_MAX = 80;

        [Export]
        public float mouseSens = 12f;

        public bool inputEnabled = true;

        public Vehicle vehicle = null;

        [Export]
        public float InputPredictionMaxSize = 0.25f;

        [Export]
        public bool posCorrection = true;

        [Export]
        public NodePath rayDragPath;

        [Export]
        public int InputPredictionMaxPackages = 1024;

        [Export]
        public NodePath cameraPath;

        [Export]
        public NodePath cameraBasePath;

        [Export]
        public NodePath targetPath;

        [Export]
        public NodePath crosshairPath;

        [Export]
        public NodePath objectEditorPath;

        public ClippedCamera camera { get; set; }
        public Spatial cameraBase { get; set; }
        public Spatial target { get; set; }
        public Control crosshair { get; set; }
        public RayCast rayDrag { get; set; }

        protected NetworkPlayerChar character { get; set; }

        [Export]
        public NodePath characterPath;

        private float fov_initial = 0f;
        private Vector3 camera_target_initial = Vector3.Zero;
        private Transform origCameraTransform;

        private ObjectEditor objectEditor;


        private float camera_x_rot = 0.0f;
        private float camera_y_rot = 0.0f;

        private Timer stateTimer = new Timer();

        private bool reProcess = false;

        public bool onFocusing = false;


        public List<FrameSnapshot> snapshots = new List<FrameSnapshot>();
        public List<FrameSnapshot> sendList = new List<FrameSnapshot>();

        public Queue<PlayerInput> inputQueue = new Queue<PlayerInput>();
        private PlayerInput lastInput = null;
        public override void _Ready()
        {
            InitPlayer();

            target = (Spatial)GetNode(targetPath);

            camera = (ClippedCamera)GetNode(cameraPath);
            cameraBase = (Spatial)GetNode(cameraBasePath);

            crosshair = (Control)GetNode(crosshairPath);
            character = (NetworkPlayerChar)GetNode(characterPath);
            rayDrag = (RayCast)GetNode(rayDragPath);
            objectEditor = (ObjectEditor)GetNode(objectEditorPath);

            camera_target_initial = target.Transform.origin;
            origCameraTransform = camera.Transform;

            fov_initial = camera.Fov;

            (FindNode("vehicle_hud") as Control).Visible = false;

            if (onlineCharacter != null)
            {
                var reciepe = JsonConvert.DeserializeObject<UMAReciepe>(onlineCharacter.body);

                if (reciepe != null)
                    character.initCharacter(reciepe.isMale, reciepe);
            }
        }

        public override void _PhysicsProcess(float delta)
        {


            if (Input.IsActionJustReleased("enter_menu") && Input.GetMouseMode() != Input.MouseMode.Visible)
            {
                requestEnterVehicle();
            }

            if (Input.IsActionJustReleased("start_engine") && Input.GetMouseMode() != Input.MouseMode.Visible)
            {
                requestStartEngine();
            }

            if (Input.IsActionJustReleased("change_camera") && Input.GetMouseMode() != Input.MouseMode.Visible)
            {
                if (cameraMode == PlayerCameraMode.FIRST_PERSON)
                    cameraMode = PlayerCameraMode.THIRD_PERSON;
                else if (cameraMode == PlayerCameraMode.THIRD_PERSON)
                    cameraMode = PlayerCameraMode.FIRST_PERSON;

                setCameraMode();
            }

            (FindNode("vehicle_hud") as Control).Visible = playerState.inVehicle;

            //process input
            inputQueue.Enqueue(GetInput(delta));

            while (inputQueue.Count > 0)
            {
                var newInput = inputQueue.Dequeue();

                if (newInput != null)
                {
                    if (playerState.inVehicle)
                    {
                        (FindNode("gear_label") as Label).Text = vehicle.getCurrentGear().ToString();
                        (FindNode("speed_label") as Label).Text = vehicle.getKmPerHour().ToString() + " km/h";
                    }

                    lastInput = ProcessInput(newInput, lastInput, delta);
                    character.ProcessAnimation(playerState, newInput, delta);
                }
            }

            if (sendList.Count() >= 10)
            {
                var list = sendList.GetRange(0, 10);
                sendList.RemoveRange(0, 10);

                SendInputToServer(list);
            }
        }

        private PlayerInput ProcessInput(PlayerInput newInput, PlayerInput oldInput, float delta)
        {

            if (oldInput != null)
            {
                newInput.velocity = oldInput.velocity;
            }

            if (playerState.inVehicle)
            {
                //just save snapshot and send it to server, because server is calc the movement of the vehicle
            }
            else
            {
                newInput.velocity = DoWalk(CalculateVelocityByInput(newInput, delta));
            }

            var snap = SaveSnapshot(newInput);

            return newInput;
        }


        public override void _Process(float delta)
        {
            networkStats.loop();
        }

        private void SendInputToServer(List<FrameSnapshot> snapshot)
        {
            var snap = Game.Networking.NetworkCompressor.Compress(snapshot);
            networkStats.AddPackage(snap);

            RpcUnreliableId(1, "OnNewInputSnapshot", snap);
        }
        private FrameSnapshot SaveSnapshot(PlayerInput state)
        {
            var snapshot = new FrameSnapshot();
            snapshot.movementState = state;
            snapshot.timestamp = OS.GetTicksMsec();

            snapshot.origin = GetPlayerPosition();
            snapshot.rotation = GetPlayerRotation();

            snapshots.Add(snapshot);
            sendList.Add(snapshot);

            //clear snapshots after
            if (snapshots.Count() > InputPredictionMaxPackages)
                snapshots.RemoveAt(0);


            return snapshot;
        }

        [Puppet]
        public void OnServerSnapshot(string correctedSnapshotJson)
        {
            networkStats.AddInPackage(correctedSnapshotJson);

            if (reProcess == true || !posCorrection)
                return;

            var correctedSnapshot = Game.Networking.NetworkCompressor.Decompress<FrameSnapshot>(correctedSnapshotJson);
            var oldSnapshot = snapshots.Find(t => t.timestamp == correctedSnapshot.timestamp);

            //remove all older then corrected snapshot and it to
            if (oldSnapshot != null)
            {
                var snapshotList = snapshots.Where(tf => tf.timestamp > correctedSnapshot.timestamp).ToList();

                var orig1 = oldSnapshot.origin;
                var orig2 = correctedSnapshot.origin;


                //draw server side mesh

                var td = (GetNode("server_side") as MeshInstance).GlobalTransform;
                td.origin = correctedSnapshot.origin;
                (GetNode("server_side") as MeshInstance).GlobalTransform = td;

                var td2 = (GetNode("client_side") as MeshInstance).GlobalTransform;
                td2.origin = oldSnapshot.origin;
                (GetNode("client_side") as MeshInstance).GlobalTransform = td2;


                if ((orig1 - orig2).Length() > InputPredictionMaxSize)
                {

                    inputQueue.Clear();
                    reProcess = true;

                    //roleback

                    GD.Print("[CLIENT] Player  correction for " + orig1 + " - " + orig2 + "  by " + (orig1 - orig2).Length());

                    SetPlayerPosition(correctedSnapshot.origin);
                    SetPlayerRotation(correctedSnapshot.rotation);


                    lastInput = correctedSnapshot.movementState;
                    snapshotList.Reverse();

                    foreach (var lostSnapshot in snapshotList)
                    {
                        inputQueue.Enqueue(lostSnapshot.movementState);
                    }

                    reProcess = false;
                }

                snapshots.RemoveAll(tf => tf.timestamp <= correctedSnapshot.timestamp);
            }
            else if (snapshots.Count > 1)
            {
                snapshots.Clear();


                GD.Print("[Client] Desyncing");

                SetPlayerPosition(correctedSnapshot.origin);
                SetPlayerRotation(correctedSnapshot.rotation);


                lastInput = correctedSnapshot.movementState;
            }
        }

        private PlayerInput GetInput(float delta)
        {

            if (inputEnabled == false)
                return null;

            var movementInput = new PlayerInput();

            movementInput.cam_direction = Vector3.Zero;
            movementInput.movement_direction = Vector2.Zero;

            var headBasis = camera.GlobalTransform.basis;


            if (Input.IsActionJustReleased("editor") && Input.GetMouseMode() != Input.MouseMode.Visible)
            {
                objectEditor.Show();
            }

            if (Input.IsActionPressed("move_forward") && Input.GetMouseMode() != Input.MouseMode.Visible)
                movementInput.movement_direction.y += 1;
            if (Input.IsActionPressed("move_backward") && Input.GetMouseMode() != Input.MouseMode.Visible)
                movementInput.movement_direction.y -= 1;
            if (Input.IsActionPressed("move_left") && Input.GetMouseMode() != Input.MouseMode.Visible)
                movementInput.movement_direction.x -= 1;
            if (Input.IsActionPressed("move_right") && Input.GetMouseMode() != Input.MouseMode.Visible)
                movementInput.movement_direction.x += 1;


            movementInput.movement_direction = movementInput.movement_direction.Normalized();

            movementInput.cam_direction += -headBasis.z * movementInput.movement_direction.y;
            movementInput.cam_direction += headBasis.x * movementInput.movement_direction.x;

            //movementInput.cam_direction.y = 0; //dont need on fps?

            movementInput.cam_direction = movementInput.cam_direction.Normalized();

            //sprint
            if (Input.IsActionPressed("move_sprint"))
                movementInput.isSprinting = true;
            else
                movementInput.isSprinting = false;

            //jumping
            if (Input.IsActionPressed("move_jump"))
                movementInput.isJumping = true;
            else
                movementInput.isJumping = false;

            //jumping
            if (Input.IsActionJustReleased("map"))
            {
                (GetNode("hud/radar_map") as RadarMap).Visible = !(GetNode("hud/radar_map") as RadarMap).Visible;
            }

            //aiming
            var camera_target = camera_target_initial;
            var fov = fov_initial;
            var crosshair_alpha = 0.0f;

            if (Input.IsActionPressed("rmb") || onFocusing)
            {
                camera_target.x = -1.25f;
                crosshair_alpha = 1.0f;
                fov = 60;
                movementInput.isAiming = true;
            }

            if (Input.IsActionJustReleased("rmb"))
            {
                movementInput.isAiming = false;
            }

            var crmod = crosshair.Modulate;
            crmod.a += (crosshair_alpha - crosshair.Modulate.a) * 0.15f;
            crosshair.Modulate = crmod;

            var tf = target.Transform;
            tf.origin.x += (camera_target.x - target.Transform.origin.x) * 0.15f;
            target.Transform = tf;
            camera.Fov += (fov - camera.Fov) * 0.15f;

            // Force
            if (movementInput.isAiming && !playerState.weaponEquipped)
            {
                var space_state = GetWorld().DirectSpaceState;
                var center_position = GetViewport().Size / 2;
                var ray_from = camera.ProjectRayOrigin(center_position);
                var ray_to = ray_from + camera.ProjectRayNormal(center_position) * GRAB_DISTANCE;

                var arr = new Godot.Collections.Array();
                arr.Add(this);

                Godot.Collections.Dictionary ray_result = space_state.IntersectRay(ray_from, ray_to, arr);
                /*
                if (ray_result != null && ray_result.Contains("collider"))
                {
                    var body = ray_result["collider"];
                    if (body is RigidBody)
                    {
                        if (Input.IsActionJustPressed("lmb") && playerState.onGround)
                        {
                            (body as RigidBody).ApplyImpulse(new Vector3(0, 0, 0), -camera.GlobalTransform.basis.z.Normalized() * THROW_FORCE);
                        }
                    }
                }
                */
            }

            return movementInput;
        }

        [Puppet]
        private void enterVehicle(int vehicleId)
        {
            var vehicleNode = (world as World).spawner.GetNodeOrNull<WorldObjectNode>(vehicleId.ToString());
            if (vehicleNode != null)
            {
                vehicle = vehicleNode.holdedObject as Vehicle;
                shape.Disabled = true;
                vehicle.driver = this;
                playerState.inVehicle = true;
                camera.ClipToBodies = false;

                setCameraMode();
            }
        }


        [Puppet]
        private void leaveVehicle()
        {
            GD.Print("[Client] Leave vehicle ");

            if (vehicle != null)
            {
                disableVehicleMode(vehicle);
                vehicle = null;

                camera.ClipToBodies = true;
                setCameraMode();
            }
        }

        [Puppet]
        private void setVehicleEngine(bool engine)
        {
            if (vehicle != null)
            {
                GD.Print("[Client] Start vehicle engine");
                vehicle.engineStarted = !engine;
                vehicle.StartEngine();
            }
        }

        private void requestStartEngine()
        {
            if (vehicle != null && playerState.inVehicle)
                RpcId(1, "requestStartStopEngine");
        }

        private void requestEnterVehicle()
        {
            var vehicleId = -1;

            rayDrag.AddException(world.getMapTerrain());

            if (rayDrag.IsColliding())
            {
                if (rayDrag.GetCollider() != null && rayDrag.GetCollider() is Vehicle)
                {
                    var node = rayDrag.GetCollider() as Vehicle;
                    vehicleId = node.vehicleId;
                }
            }

            rayDrag.RemoveException(world.getMapTerrain());
            RpcId(1, "requestVehicle", vehicleId);
        }

        protected void setCameraMode()
        {
            var t = Vector3.Zero;
            var cameraRotation = (cameraBase.FindNode("rotation") as Spatial);

            cameraRotation.RotationDegrees = Vector3.Zero;
            cameraBase.RotationDegrees = Vector3.Zero;

            if (cameraMode == PlayerCameraMode.THIRD_PERSON)
            {
                if (playerState.inVehicle)
                {
                    t.x = -0.4f;
                    t.y = 1.4f;
                    t.z = -3f;
                }
                else
                {

                    t.z = -0.6f;
                    t.y = 0.7f;
                }
                camera.Translation = t;
            }
            else if (cameraMode == PlayerCameraMode.FIRST_PERSON)
            {
                if (playerState.inVehicle)
                {

                    t.z = 1.1f;
                    t.y = 0.6f;
                }
                else
                {

                    t.z = 0.8f;
                    t.y = 0.8f;
                }

                camera.Translation = t;
            }

            var showChar = (cameraMode == PlayerCameraMode.THIRD_PERSON);
            var charnode = shape.GetNodeOrNull<NetworkPlayerChar>("char");
            if (charnode != null)
            {
                charnode.Visible = showChar;
            }
        }

        public override void _Input(InputEvent @event)
        {
            base._Input(@event);

            var mouseSense = Mathf.Clamp(mouseSens, -50, 50) / 100;

            if (@event is InputEventMouseMotion && Input.GetMouseMode() == Input.MouseMode.Captured)
            {
                var mot = @event as InputEventMouseMotion;
                var cameraRotation = (cameraBase.FindNode("rotation") as Spatial);

                if (cameraMode == PlayerCameraMode.THIRD_PERSON)
                {
                    cameraBase.RotateY(Mathf.Deg2Rad(-mot.Relative.x * mouseSense));
                    cameraBase.Orthonormalize();

                    camera_x_rot = Mathf.Clamp(camera_x_rot + Mathf.Deg2Rad(mot.Relative.y * mouseSense), Mathf.Deg2Rad(CAMERA_X_ROT_MIN), Mathf.Deg2Rad(CAMERA_X_ROT_MAX));

                    var newRot = cameraRotation.Rotation;
                    newRot.x = camera_x_rot;
                    cameraRotation.Rotation = newRot;
                }
                else if (cameraMode == PlayerCameraMode.FIRST_PERSON)
                {
                    if (mot.Relative.Length() > 0)
                    {
                        var horizontal = -mot.Relative.x * (mouseSens / 100);
                        var vertical = mot.Relative.y * (mouseSens / 100);

                        cameraBase.RotateY(Mathf.Deg2Rad(horizontal));
                        cameraRotation.RotateX(Mathf.Deg2Rad(vertical));

                        var temp_rot = cameraRotation.RotationDegrees;
                        temp_rot.x = Mathf.Clamp(temp_rot.x, -90, 90);
                        cameraRotation.RotationDegrees = temp_rot;
                    }
                }

            }
        }

    }
}