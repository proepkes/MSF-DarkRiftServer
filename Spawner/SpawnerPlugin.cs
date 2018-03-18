using DarkRift;
using DarkRift.Client;
using DarkRift.Server;
using System;
using System.Net;
using MessageReceivedEventArgs = DarkRift.Client.MessageReceivedEventArgs;

namespace Spawner
{
    public class SpawnerPlugin : Plugin
    {
        private DarkRiftClient client;
        public override Version Version => new Version(1, 0, 0);
        public override bool ThreadSafe => true;

        public IPAddress MasterIpAddress { get; set; }
        public int MasterPort { get; set; }
        public string Region { get; set; }

        public SpawnerPlugin(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            MasterIpAddress = IPAddress.Parse(pluginLoadData.Settings.Get(nameof(MasterIpAddress)));
            MasterPort = Convert.ToInt32(pluginLoadData.Settings.Get(nameof(MasterPort)));
            Region = pluginLoadData.Settings.Get(nameof(Region));
        }

        protected override void Loaded(LoadedEventArgs args)
        {
            base.Loaded(args);
            client.ConnectInBackground(MasterIpAddress, MasterPort, IPVersion.IPv4, OnConnectedToMaster);
        }

        private void OnConnectedToMaster(Exception exception)
        {
            if (exception != null)
            {
                WriteEvent("Connection to master failed", LogType.Fatal, exception);
                return;
            }

            WriteEvent("Connected to master", LogType.Info);
            client.MessageReceived += OnMessageFromMaster;
        }

        private void OnMessageFromMaster(object sender, MessageReceivedEventArgs messageReceivedEventArgs)
        {
            //test
        }
    }
}
