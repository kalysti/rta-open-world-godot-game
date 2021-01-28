using System;
using Godot;
using SQLite;

namespace Game
{
    [Table("objects")]
    [Serializable]
    public class WorldObject
    {
        [PrimaryKey, AutoIncrement]
        [Column("id")]
        public int Id { get; set; }

        [Column("pos_x")]
        public float pos_x { get; set; }

        [Column("pos_y")]
        public float pos_y { get; set; }

        [Column("pos_z")]
        public float pos_z { get; set; }

        [Column("rot_x")]
        public float rot_x { get; set; }

        [Column("rot_y")]
        public float rot_y { get; set; }

        [Column("rot_z")]
        public float rot_z { get; set; }


        [Column("scale_x")]
        public float scale_x { get; set; }

        [Column("scale_y")]
        public float scale_y { get; set; }

        [Column("scale_z")]
        public float scale_z { get; set; }

        [Column("type")]
        public WorldObjectType type { get; set; }

        [Column("model")]
        public string modelName { get; set; }

        public Vector3 GetPosition()
        {
            return new Vector3(pos_x, pos_y, pos_z);
        }
        public Vector3 GetRotation()
        {
            return new Vector3(rot_x, rot_y, rot_z);
        }
        public Vector3 GetScale()
        {
            return new Vector3(scale_x, scale_y, scale_z);
        }
        public void SetPosition(Vector3 pos)
        {
            pos_x = pos.x;
            pos_y = pos.y;
            pos_z = pos.z;
        }
        public void SetRotation(Vector3 rot)
        {
            rot_x = rot.x;
            rot_y = rot.y;
            rot_z = rot.z;
        }

        public void SetScale(Vector3 scale)
        {
            scale_x = scale.x;
            scale_y = scale.y;
            scale_z = scale.z;
        }

        public string getResourcePath()
        {
            if (type == WorldObjectType.PROPERTY)
            {
                return "res://objects/" + modelName + ".tscn";
            }
            else if (type == WorldObjectType.VEHICLE)
            {
                return "res://vehicles/" + modelName + ".tscn";

            }
            else if (type == WorldObjectType.SYSTEM)
            {
                return "res://" + modelName + ".tscn";
            }
            else if (type == WorldObjectType.MARKER)
            {
                return "res://utils/world/objects/marker/marker.tscn";
            }
            else
            {
                return null;
            }
        }
    }
}

