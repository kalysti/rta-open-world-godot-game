using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Game
{
    public class PlayerFootsteps : Spatial
    {
        const string path = "res://audio/character/footsteps";

        private bool isStepLeft = false;
        private float next_step = 0.0f;

        [Export]
        public float stepDelay = 0.5f;

        [Export]
        public float maxIncreaseVolume = 1.0f;

        private Vector3 last_velocity = Vector3.Zero;

        public NetworkPlayer player = null;

        [Export]
        Godot.Collections.Dictionary<string, Color> colorList = new Godot.Collections.Dictionary<string, Color>();

        public Godot.Collections.Dictionary<string, AudioStreamSample[]> footsteps = new Godot.Collections.Dictionary<string, AudioStreamSample[]>();

        public override void _Ready()
        {
            scanDir(path);
            player = (GetParent() as NetworkPlayer);
        }

        public override void _Process(float delta)
        {
            if (player == null)
                return;

            if (next_step > 0.0)
                next_step = Mathf.Max(next_step - delta, 0.0f);

            /**
            	if (controller.on_floor && last_velocity < -controller.LandingThreshold):
		stream_player.stream = JumpLanding;
		stream_player.play();
		next_step = 0.4;
	

    */

            last_velocity = player.currentVelocity;
        }
        public override void _PhysicsProcess(float delta)
        {

            if (next_step <= 0.0)
                playStep();
        }

        int closestColor2(Color[] map, Color current)
        {
            float shortestDistance;
            int index;

            index = -1;
            shortestDistance = int.MaxValue;

            for (int i = 0; i < map.Length; i++)
            {
                Color match;
                float distance;

                match = map[i];
                distance = ColorDiff(current, match);

                if (distance < shortestDistance)
                {
                    index = i;
                    shortestDistance = distance;
                }
            }

            return index;
        }

        float ColorDiff(Color c1, Color c2)
        {
            float redDifference;
            float greenDifference;
            float blueDifference;

            redDifference = c1.r - c2.r;
            greenDifference = c1.g - c2.g;
            blueDifference = c1.b - c2.b;

            return redDifference * redDifference + greenDifference * greenDifference + blueDifference * blueDifference;
        }
        public string translateTerrain()
        {
            var mapPos = player.world.getMapImagePosition();
            var color = player.world.getMapColorByPosition((int)mapPos.x, (int)mapPos.z);

            var index = closestColor2(colorList.Values.ToArray(), color);

            if (index >= 0)
                return colorList.ElementAt(index).Key;
            else
                return null;
        }

        public void playStep()
        {
            string material = null;

            if (player == null)
                return;

            if ((GetNode("player_left") as AudioStreamPlayer3D).Playing || (GetNode("player_right") as AudioStreamPlayer3D).Playing)
                return;

            var velocity = player.currentVelocity;
            velocity.y = 0.0f;

            var maxSpeed = (player.playerState.isSprinting) ? 10.0f : 3.0f;

            if (!player.rayGround.IsColliding() || velocity.Length() < 3.0f / 2.0)
                return;

            var colliderObject = player.rayGround.GetCollider();

            if (player.world != null && player.world.baseMapImage != null && player.world.getMapTerrain() == colliderObject)
            {
                material = translateTerrain();
            }
            else if (colliderObject is FootstepStaticBody)
            {
                material = (colliderObject as FootstepStaticBody).footstepType.ToString().ToLower();
            }

            if (string.IsNullOrEmpty(material))
                return;

            else if (footsteps.ContainsKey(material))
            {
                generateSound(material, velocity);
            }
        }

        private void generateSound(string material, Vector3 velocity)
        {

            var speed = 3.0f / velocity.Length();
            next_step = stepDelay * speed;

            var randomSounds = footsteps[material];
            var random = new RandomNumberGenerator();
            random.Randomize();

            if (randomSounds.Length <= 0)
                return;

            var rand = random.RandiRange(0, randomSounds.Length - 1);
            var sample = randomSounds[rand];
            random.Randomize();

            var scale = (sample.GetLength() > stepDelay) ? sample.GetLength() / stepDelay : sample.GetLength();
            var player = isStepLeft ? (GetNode("player_left") as AudioStreamPlayer3D) : (GetNode("player_right") as AudioStreamPlayer3D);

            player.PitchScale = scale;
            player.Stream = sample;
            player.UnitDb = random.RandfRange(-1f, 1f);
            player.UnitSize = Mathf.Clamp(velocity.Length(), 0.0f, maxIncreaseVolume);
            player.Play();

            isStepLeft = !isStepLeft;
        }

        private void scanDir(string path)
        {
            string file_name = null;
            var dir = new Directory();
            var error = dir.Open(path);
            if (error != Error.Ok)
            {
                GD.PrintErr("Can't open " + path + "!");
            }

            dir.ListDirBegin(true);
            file_name = dir.GetNext();
            while (file_name != "")
            {
                if (dir.CurrentIsDir())
                {
                    var new_path = path + "/" + file_name;

                    var samples = findSamples(new_path);

                    if (samples.Length > 0)
                    {
                        footsteps.Add(file_name, samples);
                    }
                }

                file_name = dir.GetNext();
            }

            dir.ListDirEnd();
        }

        private AudioStreamSample[] findSamples(string path)
        {
            string file_name = null;
            var files = new System.Collections.Generic.List<AudioStreamSample>();
            var dir = new Directory();
            var error = dir.Open(path);
            if (error != Error.Ok)
            {
                GD.PrintErr("Can't open " + path + "!");
                return null;
            }

            dir.ListDirBegin(true);
            file_name = dir.GetNext();
            while (file_name != "")
            {
                if (!dir.CurrentIsDir())
                {
                    var name = path + "/" + file_name;
                    if (file_name.EndsWith(".wav"))
                    {
                        var step = GD.Load<AudioStreamSample>(name);
                        files.Add(step);
                    }
                }

                file_name = dir.GetNext();
            }

            dir.ListDirEnd();
            return files.ToArray();
        }
    }

}