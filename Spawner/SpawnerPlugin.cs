using DarkRift;
using DarkRift.Client;
using DarkRift.Server;
using System;
using System.Net;
using Utils;
using Utils.Messages.Response;
using MessageReceivedEventArgs = DarkRift.Client.MessageReceivedEventArgs;

namespace Spawner
{
    public class SpawnerPlugin : Plugin
    {
        private DarkRiftClient _client;
        public override Version Version => new Version(1, 0, 0);
        public override bool ThreadSafe => true;

        public IPAddress MasterIpAddress { get; set; }
        public int MasterPort { get; set; }

        public string SpawnerIpAddress { get; set; }
        public int MaxProcesses { get; set; }
        public string Region { get; set; }

        public bool AutoStartSpawner { get; set; }

        public SpawnerPlugin(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            MasterIpAddress = IPAddress.Parse(pluginLoadData.Settings.Get(nameof(MasterIpAddress)));
            MasterPort = Convert.ToInt32(pluginLoadData.Settings.Get(nameof(MasterPort)));

            SpawnerIpAddress = pluginLoadData.Settings.Get(nameof(SpawnerIpAddress));
            MaxProcesses = Convert.ToInt32(pluginLoadData.Settings.Get(nameof(MaxProcesses)));
            Region = pluginLoadData.Settings.Get(nameof(Region));

            AutoStartSpawner = Convert.ToBoolean(pluginLoadData.Settings.Get(nameof(AutoStartSpawner)));
        }

        protected override void Loaded(LoadedEventArgs args)
        {
            base.Loaded(args);
            _client = new DarkRiftClient();
            _client.ConnectInBackground(MasterIpAddress, MasterPort, IPVersion.IPv4, OnConnectedToMaster);
        }

        private void OnConnectedToMaster(Exception exception)
        {
            if (exception != null)
            {
                WriteEvent("Connection to master failed", LogType.Fatal, exception);
                return;
            }

            _client.MessageReceived += OnMessageFromMaster;

            if (AutoStartSpawner)
            {
                _client.SendMessage(Message.Create(MessageTags.RegisterSpawner, new SpawnerOptions
                {
                    Region = Region,
                    MachineIp = SpawnerIpAddress,
                    MaxProcesses = MaxProcesses
                }), SendMode.Reliable);
            }
        }

        private void OnMessageFromMaster(object sender, MessageReceivedEventArgs e)
        {
            using (var message = e.GetMessage())
            {
                switch (message.Tag)
                {
                    case MessageTags.RegisterSpawnerSuccess:
                        HandleRegisterSpawnerSuccess(message);
                        break;
                    case MessageTags.RegisterSpawnerFailed:
                        HandleRegisterSpawnerFailed(message);
                        break;
                }
            }
        }

        private void HandleRegisterSpawnerSuccess(Message message)
        {
            var data = message.Deserialize<RegisterSpawnerSuccessMessage>();
            if (data != null)
            {
                WriteEvent("Spawner " + Region + "-" + data.SpawnerID + " connected to master: " + MasterIpAddress + ":" + MasterPort, LogType.Info);
            }
        }

        private void HandleRegisterSpawnerFailed(Message message)
        {
            WriteEvent("Failed to register spawner", LogType.Error);
        }
    }
}
