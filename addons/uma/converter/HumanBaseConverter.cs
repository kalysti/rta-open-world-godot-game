using Godot;
using System.Collections.Generic;
using System.Linq;

namespace UMA.DNA
{
    public abstract class HumanBaseConverter
    {
        public List<GodotBindPose> poses = new List<GodotBindPose>();

        //working
        public void SetBoneScale(string skin, Vector3 value)
        {
            var overridePose2 = poses.Where(tf => tf.name.ToLower().Replace("_", "").Replace("0", "") == skin.ToLower().Replace("_", "").Replace("0", "")).FirstOrDefault();
            if (overridePose2 != null)
            {
                var sc = new Vector3(value.y, value.x, value.z);
                overridePose2.scale = sc;
            }
            else
            {
                GD.Print("Cant find bone: " + skin);
            }
        }

        public void SetBoneRotation(string skin, Quat value)
        {
            var overridePose2 = poses.Where(tf => tf.name.ToLower().Replace("_", "").Replace("0", "") == skin.ToLower().Replace("_", "").Replace("0", "")).FirstOrDefault();
            if (overridePose2 != null)
            {
                var tf = overridePose2.tf;
                var q = new Quat();
                q.x = value.y;
                q.x = value.x;
                q.x = value.z;
                q.w = value.w ;
                tf.basis = new Basis(q);
                overridePose2.tf = tf;
            }
            else
            {
                GD.Print("Cant find bone: " + skin);
            }
        }

        public void SetBonePositionRelative(string skin, Vector3 value, float weight = 1.0f)
        {

            var overridePose2 = poses.Where(tf => tf.name.ToLower().Replace("_", "").Replace("0", "") == skin.ToLower().Replace("_", "").Replace("0", "")).FirstOrDefault();
            if (overridePose2 != null)
            {
                var tf = overridePose2.tf;
                tf.origin += new Vector3(value.y, value.x, value.z) * (weight * -1);
                overridePose2.tf = tf;

                //SetBoneRest(overridePose2.index, tf);
            }
            else
            {
                GD.Print("Cant find bone: " + skin);
            }
        }
    }
}