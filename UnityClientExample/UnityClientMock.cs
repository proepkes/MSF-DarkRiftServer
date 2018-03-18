using System;
using DarkRift;
using DarkRift.Client;

namespace UnityClientExample
{
    public abstract class UnityClientMock
    {
        public event EventHandler<DisconnectedEventArgs> Disconnected;
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;
        public bool Connected { get; set; }

        public abstract bool SendMessage(Message create, SendMode reliable);
        public abstract bool Disconnect();
    }
}