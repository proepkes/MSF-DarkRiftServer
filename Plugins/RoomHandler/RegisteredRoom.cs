using System;
using System.Collections.Generic;
using System.Linq;
using DarkRift.Server;
using Utils;
using Utils.Packets;

namespace ServerPlugins.RoomHandler
{
    /// <summary>
    ///     This is an instance of the room in master server
    /// </summary>
    public class RegisteredRoom
    {
        public delegate void GetAccessCallback(RoomAccessPacket access, string error);

        private readonly Dictionary<int, RoomAccessPacket> _accessesInUse;
        private readonly HashSet<int> _pendingRequests;

        private readonly Dictionary<int, IClient> _players;
        private readonly Dictionary<string, RoomAccessData> _unconfirmedAccesses;

        public RoomOptions Options { get; private set; }
        public int ID { get; }
        public IClient Peer { get; }

        public int OnlineCount => _accessesInUse.Count;

        public RegisteredRoom(int id, IClient peer, RoomOptions options)
        {
            ID = id;
            Peer = peer;
            Options = options;

            _unconfirmedAccesses = new Dictionary<string, RoomAccessData>();
            _players = new Dictionary<int, IClient>();
            _accessesInUse = new Dictionary<int, RoomAccessPacket>();
            _pendingRequests = new HashSet<int>();
        }

        public event Action<IClient> PlayerJoined;
        public event Action<IClient> PlayerLeft;

        public event Action<RegisteredRoom> Destroyed;

        public void ChangeOptions(RoomOptions options)
        {
            Options = options;
        }


        /// <summary>
        ///     Sends a request to room, to retrieve an access to it for a specified peer,
        ///     with some extra properties
        /// </summary>
        public void GetAccess(IClient client)
        {
            // If request is already pending
            if (_pendingRequests.Contains(client.ID)) return;

            // If player is already in the game
            if (_players.ContainsKey(client.ID)) return;

            // If player has already received an access and didn't claim it
            // but is requesting again - send him the old one
            var currentAccess = _unconfirmedAccesses.Values.FirstOrDefault(v => v.Peer == client);
            if (currentAccess != null)
            {
                // Restore the timeout //TODO: Timeout-Setting (no fixed value of 20)
                currentAccess.Timeout = DateTime.Now.AddSeconds(20);

                //callback.Invoke(currentAccess.Access, null);
                return;
            }

            // If there's a player limit
            if (Options.MaxPlayers != 0)
            {
                var playerSlotsTaken = _pendingRequests.Count
                                       + _accessesInUse.Count
                                       + _unconfirmedAccesses.Count;

                if (playerSlotsTaken >= Options.MaxPlayers) return;
            }

            var packet = new RoomAccessProvideCheckPacket
            {
                PeerId = client.ID,
                RoomId = ID
            };

            //// Add the username if available
            //var userExt = peer.GetExtension<IUserExtension>();
            //if (userExt != null && !string.IsNullOrEmpty(userExt.Username))
            //{
            //    packet.Username = userExt.Username;
            //}

            // Add to pending list
            _pendingRequests.Add(client.ID);

            //Peer.SendMessage((short) MsfOpCodes.ProvideRoomAccessCheck, packet, (status, response) =>
            //{
            //    // Remove from pending list
            //    _pendingRequests.Remove(peer.Id);

            //    if (status != ResponseStatus.Success)
            //    {
            //        callback.Invoke(null, response.AsString("Unknown Error"));
            //        return;
            //    }

            //    var accessData = response.Deserialize(new RoomAccessPacket());

            //    var access = new RoomAccessData()
            //    {
            //        Access = accessData,
            //        Peer = peer,
            //        Timeout = DateTime.Now.AddSeconds(Options.AccessTimeoutPeriod)
            //    };

            //    // Save the access
            //    _unconfirmedAccesses[access.Access.Token] = access;

            //    callback.Invoke(access.Access, null);
            //});
        }

        /// <summary>
        ///     Checks if access token is valid
        /// </summary>
        /// <param name="token"></param>
        /// <param name="peer"></param>
        /// <returns></returns>
        public bool ValidateAccess(string token, out IClient peer)
        {
            RoomAccessData data;
            _unconfirmedAccesses.TryGetValue(token, out data);

            peer = null;

            // If there's no data
            if (data == null)
                return false;

            // Remove unconfirmed
            _unconfirmedAccesses.Remove(token);

            // If player is no longer connected
            if (!data.Peer.IsConnected)
                return false;

            // Set access as used
            _accessesInUse.Add(data.Peer.ID, data.Access);

            peer = data.Peer;

            // Invoke the event
            if (PlayerJoined != null)
                PlayerJoined.Invoke(peer);

            return true;
        }

        /// <summary>
        ///     Clears all of the accesses that have not been confirmed in time
        /// </summary>
        public void ClearTimedOutAccesses()
        {
            var timedOut = _unconfirmedAccesses.Values.Where(u => u.Timeout < DateTime.Now).ToList();

            foreach (var access in timedOut) _unconfirmedAccesses.Remove(access.Access.Token);
        }

        public void OnPlayerLeft(int peerId)
        {
            _accessesInUse.Remove(peerId);

            IClient playerPeer;
            _players.TryGetValue(peerId, out playerPeer);

            if (playerPeer == null)
                return;

            if (PlayerLeft != null)
                PlayerLeft.Invoke(playerPeer);
        }

        public void Destroy()
        {
            if (Destroyed != null)
                Destroyed.Invoke(this);

            _unconfirmedAccesses.Clear();

            // Clear listeners
            PlayerJoined = null;
            PlayerLeft = null;
            Destroyed = null;
        }

        private class RoomAccessData
        {
            public RoomAccessPacket Access;
            public IClient Peer;
            public DateTime Timeout;
        }
    }
}