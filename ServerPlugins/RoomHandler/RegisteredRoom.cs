using System;
using System.Collections.Generic;
using System.Linq;
using DarkRift;
using DarkRift.Server;
using Utils;
using Utils.Messages.Responses;
using Utils.Packets;

namespace ServerPlugins.RoomHandler
{
    /// <summary>
    ///     This is an instance of the room in master server
    /// </summary>
    public class RegisteredRoom
    {
        private class ClientEquality : IEqualityComparer<IClient>
        {
            public bool Equals(IClient x, IClient y)
            {
                return x.ID == y.ID;
            }

            public int GetHashCode(IClient obj)
            {
                return obj.ID;
            }
        }

        public delegate void GetAccessCallback(RoomAccessPacket access, string error);

        private readonly Dictionary<int, RoomAccessPacket> _accessesInUse;
        private readonly Dictionary<IClient, GetAccessCallback> _pendingRequests;

        private readonly Dictionary<int, IClient> _players;
        private readonly Dictionary<string, RoomAccessData> _unconfirmedAccesses;

        public int ID { get; }
        public IClient Client { get; }
        public RoomOptions Options { get; private set; }

        public int OnlineCount => _accessesInUse.Count;


        public RegisteredRoom(int id, IClient client, RoomOptions options)
        {
            ID = id;
            Client = client;
            Options = options;

            _unconfirmedAccesses = new Dictionary<string, RoomAccessData>();
            _players = new Dictionary<int, IClient>();
            _accessesInUse = new Dictionary<int, RoomAccessPacket>();
            _pendingRequests = new Dictionary<IClient, GetAccessCallback>(new ClientEquality());

            //Connection from masterserver to room
            Client.MessageReceived += OnMessageReceived;
        }

        private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            var message = e.GetMessage();
            if (message != null)
            {
                switch (message.Tag)
                {
                    case MessageTags.ProvideRoomAccessCheckSuccess:
                        HandleProvideRoomAccessCheckSuccess(message);
                        break;
                }
            }
        }

        private void HandleProvideRoomAccessCheckSuccess(Message message)
        {
            var data = message.Deserialize<RoomAccessPacket>();
            if (data != null)
            {
                var client = _pendingRequests.Keys.FirstOrDefault(key => key.ID == data.ClientID);
                if (client != null)
                {
                    //call the callback
                    _pendingRequests[client](data, null);

                    var access = new RoomAccessData
                    {
                        Access = data,
                        Client = client,
                        Timeout = DateTime.Now.AddSeconds(20)
                    };
                    _unconfirmedAccesses[data.Token] = access;
                    _pendingRequests.Remove(client);
                }
            }
        }

        /// <summary>
        ///     Sends a request to room, to retrieve an access to it for a specified peer,
        ///     with some extra properties
        /// </summary>
        public void GetAccess(IClient client, GetAccessCallback callback)
        {
            // If request is already pending
            if (_pendingRequests.ContainsKey(client))
            {
                callback.Invoke(null, "You've already requested an access to this room");
                return;
            }

            // If player is already in the game
            if (_players.ContainsKey(client.ID))
            {
                callback.Invoke(null, "You are already in this room");
                return;
            }

            // If player has already received an access and didn't claim it
            // but is requesting again - send him the old one
            var currentAccess = _unconfirmedAccesses.Values.FirstOrDefault(v => v.Client.ID == client.ID);
            if (currentAccess != null)
            {
                // Restore the timeout //TODO: Timeout-Setting (no fixed value of 20)
                currentAccess.Timeout = DateTime.Now.AddSeconds(20);

                callback.Invoke(currentAccess.Access, null);
                return;
            }

            // If there's a player limit
            if (Options.MaxPlayers != 0)
            {
                var playerSlotsTaken = _pendingRequests.Count
                                       + _accessesInUse.Count
                                       + _unconfirmedAccesses.Count;

                if (playerSlotsTaken >= Options.MaxPlayers)
                {
                    callback.Invoke(null, "Room is already full");
                    return;
                }
            }

            var packet = new RoomAccessProvideCheckPacket
            {
                ClientID = client.ID,
                RoomID = ID
            };
            
            // Add to pending list
            _pendingRequests[client] = callback;

            Client.SendMessage(Message.Create(MessageTags.ProvideRoomAccessCheck, packet), SendMode.Reliable);
        }

        public bool ValidateAccess(string token, out IClient client)
        {
            _unconfirmedAccesses.TryGetValue(token, out var data);

            client = null;

            if (data == null)
                return false;

            _unconfirmedAccesses.Remove(token);

            if (!data.Client.IsConnected)
                return false;

            _accessesInUse.Add(data.Client.ID, data.Access);

            client = data.Client;

            return true;
        }

        public void ClearTimedOutAccesses()
        {
            var timedOut = _unconfirmedAccesses.Values.Where(u => u.Timeout < DateTime.Now).ToList();

            foreach (var access in timedOut)
                _unconfirmedAccesses.Remove(access.Access.Token);
        }

        public void OnPlayerLeft(int peerId)
        {
            _accessesInUse.Remove(peerId);

            _players.TryGetValue(peerId, out var playerPeer);

            if (playerPeer == null)
                return;
        }

        public void Destroy()
        {
            _unconfirmedAccesses.Clear();

        }

        private class RoomAccessData
        {
            public RoomAccessPacket Access;
            public IClient Client;
            public DateTime Timeout;
        }
    }
}