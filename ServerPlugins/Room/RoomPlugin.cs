using System;
using System.IO;
using System.Net;
using System.Reflection;
using DarkRift;
using DarkRift.Client;
using DarkRift.Server;
using ServerPlugins;
using Utils;
using Utils.Messages.Notifications;
using Utils.Messages.Requests;
using Utils.Messages.Responses;
using Utils.Packets;

namespace WorldPlugins.Room
{
    public delegate void RoomAccessProviderCallback(RoomAccessPacket access, string error);
    public delegate void RoomAccessProvider(UsernameAndPeerIdPacket requester, RoomAccessProviderCallback giveAccess);

    /// <summary>
    ///     This Plugin goes to the spawned server
    /// </summary>
    public class RoomPlugin : ServerPluginBase
    {
        // ReSharper disable InconsistentNaming
        private readonly int SpawnTaskID;

        private readonly string SpawnCode;
        // ReSharper restore InconsistentNaming

        private readonly DarkRiftClient _client;

        public override Version Version => new Version(1, 0, 0);

        public override bool ThreadSafe => true;

        public IPAddress MasterIpAddress { get; set; }
        public int MasterPort { get; set; }
        public int MaxPlayers { get; set; }
        public bool IsPublic { get; set; }
        public string WorldName { get; set; }
        public string RoomName { get; set; }
        public bool IsRoomRegistered { get; protected set; }

        private RoomAccessProvider _accessProvider;

        public RoomPlugin(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            MasterIpAddress = IPAddress.Parse(pluginLoadData.Settings.Get(nameof(MasterIpAddress)));
            MasterPort = Convert.ToInt32(pluginLoadData.Settings.Get(nameof(MasterPort)));

            IsPublic = Convert.ToBoolean(pluginLoadData.Settings.Get(nameof(IsPublic)));
            SpawnCode = pluginLoadData.Settings.Get(nameof(SpawnCode));
            SpawnTaskID = Convert.ToInt32(pluginLoadData.Settings.Get(nameof(SpawnTaskID)));
            MaxPlayers = Convert.ToInt32(pluginLoadData.Settings.Get(nameof(MaxPlayers)));
            WorldName = pluginLoadData.Settings.Get(nameof(WorldName));
            RoomName = pluginLoadData.Settings.Get(nameof(RoomName));

            _client = new DarkRiftClient();
        }

        protected override void Loaded(LoadedEventArgs args)
        {
            base.Loaded(args);
            WriteEvent("Connecting to " + MasterIpAddress + ":" + MasterPort, LogType.Info);
            _client.ConnectInBackground(MasterIpAddress, MasterPort, IPVersion.IPv4, OnConnectedToMaster);

            SetHandler(MessageTags.RegisterRoomSuccess, HandleRegisterRoomSuccess);
            SetHandler(MessageTags.RegisterSpawnedProcessSuccess, HandleRegisterSpawnedProcessSuccess);
        }

        private void OnConnectedToMaster(Exception exception)
        {
            _client.SendMessage(Message.Create(MessageTags.RegisterSpawnedProcess,
                    new RegisterSpawnedProcessMessage {SpawnTaskID = SpawnTaskID, SpawnCode = SpawnCode}),
                SendMode.Reliable);
        }
        private void HandleRegisterRoomSuccess(IClient client, Message message)
        {
            var data = message.Deserialize<RegisterRoomSuccessMessage>();
            if (data != null)
            {
                IsRoomRegistered = true;

                _accessProvider = CreateAccess;


                var packet = new SpawnFinalizedMessage
                {
                    SpawnTaskID = SpawnTaskID,
                    RoomID = data.RoomID
                };

                _client.SendMessage(Message.Create(MessageTags.CompleteSpawnProcess, packet), SendMode.Reliable);
            }
        }

        private void HandleRegisterSpawnedProcessSuccess(IClient client, Message message)
        {
            WriteEvent("We've registered this process to the master. Starting room...", LogType.Info);

            
            
            // 1. Create options object
            var options = new RoomOptions
            {
                RoomName = RoomName,
                WorldName = WorldName,
                MaxPlayers = MaxPlayers,
                IsPublic = IsPublic
            };

            // 2. Send a request to create a room
            _client.SendMessage(Message.Create(MessageTags.RegisterRoom, options), SendMode.Reliable);
        }

        private void GameOnResumed()
        {
            WriteEvent("GameOnResumed", LogType.Info);
        }


        public virtual void CreateAccess(UsernameAndPeerIdPacket requester, RoomAccessProviderCallback callback)
        {
            callback.Invoke(new RoomAccessPacket()
            {
                Token = Guid.NewGuid().ToString()
            }, null);
        }
    }
}