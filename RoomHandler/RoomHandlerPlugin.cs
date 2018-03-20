using System;
using System.Collections.Generic;
using DarkRift;
using DarkRift.Server;
using RoomLib;
using Utils;
using Utils.Messages.Responses;

namespace RoomHandler
{
    public class RoomHandlerPlugin : Plugin
    {
        private int _nextRoomID = 0;
        private int _roomIdGenerator;
        private List<RegisteredRoom> Rooms;

        public override Version Version => new Version(1, 0, 0);
        public override bool ThreadSafe => true;

        public RoomHandlerPlugin(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            Rooms = new List<RegisteredRoom>();
            ClientManager.ClientConnected +=OnClientConnected;
        }

        private void OnClientConnected(object sender, ClientConnectedEventArgs e)
        {
            e.Client.MessageReceived += OnMessageReceived;
        }

        private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            using (var message = e.GetMessage())
            {
                switch (message.Tag)
                {
                    case MessageTags.RegisterRoom:
                        HandleRegisterRoom(e.Client, message);
                        break;
                        
                }
            }
        }

        private void HandleRegisterRoom(IClient client, Message message)
        {
            if (!HasRoomRegistrationPermissions(client))
            {
                client.SendMessage(
                    Message.Create(MessageTags.RegisterRoomFailed,
                        new FailedMessage {Reason = "Insufficient permissions", Status = ResponseStatus.Unauthorized}),
                    SendMode.Reliable);
                return;
            }

            var data = message.Deserialize<RoomOptions>();
            if (data != null)
            {

                var room = RegisterRoom(client, data);

                // Respond with a room id
                client.SendMessage(Message.Create(MessageTags.RegisterRoomSuccess, new RegisterRoomSuccessMessage { Status = ResponseStatus.Success, RoomID = room.ID}),
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
            Rooms.Add(room);
            
            return room;
        }

        private int GenerateRoomId()
        {
            return _roomIdGenerator++;
        }
    }
}