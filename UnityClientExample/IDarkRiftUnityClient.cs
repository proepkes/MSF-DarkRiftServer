using System;
using DarkRift;
using DarkRift.Client;

namespace UnityClientExample
{
    public interface IDarkRiftUnityClient
    {
        bool Connected { get; }

        bool SendMessage(Message create, SendMode reliable);
        bool Disconnect();

        event EventHandler<DisconnectedEventArgs> Disconnected;
        event EventHandler<MessageReceivedEventArgs> MessageReceived;
    }
}