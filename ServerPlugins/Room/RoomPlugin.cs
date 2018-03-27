using System;
using System.Collections.Generic;
using System.Net;
using DarkRift;
using DarkRift.Client;
using DarkRift.Server;
using ServerPlugins.Game;
using ServerPlugins.Game.Entities;
using Utils;
using Utils.Messages.Notifications;
using Utils.Messages.Requests;
using Utils.Messages.Responses;
using Utils.Packets;
using MessageReceivedEventArgs = DarkRift.Client.MessageReceivedEventArgs;

namespace ServerPlugins.Room
{
    public delegate void RoomAccessProviderCallback(RoomAccessPacket access, string error);
    public delegate void RoomAccessProvider(UsernameAndPeerIdPacket requester, RoomAccessProviderCallback giveAccess);

    /// <summary>
    ///     This Plugin goes to the spawned server
    /// </summary>
    public class RoomPlugin : ServerPluginBase
    {
        // ReSharper disable InconsistentNaming
        private int RoomID;

        private readonly int SpawnTaskID;

        private readonly string SpawnCode;
        // ReSharper restore InconsistentNaming

        //Connection to master
        private readonly DarkRiftClient _client;
        
        public override bool ThreadSafe => false;

        
        public IPAddress MasterIpAddress { get; set; }
        public int MasterPort { get; set; }
        public int MaxPlayers { get; set; }
        public bool IsPublic { get; set; }
        public string WorldName { get; set; }
        public string RoomName { get; set; }
        public string Region { get; set; }
        public bool IsRoomRegistered { get; protected set; }
        public int AssignedPort { get; set; }
        public string MachineIp { get; set; }


        private GamePlugin _game;
        private RoomAccessProvider _accessProvider;
        private readonly Dictionary<int, IClient> _pendingAccessValidations;

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
            Region = pluginLoadData.Settings.Get(nameof(Region));
            AssignedPort = Convert.ToInt32(pluginLoadData.Settings.Get(nameof(AssignedPort)));
            MachineIp = pluginLoadData.Settings.Get(nameof(MachineIp));

            _client = new DarkRiftClient();
            _pendingAccessValidations = new Dictionary<int, IClient>();

        }

        protected override void Loaded(LoadedEventArgs loadedArgs)
        {
            base.Loaded(loadedArgs);

            _game = PluginManager.GetPluginByType<GamePlugin>();

            WriteEvent("Connecting to " + MasterIpAddress + ":" + MasterPort, LogType.Info);
            _client.MessageReceived += OnMessageFromMaster;
            _client.ConnectInBackground(MasterIpAddress, MasterPort, IPVersion.IPv4, OnConnectedToMaster);
            
            ClientManager.ClientConnected += OnPlayerConnected;
        }

        private void OnPlayerConnected(object sender, ClientConnectedEventArgs e)
        {
            e.Client.MessageReceived += OnMessageFromPlayer;
        }

        private void OnMessageFromPlayer(object sender, DarkRift.Server.MessageReceivedEventArgs e)
        {
            var message = e.GetMessage();
            if (message != null)
            {
                switch (message.Tag)
                {
                    case MessageTags.AccessRoom:
                        HandleAccessRoom(e.Client, message);
                        break;
                }
            }
        }

        private void OnMessageFromMaster(object client, MessageReceivedEventArgs args)
        {
            var message = args.GetMessage();
            if (message != null)
            {
                switch (message.Tag)
                {
                    case MessageTags.RegisterRoomSuccess:
                        HandleRegisterRoomSuccess(message);
                        break;
                    case MessageTags.RegisterSpawnedProcessSuccess:
                        HandleRegisterSpawnedProcessSuccess(message);
                        break;
                    case MessageTags.CompleteSpawnProcessSuccess:
                        WriteEvent("Ready for players.", LogType.Info);
                        break;
                    case MessageTags.CompleteSpawnProcessFailed:
                        var msg = message.Deserialize<FailedMessage>();
                        WriteEvent("CompleteSpawnProcess failed: " + msg.Status + "(" + msg.Reason + ")", LogType.Warning);
                        break;
                    case MessageTags.ProvideRoomAccessCheck:
                        HandleProvideRoomAccessCheck(message);
                        break;
                    case MessageTags.ValidateRoomAccessSuccess:
                        HandleValidateRoomAccessSuccess(message);
                        break;
                    case MessageTags.ValidateRoomAccessFailed:
                        HandleValidateRoomAccessFailed(message);
                        break;
                }
            }
        }

        private void HandleValidateRoomAccessSuccess(Message message)
        {
            var data = message.Deserialize<RoomAccessValidatedPacket>();
            if (data != null)
            {
                if (_pendingAccessValidations.ContainsKey(data.ClientID))
                {
                    var validatedClient = _pendingAccessValidations[data.ClientID];
                    _pendingAccessValidations.Remove(data.ClientID);

                    WriteEvent("Confirmed token access for client: " + validatedClient.ID, LogType.Info);

                    //// Get account info
                    //Msf.Server.Auth.GetPeerAccountInfo(validatedClient.PeerId, (info, errorMsg) =>
                    //{
                    //    if (info == null)
                    //    {
                    //        Logger.Error("Failed to get account info of peer " + validatedClient.PeerId + "" +
                    //                     ". Error: " + errorMsg);
                    //        return;
                    //    }

                    //    Logger.Debug("Got peer account info: " + info);

                    //    var player = new UnetMsfPlayer(netmsg.conn, info);

                    //    OnPlayerJoined(player);
                    //});

                    _game.AddEntity(new Player(validatedClient) { Name = "Player"});
                }
            }
        }

        private void HandleValidateRoomAccessFailed(Message message)
        {
            var data = message.Deserialize<IntPacket>();
            if (data != null)
            {
                if (_pendingAccessValidations.ContainsKey(data.Data))
                {
                    _pendingAccessValidations[data.Data].Disconnect();
                }
            }
        }

        private void HandleAccessRoom(IClient client, Message message)
        {
            if(_pendingAccessValidations.ContainsKey(client.ID))
                return;
            
            var data = message.Deserialize<StringPacket>();
            if (data != null)
            {
                var token = data.Data;

                _pendingAccessValidations[client.ID] = client;

                //Ask master for validation
                _client.SendMessage(Message.Create(MessageTags.ValidateRoomAccess, new RoomAccessValidatePacket {ClientID = client.ID, RoomID = RoomID, Token = token}), SendMode.Reliable);
            }
        }

        private void HandleProvideRoomAccessCheck(Message message)
        {
            var data = message.Deserialize<RoomAccessProvideCheckPacket>();
            if (data != null)
            {
                //We accept every client
                _client.SendMessage(Message.Create(MessageTags.ProvideRoomAccessCheckSuccess,
                    new RoomAccessPacket
                {
                    ClientID = data.ClientID,
                    RoomIp = MachineIp,
                    RoomPort = AssignedPort,
                    RoomID = RoomID,
                    Token = Guid.NewGuid().ToString(),
                    RoomName = RoomName
                }),SendMode.Reliable);
            }
        }

        private void OnConnectedToMaster(Exception exception)
        {
            WriteEvent("Connected to Master", LogType.Info);
            _client.SendMessage(Message.Create(MessageTags.RegisterSpawnedProcess,
                    new RegisterSpawnedProcessMessage {SpawnTaskID = SpawnTaskID, SpawnCode = SpawnCode}),
                SendMode.Reliable);
        }
        private void HandleRegisterRoomSuccess(Message message)
        {
            var data = message.Deserialize<RegisterRoomSuccessMessage>();
            if (data != null)
            {
                IsRoomRegistered = true;

                RoomID = data.RoomID;


                var packet = new SpawnFinalizedMessage
                {
                    SpawnTaskID = SpawnTaskID,
                    RoomID = data.RoomID
                };

                _client.SendMessage(Message.Create(MessageTags.CompleteSpawnProcess, packet), SendMode.Reliable);
            }
        }

        private void HandleRegisterSpawnedProcessSuccess(Message message)
        {
            WriteEvent("Starting room...", LogType.Info);

            _game.Started += () =>
            {
                // 1. Create options object
                var options = new RoomOptions
                {
                    RoomName = RoomName,
                    WorldName = WorldName,
                    MaxPlayers = MaxPlayers,
                    IsPublic = IsPublic,
                    Region = Region,
                };

                // 2. Send a request to create a room
                _client.SendMessage(Message.Create(MessageTags.RegisterRoom, options), SendMode.Reliable);
            };

            //Run game logic here before the room will be registered and opened for players
            //Example: generate a seed for procedural levels, generate navmeshes, load essential gamedata
            _game.LoadLevel(RoomName);
            _game.Start();
            
        }
    }
}