using System;
using System.IO;
using System.Net;
using System.Reflection;
using DarkRift;
using DarkRift.Client;
using DarkRift.Server;
using ServerPlugins;

namespace WorldPlugins.Time
{
    /// <summary>
    ///     This Plugin goes to the spawned server
    /// </summary>
    public class TimePlugin : DefaultWorldPlugin
    {
        public TimePlugin(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
        }

        protected override void OnMessagereceived(object sender, DarkRift.Server.MessageReceivedEventArgs e)
        {
        }
    }
}