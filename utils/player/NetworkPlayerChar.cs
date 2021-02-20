using Godot;
using System;
using Newtonsoft.Json;

namespace Game
{
    public class NetworkPlayerChar : Spatial
    {

        [Signal]
        public delegate void CharacterInitalized(string reciepeJson);

        [Export]
        public NodePath skeletonPath;

        [Export]
        public NodePath animationTreePath;

        public UMASkeleton skeleton { get; set; }
        public AnimationTree animTree { get; set; }

        private bool _isMale = false;

        [Export]
        public bool isMale { get { return _isMale; } set { _isMale = value; } }

        [Export(PropertyHint.File, "*.tscn; Scene file")]
        public string femaleChar { get; set; }

        [Export(PropertyHint.File, "*.tscn; Scene file")]
        public string maleChar { get; set; }

        [Export(PropertyHint.File, "*.tscn; Scene file")]
        public string femaleCharAnimator { get; set; }

        [Export(PropertyHint.File, "*.tscn; Scene file")]
        public string maleCharAnimator { get; set; }

        public Godot.Collections.Dictionary<string, float> appearance = new Godot.Collections.Dictionary<string, float>();

        public override void _Ready()
        {
            animTree = (AnimationTree)GetNode(animationTreePath);
        }

        public UMAReciepe initCharacter(bool isMaleOrFemale, UMAReciepe reciepe = null)
        {
            var animTree2 = (AnimationTree)animTree.Duplicate();

            animTree.Active = false;
            var amature = GetNodeOrNull<Spatial>("Armature");
            var player = GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
            var oldSkeleton = GetNodeOrNull<UMASkeleton>("Armature/Skeleton");

            if (player != null)
                RemoveChild(player);

            if (oldSkeleton != null)
                amature.RemoveChild(oldSkeleton);

            PackedScene scene = GD.Load<PackedScene>((isMaleOrFemale) ? maleChar : femaleChar);

            var newNode = (UMASkeleton)scene.Instance();
            newNode.Name = "Skeleton";
            skeleton = newNode;

            //newNode.Connect("Created", this, "onCharacterCreated");
            amature.AddChild(newNode);

            if (reciepe == null)
            {
                reciepe = newNode.getDefaultReciepe();
            }

            if (reciepe != null)
            {
                newNode.loadReciepe(reciepe);
                newNode.generateDNA(reciepe.dna);
            }

            PackedScene animationPlayerScene = GD.Load<PackedScene>((isMaleOrFemale) ? maleCharAnimator : femaleCharAnimator);
            var animationPlayer = (AnimationPlayer)animationPlayerScene.Instance();
            animationPlayer.Name = "AnimationPlayer";

            AddChild(animationPlayer);
            _isMale = isMaleOrFemale;

            RemoveChild(animTree);
            AddChild(animTree2);

            animTree2.AnimPlayer = animationPlayer.GetPath();

            animTree = animTree2;
            animTree.Active = true;


            var reciepeJson = JsonConvert.SerializeObject(reciepe);
            EmitSignal(nameof(CharacterInitalized), reciepeJson);

            return reciepe;
        }


        public void doCatwalk()
        {
            animTree.Set("parameters/movement_state/current", 6);
        }
        public void ProcessAnimation(NetworkPlayerState state, PlayerInput movementState, float delta)
        {
            animTree.Set("parameters/walk_velocity/blend_position", movementState.velocity.Length());

            if (state.inVehicle)
            {
                animTree.Set("parameters/movement_state/current", 2);
            }
            else if (!state.onGround)
            {
                animTree.Set("parameters/movement_state/current", 3);
            }
            else
            {
                animTree.Set("parameters/movement_state/current", 0);
            }
        }
    }
}