using Godot;
using System;

namespace Game.Loader
{
    public class Loader : Control
    {

        public override void _Ready()
        {
            string[] args = OS.GetCmdlineArgs();

            Core.isServer = Array.Exists(args, element => element == "server");
            Core.isDebug = Convert.ToBoolean(ProjectSettings.GetSetting("global/Debug"));
            
            if (!Core.isDebug)
                Core.isDebug = Array.Exists(args, element => element == "debug");

            if (Core.isDebug)
            {
                GD.Print("Debug version");
            }

            if (Core.isDebug)
                Core.isClientAndServer = true;

            if (Core.isServer || Core.isDebug)
            {
                startServer();
            }

            if (!Core.isServer || Core.isDebug)
            {
                startClient();
                //startClient2();
            }
            if (!Core.isServer && !Core.isDebug)
            {
                startClient();
            }
        }

        private void startClient()
        {
            GD.Print("Starting client");

            var scene = ResourceLoader.Load("res://utils/client/Client.tscn") as PackedScene;

            if (scene != null)
            {
                var instance = scene.Instance();
                instance.Name = "client";

                GetNode("vbox/vbox_client/client_viewport").AddChild(instance);
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
                var instance = scene.Instance();
                instance.Name = "server";
                GetNode("vbox/vbox_server/server_viewport").AddChild(instance);
            }
        }
    }

}