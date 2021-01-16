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


        public bool inputEnabled = true;

        [Export]
        public float InputPredictionMaxSize = 0.1f;

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
        }


        public override void _PhysicsProcess(float delta)
        {
            //process input
            inputQueue.Enqueue(GetInput(delta));

            while (inputQueue.Count > 0)
            {
                var newInput = inputQueue.Dequeue();

                if (newInput != null)
                {
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
                newInput.velocity = oldInput.velocity;

            newInput.velocity = DoWalk(CalculateVelocityByInput(newInput, delta));

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

                    GD.Print("[CLIENT] Pos correction for " + orig1 + " - " + orig2 + "  by " + (orig1 - orig2).Length());

                    inputQueue.Clear();
                    reProcess = true;

                    //roleback
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
                GD.Print("[Client] Desyncing");

                snapshots.Clear();

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

            //cursor
            if (Input.IsActionJustPressed("ui_cancel"))
            {
                if (Input.GetMouseMode() == Input.MouseMode.Visible)
                    Input.SetMouseMode(Input.MouseMode.Captured);

                else
                    Input.SetMouseMode(Input.MouseMode.Visible);
            }

            movementInput.cam_direction = Vector3.Zero;
            movementInput.movement_direction = Vector2.Zero;

            var cam_xform = camera.GlobalTransform;

            if (Input.IsActionPressed("move_forward") && Input.GetMouseMode() != Input.MouseMode.Visible)
                movementInput.movement_direction.y += 1;
            if (Input.IsActionPressed("move_backward") && Input.GetMouseMode() != Input.MouseMode.Visible)
                movementInput.movement_direction.y -= 1;
            if (Input.IsActionPressed("move_left") && Input.GetMouseMode() != Input.MouseMode.Visible)
                movementInput.movement_direction.x -= 1;
            if (Input.IsActionPressed("move_right") && Input.GetMouseMode() != Input.MouseMode.Visible)
                movementInput.movement_direction.x += 1;


            if (Input.IsActionJustReleased("editor") && Input.GetMouseMode() != Input.MouseMode.Visible)
            {
                objectEditor.Show();
            }

            movementInput.movement_direction = movementInput.movement_direction.Normalized();

            movementInput.cam_direction += -cam_xform.basis.z * movementInput.movement_direction.y;
            movementInput.cam_direction += cam_xform.basis.x * movementInput.movement_direction.x;

            movementInput.cam_direction.y = 0;
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

        public override void _Input(InputEvent @event)
        {
            base._Input(@event);

            if (@event is InputEventMouseMotion && Input.GetMouseMode() == Input.MouseMode.Captured)
            {
                var mot = @event as InputEventMouseMotion;
                cameraBase.RotateY(-mot.Relative.x * CAMERA_ROTATION_SPEED);
                cameraBase.Orthonormalize();

                camera_x_rot = Mathf.Clamp(camera_x_rot + mot.Relative.y * CAMERA_ROTATION_SPEED, Mathf.Deg2Rad(CAMERA_X_ROT_MIN), Mathf.Deg2Rad(CAMERA_X_ROT_MAX));
                camera_y_rot = Mathf.Clamp(camera_y_rot + mot.Relative.x * CAMERA_ROTATION_SPEED, Mathf.Deg2Rad(CAMERA_X_ROT_MIN), Mathf.Deg2Rad(CAMERA_X_ROT_MAX));
                var newRot = (cameraBase.FindNode("rotation") as Spatial).Rotation;
                newRot.x = camera_x_rot;

                (cameraBase.FindNode("rotation") as Spatial).Rotation = newRot;
            }
        }

    }
}