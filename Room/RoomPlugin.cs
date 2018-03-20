using System;
using System.Net;
using DarkRift;
using DarkRift.Client;
using DarkRift.Server;

namespace Room
{
    /// <summary>
    /// This Plugin goes to the spawned Server
    /// </summary>
    public class RoomPlugin : Plugin
    {
        private DarkRiftClient client;
        
        public override Version Version => new Version(1, 0, 0);
        public override bool ThreadSafe => true;

        public IPAddress MasterIpAddress { get; set; }
        public int MasterPort { get; set; }

        public RoomPlugin(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            MasterIpAddress = IPAddress.Parse(pluginLoadData.Settings.Get(nameof(MasterIpAddress)));
            MasterPort = Convert.ToInt32(pluginLoadData.Settings.Get(nameof(MasterPort)));

            client = new DarkRiftClient();
        }

        protected override void Loaded(LoadedEventArgs args)
        {
            base.Loaded(args);
            client.ConnectInBackground(MasterIpAddress, MasterPort, IPVersion.IPv4, OnConnectedToMaster);
        }

        private void OnConnectedToMaster(Exception exception)
        {
            WriteEvent("Room connected to master", LogType.Info);
        }
    }
}
