using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DarkRift;
using DarkRift.Server;
using Utils;
using Utils.Messages.Responses;

namespace ServerPlugins.RoomHandler
{
    public class RoomHandlerPlugin : DefaultServerPlugin
    {
        private int _nextRoomID;
        private readonly List<RegisteredRoom> _rooms;

        public override Version Version => new Version(1, 0, 0);
        public override bool ThreadSafe => true;

        public RoomHandlerPlugin(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            _rooms = new List<RegisteredRoom>();
            ClientManager.ClientDisconnected += OnClientDisconnected;
        }

        private void OnClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            lock (_rooms)
            {
                foreach (var room in _rooms.Where(registeredRoom => registeredRoom.Client.ID == e.Client.ID))
                {
                    room.Destroy();
                }
                // Remove the room from all rooms
                _rooms.RemoveAll(room => room.Client.ID == e.Client.ID);
            }

        }

        protected override void OnMessagereceived(object sender, MessageReceivedEventArgs e)
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
            lock (_rooms)
            {
                _rooms.Add(room);
            }

            return room;
        }

        private int GenerateRoomId()
        {
            return Interlocked.Increment(ref _nextRoomID);
        }
    }
}