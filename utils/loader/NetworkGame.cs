using Godot;
using System;

namespace Game.Loader
{
    public class NetworkGame : Node
    {
        public NetworkedMultiplayerENet network = null;

        public void InitNetwork()
        {

            CustomMultiplayer = new MultiplayerAPI();
            CustomMultiplayer.SetRootNode(this);

            network = new NetworkedMultiplayerENet();

            GetTree().Connect("node_added", this, "onNodeUpdate");
            customizeChildren(this);

        }

        public override void _Process(float delta)
        {
            if (CustomMultiplayer != null && CustomMultiplayer.HasNetworkPeer())
            {
                CustomMultiplayer.Poll();
            }
        }


        private void onNodeUpdate(Node node)
        {

            var path = node.GetPath().ToString();
            var mypath = GetPath().ToString();


            if (!path.Contains(mypath))
            {
                return;
            }
            var rel = path.Replace(mypath, "");

            if (rel.Length() > 0 && !rel.StartsWith("/"))
                return;


            node.CustomMultiplayer = CustomMultiplayer;
        }

        public override void _Notification(int what)
        {

            if (what == NotificationExitTree)
            {
                GetTree().Disconnect("node_added", this, "onNodeUpdate");
            }

        }

        private void customizeChildren(Node _node)
        {
            foreach (Node ar in _node.GetChildren())
            {
                ar.CustomMultiplayer = CustomMultiplayer;

                if (ar.GetChildCount() > 0)
                    customizeChildren(ar);
            }
        }

    }

}