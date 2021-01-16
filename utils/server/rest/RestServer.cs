using System.IO;
using EmbedIO;
using EmbedIO.WebApi;
using SQLite;
using Godot;

using System.Threading;

namespace Game.Rest
{
    public class RestServer
    {
        WebServer server = null;

        System.Threading.Thread serverThread = null;

        private string serverUrl = "";

        public RestServer(string port)
        {
            serverUrl = "http://*:" + port + "/";
            serverThread = new System.Threading.Thread(new ThreadStart(CreateWebServer));
            serverThread.Start();
        }

        public void Close()
        {
            serverThread.Abort();
        }
        private void CreateWebServer()
        {
            GD.Print("Start Rest Server at " + serverUrl);

            server = new WebServer(o => o
                   .WithUrlPrefix(serverUrl)
                   .WithMode(HttpListenerMode.EmbedIO))
                .WithWebApi("/api", m => m.WithController<RestController>());

            // Listen for state changes.
            server.StateChanged += (s, e) => GD.Print("WebServer New State: " + e.NewState);
            server.RunAsync();
        }

    }
}