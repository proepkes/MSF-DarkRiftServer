using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DarkRift.Server;

namespace RoomHandler
{
    public class RoomHandlerPlugin : Plugin
    {
        private int _nextRoomID = 0;

        public override Version Version => new Version(1, 0, 0);
        public override bool ThreadSafe => true;

        public RoomHandlerPlugin(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
        }
    }
}
