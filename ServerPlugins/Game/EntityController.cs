using DarkRift.Server;

namespace ServerPlugins.Game
{
    public class EntityController
    {
        private readonly IClient _client;

        public EntityController(IClient client)
        {
            _client = client;
        }
    }
}