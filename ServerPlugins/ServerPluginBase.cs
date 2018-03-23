using System;
using System.Collections.Generic;
using DarkRift;
using DarkRift.Server;

namespace ServerPlugins
{

    public delegate void MessageHandler(IClient client, Message message);

    public abstract class ServerPluginBase : Plugin
    {
        public override bool ThreadSafe => true;
        public override Version Version => new Version(1, 0, 0);

        private readonly Dictionary<ushort, MessageHandler> _handlers;

        protected ServerPluginBase(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            _handlers = new Dictionary<ushort, MessageHandler>();
            ClientManager.ClientConnected += OnClientConnected;
        }

        protected virtual void OnClientConnected(object sender, ClientConnectedEventArgs e)
        {
            e.Client.MessageReceived += OnMessagereceived;
        }

        private void OnMessagereceived(object sender, MessageReceivedEventArgs e)
        {
            var message = e.GetMessage();
            
            if (message != null)
            {
                WriteEvent("Received message with tag " + message.Tag, LogType.Trace);
                if (_handlers.ContainsKey(message.Tag))
                {
                    _handlers[message.Tag](e.Client, message);
                }
            }
            else
            {

                WriteEvent("Received message null", LogType.Trace);
            }
        }

        protected void SetHandler(ushort tag, MessageHandler handler)
        {
            _handlers[tag] = handler;
        }
    }
}