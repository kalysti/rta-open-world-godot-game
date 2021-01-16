using Godot;
using Game.Loader;
using System;
using System.Linq;
using System.Collections.Generic;
using Game.Rest;
using Newtonsoft.Json;
using System.Threading;
using SQLite;

namespace Game
{
    public class Server : NetworkGame
    {
        [Export]
        public float MAX_DISTANCE = 45.0f;

        [Export]
        public string ip = "0.0.0.0";

        [Export]
        public int maxPlayers = 10;

        [Export]
        public int port = 27021;
        [Export]
        public int maxPreAuthWaitTime = 10;

        [Export]
        public float maxObjectDistance = 10.0f;

        [Export]
        public int positionSyncTime = 100;

        [Export]
        public string currentMap = "level";
        public static SQLiteConnection database;

        [Export]
        public NodePath objectSpawnerPath;

        public ObjectSpawnerServer spawner = null;

        public RestServer restServer;


        public Server() : base()
        {
            CreateDatabase();
        }


        private void CreateDatabase()
        {
            var databasePath = System.IO.Path.Combine(OS.GetUserDataDir(), "database.db");
            GD.Print("DB Path: " + databasePath);

            database = new SQLiteConnection(databasePath);
            database.CreateTable<AuthUser>();
            database.CreateTable<OnlineCharacter>();
            database.CreateTable<Game.WorldObject>();
            

            restServer = new RestServer(port.ToString());
        }

        public override void _ExitTree()
        {
            restServer.Close();
        }

        public override void _Ready()
        {

            InitNetwork();

            spawner = (ObjectSpawnerServer)GetNode(objectSpawnerPath);

            spawner.CustomMultiplayer = spawner.CustomMultiplayer;

            network.SetBindIp(ip);
            network.CreateServer(port, maxPlayers);

            GD.Print("[Server] started at port " + port);
            Multiplayer.NetworkPeer = network;

            Multiplayer.Connect("network_peer_connected", this, "onPlayerConnect");
            Multiplayer.Connect("network_peer_disconnected", this, "onPlayerDisconnect");

        }

        public void onPlayerDisconnect(int id)
        {
            GD.Print("[Server] Client " + id.ToString() + " disconnected.");

        }
        public void onPlayerConnect(int id)
        {
            GD.Print("[Server] Client " + id.ToString() + " connected.");

            var preAuthTimer = new PreAuthTimer();
            preAuthTimer.WaitTime = maxPreAuthWaitTime;
            preAuthTimer.Name = id.ToString();

            var coll = new Godot.Collections.Array();
            coll.Add(id);
            GetNode("pre_auth_users").AddChild(preAuthTimer);
            preAuthTimer.Connect("timeout", this, "onPreAuthTimeout", coll);
            preAuthTimer.Start();

            RpcId(id, "startPreAuth");
        }

        [Remote]
        public void authPlayer(string accessToken = null)
        {
            var id = Multiplayer.GetRpcSenderId();

            GD.Print("[Server] " + id.ToString() + " try to auth");

            if (accessToken != null)
            {
                var authUser = Server.database.Table<AuthUser>().Where(o => o.Token == accessToken).FirstOrDefault();
                if (authUser != null)
                {
                    var timer = GetNode("pre_auth_users").GetNode(id.ToString()) as PreAuthTimer;
                    if (timer != null)
                    {
                        timer.Stop();
                        GetNode("pre_auth_users").RemoveChild(timer);

                        GD.Print("[Server] Player authed succesfull.");

                        var player = CreatePlayer(id, authUser.Id);
                        RpcId(id, "doHandshake", currentMap, player.GetPlayerPosition(), player.GetPlayerRotation());
                    }
                }
                else
                {
                    DisconnectClient(id, "Wrong auth informations.");
                }

            }
            else
            {
                DisconnectClient(id, "No given auth token.");
            }
        }

        [Remote]
        public void playerWorldLoaded()
        {
            var id = Multiplayer.GetRpcSenderId();
            SendPlayerList(id);
        }

        private void SendPlayerList(int id)
        {
            var list = new List<PlayerSyncListItem>();
            foreach (ServerPlayer player in GetNode("world/players").GetChildren())
            {
                if (id == player.networkId)
                    continue;

                var t = new PlayerSyncListItem();

                t.playerId = player.networkId;
                t.pos = player.GetPlayerPosition();
                t.rot = player.GetPlayerRotation();
                t.timestamp = OS.GetTicksMsec();

                list.Add(t);
            }

            RpcId(id, "GetPlayerList", Game.Networking.NetworkCompressor.Compress(list));
        }

        public ServerPlayer CreatePlayer(int id, int authid)
        {
            var playerScene = (PackedScene)ResourceLoader.Load("res://utils/player/ServerPlayer.tscn");
            var player = (ServerPlayer)playerScene.Instance();
            player.Name = id.ToString();
            player.networkId = id;
            player.authId = authid;

            GetNode("world/players").AddChild(player);

            player.Teleport((GetNode("world/map_holder/map") as BaseMap).GetSpawnPoint());
            Rpc("CreatePuppet", id, OS.GetTicksMsec(), player.GetPlayerPosition(), player.GetPlayerRotation());

            return player;
        }

        private void onPreAuthTimeout(int id)
        {
            DisconnectClient(id, "No auth information given.");
        }

        private void DisconnectClient(int id, string message = "")
        {
            RpcId(id, "forceDisconnect", message);

            var timer = GetNode("pre_auth_users").GetNode(id.ToString()) as PreAuthTimer;
            if (timer != null)
            {
                timer.Stop();
                GetNode("pre_auth_users").RemoveChild(timer);
            }

            network.DisconnectPeer(id);
        }
    }
}