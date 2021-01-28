using Godot;
using System;
using Game.Loader;
using System.Collections.Generic;
using RestClient.Net;

namespace Game
{
    public class Client : NetworkGame
    {
        [Export]
        public string hostname = "localhost";

        [Export]
        public int port = 27015;

        protected string accessToken = null;

        private int ownNetworkId = 0;

        public World gameWorld;

        public bool inputEnabled = true;

        private ServerVersion serverVersion = null;

        AcceptDialog customConnectDialog = null;
        SettingsMenu settingsMenu = null;
        ServerPreAuthDialog serverPareAuthDialog = null;
        CharacterSelector charSelector = null;

        Timer statsTimer = new Timer();
        public override void _Ready()
        {
            InitNetwork();

            Multiplayer.Connect("connected_to_server", this, "onConnected");
            Multiplayer.Connect("connection_failed", this, "onConnectionFailed");
            Multiplayer.Connect("server_disconnected", this, "onDisconnect");

            serverPareAuthDialog = GetNode("hud/server_preauth_dialog") as ServerPreAuthDialog;
            serverPareAuthDialog.Connect("onLogin", this, "onLoginSuccess");

            customConnectDialog = GetNode("hud/custom_connect_dialog") as AcceptDialog;
            customConnectDialog.Connect("confirmed", this, "onCustomServerConnect");

            charSelector = GetNode("hud/char_selector") as CharacterSelector;
            settingsMenu = GetNode("hud/settings") as SettingsMenu;
            charSelector.Visible = false;

            statsTimer.Name = "stats_timer";
            statsTimer.Autostart = true;
            statsTimer.WaitTime = 1.0f;

            statsTimer.Connect("timeout", this, "showSystemStats");
            charSelector.Connect("onSelect", this, "onSelectedChar");

            AddChild(statsTimer);
            (GetNode("hud/menu/blur_bg") as Sprite).Visible = false;
        }
        public override void _Process(float delta)
        {
            base._Process(delta);

            //cursor
            if (Input.IsActionJustPressed("ui_cancel"))
            {
                if (gameWorld != null && gameWorld.IsInsideTree())
                {
                    if ((GetNode("hud/menu") as Control).Visible)
                    {
                        Input.SetMouseMode(Input.MouseMode.Captured);
                        (GetNode("hud/menu") as Control).Visible = false;
                        (GetNode("hud/menu/menu_bg") as TextureRect).Visible = true;
                        (GetNode("hud/menu/blur_bg") as Sprite).Visible = false;
                    }

                    else
                    {
                        Input.SetMouseMode(Input.MouseMode.Visible);
                        (GetNode("hud/menu") as Control).Visible = true;
                        (GetNode("hud/menu/menu_bg") as TextureRect).Visible = false;
                        (GetNode("hud/menu/blur_bg") as Sprite).Visible = true;
                    }
                }
                else
                {
                    Input.SetMouseMode(Input.MouseMode.Visible);
                }
            }
        }
        public void onLoginSuccess(string _token)
        {
            accessToken = _token;
            charSelector.setWelcomeText("Welcome to " + serverVersion.name);
            charSelector.Visible = true;
            charSelector.hostname = hostname;
            charSelector.port = port;
            charSelector.SetToken(accessToken);
            charSelector.GetCharList();
        }

        public void onSelectedChar(int charId)
        {
            charSelector.Visible = false;

            //transmit selected char to server
            ConnectToServer();
        }
        public void onCustomServerConnect()
        {
            var customAddress = (customConnectDialog.FindNode("custom_server_address") as LineEdit).Text;
            var custumPort = (customConnectDialog.FindNode("custom_server_port") as LineEdit).Text;

            try
            {
                port = Int16.Parse(custumPort);
                hostname = customAddress;
                DoPreAuthLogin();

            }
            catch
            {

            }
        }

        public async void DoPreAuthLogin()
        {
            try
            {
                var restClient = new RestClient.Net.Client(new RestClient.Net.NewtonsoftSerializationAdapter(), new Uri("http://" + hostname + ":" + (port + 1) + "/api/hello"));
                serverVersion = await restClient.GetAsync<ServerVersion>();
                drawSystemMessage("Server version: v" + serverVersion.version);

                serverPareAuthDialog.hostname = hostname;
                serverPareAuthDialog.port = port;
                serverPareAuthDialog.setWelcomeMessage("Auth on " + serverVersion.name);
                serverPareAuthDialog.Visible = true;
            }
            catch (Exception e)
            {
                drawSystemMessage("This is not a economics server: " + e.Message);
            }
        }

        public void ConnectToServer()
        {
            var realIP = IP.ResolveHostname(hostname, IP.Type.Ipv4);
            if (realIP.IsValidIPAddress())
            {
                drawSystemMessage("Try to connect to " + realIP + ":" + port);
                var error = network.CreateClient(realIP, port);
                if (error != Error.Ok)
                {
                    drawSystemMessage("Network error:" + error.ToString());
                }

                Multiplayer.NetworkPeer = network;
            }
            else
            {
                drawSystemMessage("Hostname not arreachable.");
            }
        }

        public void onConnectionFailed()
        {
            drawSystemMessage("Error cant connect.");
            freeMap();
        }

        public void onDisconnect()
        {
            drawSystemMessage("Server disconnected.");
            freeMap();
        }

        public void onConnected()
        {
            ownNetworkId = Multiplayer.GetNetworkUniqueId();
            drawSystemMessage("Connection established. Your id is " + ownNetworkId.ToString());
        }

        private void drawSystemMessage(string message)
        {
            GD.Print("[Client] " + message);
            (GetNode("hud/top/message") as Label).Text = message;
        }

        private void _on_connect_button_pressed()
        {
            if (gameWorld == null || (gameWorld != null && !gameWorld.IsInsideTree()))
            {
                customConnectDialog.PopupCentered();
            }
            else
            {
                network.CloseConnection();
                freeMap();
            }
        }

        private void _on_settings_button_pressed()
        {
            settingsMenu.PopupCentered();
        }

        [Puppet]
        public void CreatePuppet(int playerId, uint timestamp, Vector3 pos, Vector3 rot)
        {
            if (gameWorld != null && gameWorld.player != null && playerId != gameWorld.player.networkId)
                gameWorld.CreatePuppet(playerId, timestamp, pos, rot);
        }

        [Puppet]
        public void InitObjectList(string objectListJson)
        {
            var objectList = Game.Networking.NetworkCompressor.Decompress<List<WorldObject>>(objectListJson);

            if (gameWorld == null)
                return;

            gameWorld.spawner.InitObjectList(objectList, spawnPoint);

            //Creation objects are finish
            var creationThread = new System.Threading.Thread(() =>
            {
                while (!gameWorld.spawner.allInitObjectsCreated())
                {
                    System.Threading.Thread.Sleep(100);
                }

                CallDeferred("playerWorldProccesed");
            });

            creationThread.Start();
        }

        public void playerWorldProccesed()
        {

            GD.Print("[Client] Game world is processed");
            gameWorld.CreateLocalPlayer(ownNetworkId, spawnPoint, spawnRotation, inputEnabled);
            Input.SetMouseMode(Input.MouseMode.Captured);
            RpcId(1, "ActivatePlayer");
        }

        [Puppet]
        public void GetPlayerList(string playerListJson)
        {
            var playerList = Game.Networking.NetworkCompressor.Decompress<List<PlayerSyncListItem>>(playerListJson);

            if (gameWorld == null)
                return;

            foreach (var item in playerList)
            {
                gameWorld.CreatePuppet(item.playerId, item.timestamp, item.pos, item.rot);
            }
        }
        private Vector3 spawnPoint = Vector3.Zero;
        private Vector3 spawnRotation = Vector3.Zero;

        [Puppet]
        private void doHandshake(string mapName, Vector3 _spawnPoint, Vector3 _spawnRot)
        {
            drawSystemMessage("Handhshaking complete. Loading map: " + mapName);
            drawSystemMessage("Loading game world");

            (GetNode("hud/menu/lobby_music") as AudioStreamPlayer).Stop();

            var scene = (PackedScene)ResourceLoader.Load("res://utils/world/World.tscn");
            gameWorld = (World)scene.Instance();
            gameWorld.Name = "world";
            AddChild(gameWorld);

            spawnPoint = _spawnPoint;
            spawnRotation = _spawnRot;
            (GetNode("hud/menu") as Control).Visible = false;

            gameWorld.Connect("onMapLoadComplete", this, "onPlayerWorldLoaded");
            gameWorld.LoadMap();
        }

        private void onPlayerWorldLoaded()
        {
            drawSystemMessage("Creating game world");
            RpcId(1, "playerWorldLoaded");
        }

        [PuppetSync]
        private void startPreAuth()
        {
            drawSystemMessage("Start to pre auth with token" + accessToken);
            RpcId(1, "authPlayer", accessToken);
        }

        [PuppetSync]
        private void forceDisconnect(string message)
        {
            drawSystemMessage("Connection lost for reason:" + message);
            freeMap();
        }
        private void freeMap()
        {
            if (gameWorld != null)
            {
                drawSystemMessage("Freeing game world");
                gameWorld.QueueFree();
                gameWorld = null;
            }

            (GetNode("hud/menu") as Control).Visible = true;
            (GetNode("hud/menu/menu_bg") as TextureRect).Visible = true;
            (GetNode("hud/menu/blur_bg") as Sprite).Visible = false;
            (GetNode("hud/menu/lobby_music") as AudioStreamPlayer).Play();

            Input.SetMouseMode(Input.MouseMode.Visible);
        }

        private void showSystemStats()
        {
            var pos = "";
            var net_out = "";
            var net_in = "";
            var objects = "";

            if (gameWorld != null && gameWorld.IsInsideTree() && gameWorld.player != null)
            {
                pos += "x:" + Math.Round(gameWorld.player.GetPlayerPosition().x, 4);
                pos += ", y:" + Math.Round(gameWorld.player.GetPlayerPosition().y, 4);
                pos += ", z:" + Math.Round(gameWorld.player.GetPlayerPosition().z, 4);

                net_out = gameWorld.player.networkStats.getNetOut();
                net_in = gameWorld.player.networkStats.getNetIn();
            }

            if (gameWorld != null && gameWorld.spawner != null)
            {
                objects += gameWorld.spawner.objectsDrawin;
                objects += " / ";
                objects += gameWorld.spawner.totalObjects;
            }

            (GetNode("hud/top/pos") as Label).Text = "Pos: " + pos;
            (GetNode("hud/top/net") as Label).Text = "Net in / out: " + net_in + " / " + net_out;
            (GetNode("hud/top/fps_counter") as Label).Text = "FPS: " + Engine.GetFramesPerSecond();
            (GetNode("hud/top/objects") as Label).Text = "Objects: " + objects;
            (GetNode("hud/top/memory") as Label).Text = "Memory: " + OS.GetStaticMemoryUsage() / 1024 / 1024 + " MB";
            (GetNode("hud/top/video_memory") as Label).Text = "VRAM: " + (Performance.GetMonitor(Performance.Monitor.RenderVideoMemUsed) / 1024 / 1024).ToString() + " MB";
        }

    }
}