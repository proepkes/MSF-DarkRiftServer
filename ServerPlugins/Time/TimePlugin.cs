using System;
using System.IO;
using System.Net;
using System.Reflection;
using DarkRift;
using DarkRift.Client;
using DarkRift.Server;
using ServerPlugins;
using Utils;
using Utils.Packets;

namespace WorldPlugins.Time
{
    /// <summary>
    ///     This Plugin goes to the spawned server
    /// </summary>
    public class TimePlugin : ServerPluginBase
    {
        //time since this plugin was loaded
        private readonly long _startTime;
        

        public TimePlugin(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            _startTime = DateTime.Now.Ticks;
        }

        
        protected override void Loaded(LoadedEventArgs args)
        {
            base.Loaded(args);
            SetHandler(MessageTags.GetNetworkTime, HandleGetNetworkTime);
        }

        private void HandleGetNetworkTime(IClient client, Message message)
        {
            client.SendMessage(Message.Create(MessageTags.GetNetworkTime,
                    new FloatPacket {Data = (float) TimeSpan.FromTicks(DateTime.Now.Ticks - _startTime).TotalSeconds}),
                SendMode.Reliable);
        }
    }
}