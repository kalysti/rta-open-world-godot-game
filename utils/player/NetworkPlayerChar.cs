using Godot;
using System;
namespace Game
{

    public class NetworkPlayerChar : Spatial
    {

        [Export]
        public NodePath skeletonPath;

        [Export]
        public NodePath animationTreePath;

        public Skeleton skeleton { get; set; }
        public AnimationTree animTree { get; set; }

        public AnimationNodeStateMachinePlayback animation_state_machine { get; set; }
        public Godot.Collections.Dictionary<string, float> appearance = new Godot.Collections.Dictionary<string, float>();

        public override void _Ready()
        {
            skeleton = (Skeleton)GetNode(skeletonPath);

            AddClothes("body");
            AddClothes("eyes");

            animTree = (AnimationTree)GetNode(animationTreePath);
            animation_state_machine = (AnimationNodeStateMachinePlayback)animTree.Get("parameters/playback");

        }
        private void ResetMesh(MeshInstance mesh_inst)
        {
            var dic = (Vector3[])CharEditGlobal.meshs_shapes[mesh_inst.Name]["base_form"];
            DrawMesh(mesh_inst, dic);
        }

        private void GenerateMesh(MeshInstance mesh_inst)
        {
            ResetMesh(mesh_inst);
            Vector3[] vertex_arr = (Vector3[])mesh_inst.Mesh.SurfaceGetArrays(0)[(int)Mesh.ArrayType.Vertex];
            foreach (var x in appearance)
            {
                var dic = (Godot.Collections.Dictionary)CharEditGlobal.meshs_shapes[mesh_inst.Name]["shp_name_index"];
                if (dic.Contains(x.Key))
                {
                    var newShapeIndex = (int)dic[x.Key];
                    vertex_arr = UpdateVertex(mesh_inst, vertex_arr, newShapeIndex, appearance[x.Key]);
                }
            }

            DrawMesh(mesh_inst, vertex_arr);
        }

        public void GenerateRandomFace(float cartoonish)
        {
            var blend_array = (Godot.Collections.Dictionary)CharEditGlobal.meshs_shapes["forms"]["head"];
            var random = new RandomNumberGenerator();
            foreach (System.Collections.DictionaryEntry x in blend_array)
            {
                appearance[x.Key.ToString()] = random.Randf() * cartoonish;
                random.Randomize();
            }

            GenerateAllMeshes();
        }

        public void GenerateAllMeshes()
        {
            var time = OS.GetTicksMsec();

            foreach (MeshInstance mesh in skeleton.GetChildren())
            {
                GenerateMesh(mesh);
            }

        }

        private Vector3[] UpdateVertex(MeshInstance mesh, Vector3[] vertex_arr, int shape_index, float value)
        {
            var list = new Godot.Collections.Dictionary<string, Int32>();
            var blend_array = (Godot.Collections.Array)CharEditGlobal.meshs_shapes[mesh.Name]["blendshapes"];
            Vector3[] blend = (Vector3[])blend_array[shape_index];

            for (int i = 0; i < vertex_arr.Length; i++)
            {
                vertex_arr[i] += blend[i] * value;
            }

            return vertex_arr;

        }

        public void UpdateMorph(string shape_name, float value)
        {
            float temp = value;
            if (appearance.ContainsKey(shape_name))
            {
                value -= appearance[shape_name];
            }

            foreach (MeshInstance mesh in skeleton.GetChildren())
            {
                var dic = (Godot.Collections.Dictionary)CharEditGlobal.meshs_shapes[mesh.Name]["shp_name_index"];

                if (dic.Contains(shape_name))
                {
                    Int32 record = (Int32)dic[shape_name];
                    Vector3[] vertex_arr = (Vector3[])mesh.Mesh.SurfaceGetArrays(0)[(int)Mesh.ArrayType.Vertex];
                    DrawMesh(mesh, UpdateVertex(mesh, vertex_arr, record, value));
                }
            }

            appearance[shape_name] = temp;
            if (appearance[shape_name] == 0)
                appearance.Remove(shape_name);
        }

        public void DrawMesh(MeshInstance mesh, Vector3[] vertex_array)
        {
            var mat = mesh.GetSurfaceMaterial(0);
            var mesh_arrs = mesh.Mesh.SurfaceGetArrays(0);

            mesh_arrs[(int)Mesh.ArrayType.Vertex] = vertex_array;
            var new_array_mesh = new ArrayMesh();
            new_array_mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, mesh_arrs);
            mesh.Mesh = new_array_mesh;

            mesh.SetSurfaceMaterial(0, mat);
        }

        public void AddClothes(string cloth)
        {
            var node = skeleton.GetNodeOrNull(cloth);
            if (node != null)
                return;

            var clothes = (PackedScene)ResourceLoader.Load("res://assets/character/clothes/"+cloth+".tscn");
            MeshInstance newMesh = (MeshInstance)clothes.Instance();
            skeleton.AddChild(newMesh);

            GenerateMesh(newMesh);
        }
        public void RemoveClothes(string cloth)
        {
            foreach (MeshInstance node in skeleton.GetChildren())
            {
                if (node.Name == cloth)
                {
                    node.QueueFree();
                }
            }
        }
        public void doCatwalk()
        {
             animation_state_machine.Travel("catwalk");
        }
        public void ProcessAnimation(NetworkPlayerState state, PlayerInput movementState, float delta)
        {
            animTree.Set("parameters/blend_tree/locomotion/idle_walk_run/blend_position", movementState.velocity.Length());

            if (state.inVehicle)
            {
                animation_state_machine.Travel("drive");
            }
            else if (!state.onGround)
                animation_state_machine.Travel("fall");
            else
            {
                if (state.isClimbing)
                    animation_state_machine.Travel("climb");
                else
                    animation_state_machine.Travel("blend_tree");
            }
        }
    }
}