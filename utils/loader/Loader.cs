using Godot;
using System;

namespace Game.Loader
{
    public class Loader : Control
    {

        public Client client = null;
        public Server server = null;
        public override void _Ready()
        {

            string[] args = OS.GetCmdlineArgs();

            Core.isServer = Array.Exists(args, element => element == "server");
            Core.isDebug = Convert.ToBoolean(ProjectSettings.GetSetting("global/Debug"));

            if (!Core.isDebug)
                Core.isDebug = Array.Exists(args, element => element == "debug");
            else
            {
                var p = Array.Exists(args, element => element == "no-debug");

                if (p)
                {
                    Core.isDebug = false;
                }
            }

            if (Core.isDebug)
            {
                GD.Print("Debug version");
            }
            else
            {
                ProjectSettings.LoadResourcePack("res://assets.pck");
            }

            if (Core.isDebug)
                Core.isClientAndServer = true;

            if (!Core.isServer || Core.isDebug || (!Core.isServer && !Core.isDebug))
            {
                startClient();
            }

            if (Core.isServer || Core.isDebug)
            {
                startServer();
            }
        }

        private void startClient()
        {
            GD.Print("Starting client");

            var scene = ResourceLoader.Load("res://utils/client/Client.tscn") as PackedScene;

            if (scene != null)
            {
                client = (Client)scene.Instance();
                client.Name = "client";

                GetNode("vbox/vbox_client/client_viewport").AddChild(client);
            }
        }

        private void startClient2()
        {
            GD.Print("Starting client 2");

            var scene = ResourceLoader.Load("res://utils/client/Client.tscn") as PackedScene;

            if (scene != null)
            {
                var instance = scene.Instance() as Client;
                instance.Name = "client";
                instance.inputEnabled = false;

                GetNode("vbox/vbox_client_2/client_2_viewport").AddChild(instance);
            }
        }

        private void startServer()
        {
            GD.Print("Starting server");

            var scene = ResourceLoader.Load("res://utils/server/Server.tscn") as PackedScene;

            if (scene != null)
            {
                server = (Server)scene.Instance();
                server.Name = "server";
                // server.Connect("onServerIsReady", client, "doAutologin");
                GetNode("vbox/vbox_server/server_viewport").AddChild(server);
            }
        }
    }

}