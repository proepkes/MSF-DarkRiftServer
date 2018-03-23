using DarkRift;
using DarkRift.Server;
using Utils;

namespace ServerPlugins.Game
{
    /// <summary>
    ///     This Plugin goes to the spawned server
    /// </summary>
    public class GamePlugin : ServerPluginBase
    {
        private float time;

        public GamePlugin(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
        }

        protected override void Loaded(LoadedEventArgs args)
        {
            base.Loaded(args);
            SetHandler(MessageTags.GetNetworkTime, HandleGetNetworkTime);
        }

        private void HandleGetNetworkTime(IClient client, Message message)
        {
            
        }
    }
}