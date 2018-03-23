using System;
using DarkRift.Server;

namespace ServerPlugins
{
    public abstract class DefaultServerPlugin : Plugin
    {
        public override bool ThreadSafe => true;
        public override Version Version => new Version(1, 0, 0);

        protected DefaultServerPlugin(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
        }
    }
}