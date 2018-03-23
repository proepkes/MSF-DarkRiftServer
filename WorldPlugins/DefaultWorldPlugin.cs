using System;
using DarkRift;
using DarkRift.Server;

namespace ServerPlugins
{
    public abstract class DefaultWorldPlugin : Plugin
    {
        public override bool ThreadSafe => true;
        public override Version Version => new Version(1, 0, 0);

        protected DefaultWorldPlugin(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            ClientManager.ClientConnected += OnClientConnected;
        }

        protected virtual void OnClientConnected(object sender, ClientConnectedEventArgs e)
        {
            e.Client.MessageReceived += OnMessagereceived;
        }

        private void OnMessagereceived(object sender, MessageReceivedEventArgs e)
        {
            OnMessagereceived(sender, e.GetMessage());
        }

        protected abstract void OnMessagereceived(object sender, Message e);
    }
}