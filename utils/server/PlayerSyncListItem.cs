using Godot;
using System;
using Game.Loader;
using System.Net;
namespace Game
{
    [Serializable]
    public class PlayerSyncListItem 
    {
        public Vector3 pos;
        public Vector3 rot;
        public uint timestamp;
        public int playerId;
    }
}