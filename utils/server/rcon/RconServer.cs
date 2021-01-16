using Godot;
using System;
using Game.Loader;
using RCONServerLib;
using System.Net;
using EmbedIO;
using EmbedIO.WebApi;

namespace Game.Rcon
{
    public class RconServer : Node
    {
        [Export]
        public int port = 27015;

        public RemoteConServer server;

        public override void _Ready()
        {
            var server = new RemoteConServer(IPAddress.Any, port)
            {
                SendAuthImmediately = true,
                Debug = true,
                Password = "test"
            };

            server.CommandManager.Add("players", "Player list", (command, arguments) =>
           {
               var players = GetParent().GetNode("world").GetNode("players").GetChildren();
               var str = "";
               str += "ID \t";
               str += "X \t";
               str += "Y \t";
               str += "Z \t";
               str += "PING\n";

               foreach (NetworkPlayer p in players)
               {
                   str += p.networkId + "\t";
                   str += Math.Round(p.GlobalTransform.origin.x, 4) + "\t";
                   str += Math.Round(p.GlobalTransform.origin.y, 4) + "\t";
                   str += Math.Round(p.GlobalTransform.origin.z, 4) + "\t";
                   str += p.networkStats.pingMs;
               }

               return str;
           });

            GD.Print("Start RCON Server at port" + port);
            server.StartListening();

        }
    }
}