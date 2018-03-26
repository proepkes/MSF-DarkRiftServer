using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DarkRift;
using DarkRift.Server;
using Utils;
using Utils.Messages.Responses;
using Utils.Packets;

namespace ServerPlugins.RoomHandler
{
    public class RoomHandlerPlugin : ServerPluginBase
    {
        private int _nextRoomID;
        private readonly List<RegisteredRoom> _rooms;

        public override bool ThreadSafe => false;

        public RoomHandlerPlugin(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            _rooms = new List<RegisteredRoom>();
            ClientManager.ClientDisconnected += OnClientDisconnected;
        }
        protected override void Loaded(LoadedEventArgs args)
        {
            base.Loaded(args);
            SetHandler(MessageTags.RegisterRoom, HandleRegisterRoom);
            SetHandler(MessageTags.GetRoomAccess, HandleGetRoomAccess);
            SetHandler(MessageTags.ValidateRoomAccess, HandleValidateRoomAccess);

            Task.Run(() => CleanUnconfirmedAccesses());
        }

        private void CleanUnconfirmedAccesses()
        {
            while (true)
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));

                foreach (var registeredRoom in _rooms)
                {
                    registeredRoom.ClearTimedOutAccesses();
                }
            }
        }

        private void OnClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            foreach (var room in _rooms.Where(registeredRoom => registeredRoom.Client.ID == e.Client.ID))
            {
                room.Destroy();
            }
            // Remove the room from all rooms
            _rooms.RemoveAll(room => room.Client.ID == e.Client.ID);
        }

        private void HandleValidateRoomAccess(IClient client, Message message)
        {
            var data = message.Deserialize<RoomAccessValidatePacket>();
            if (data != null)
            {
                var room = _rooms.FirstOrDefault(registeredRoom => registeredRoom.ID == data.RoomID);
                
                if (room == null)
                {
                    client.SendMessage(Message.Create(MessageTags.ValidateRoomAccessFailed, new IntPacket {Data = data.ClientID }), SendMode.Reliable);
                    return;
                }

                if (client != room.Client)
                {
                    client.SendMessage(Message.Create(MessageTags.ValidateRoomAccessFailed, new IntPacket { Data = data.ClientID }), SendMode.Reliable);
                    return;
                }


                if (!room.ValidateAccess(data.Token, out var playerClient))
                {
                    client.SendMessage(Message.Create(MessageTags.ValidateRoomAccessFailed, new IntPacket { Data = data.ClientID }), SendMode.Reliable);
                    return;
                }

                // Respond with success and player's peer id
                client.SendMessage(Message.Create(MessageTags.ValidateRoomAccessSuccess,
                    new RoomAccessValidatedPacket
                    {
                        ClientID = data.ClientID,
                        MasterClientID = playerClient.ID,
                        
                    }), SendMode.Reliable);
            }
        }

        private void HandleGetRoomAccess(IClient client, Message message)
        {
            var data = message.Deserialize<RoomAccessRequestPacket>();
            if (data != null)
            {
                var room = _rooms.FirstOrDefault(registeredRoom => registeredRoom.ID == data.RoomId);

                if (room == null)
                {
                    client.SendMessage(
                        Message.Create(MessageTags.GetRoomAccessFailed,
                            new FailedMessage {Reason = "Room does not exist", Status = ResponseStatus.Failed}),
                        SendMode.Reliable);
                    return;
                }
                
                // Send room access request to peer who owns it
                room.GetAccess(client, (packet, error) =>
                {
                    if (packet == null)
                    {
                        client.SendMessage(Message.Create(MessageTags.GetRoomAccessFailed, new FailedMessage {Reason = "Access denied", Status = ResponseStatus.Unauthorized}), SendMode.Reliable);
                        return;
                    }
                    
                    client.SendMessage(Message.Create(MessageTags.GetRoomAccessSuccess, packet), SendMode.Reliable);
                });
            }
        }

        private void HandleRegisterRoom(IClient client, Message message)
        {
            if (!HasRoomRegistrationPermissions(client))
            {
                client.SendMessage(
                    Message.Create(MessageTags.RegisterRoomFailed,
                        new FailedMessage { Reason = "Insufficient permissions", Status = ResponseStatus.Unauthorized }),
                    SendMode.Reliable);
                return;
            }

            var data = message.Deserialize<RoomOptions>();
            if (data != null)
            {

                var room = RegisterRoom(client, data);

                // Respond with a room id
                client.SendMessage(Message.Create(MessageTags.RegisterRoomSuccess, new RegisterRoomSuccessMessage { Status = ResponseStatus.Success, RoomID = room.ID }),
                    SendMode.Reliable);
            }
            else
            {

                client.SendMessage(
                    Message.Create(MessageTags.RegisterRoomFailed,
                        new FailedMessage { Reason = "Unable to read data", Status = ResponseStatus.Failed }),
                    SendMode.Reliable);
            }
        }

        private bool HasRoomRegistrationPermissions(IClient client)
        {
            //TODO: Check room-register permissions
            return true;
        }

        private RegisteredRoom RegisterRoom(IClient client, RoomOptions options)
        {
            // Create the object
            var room = new RegisteredRoom(GenerateRoomId(), client, options);


            // Add the room to a list of all rooms
            _rooms.Add(room);

            return room;
        }

        private int GenerateRoomId()
        {
            //return Interlocked.Increment(ref _nextRoomID);
            return _nextRoomID++;
        }
    }
}