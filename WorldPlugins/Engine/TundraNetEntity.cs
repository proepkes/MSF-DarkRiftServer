using DarkRift.Server;
using Urho;

namespace WorldEngine
{
    public class TundraNetEntity : Component
    {
        public TundraNetEntity(IClient client)
        {
            Client = client;
        }

        public IClient Client { get; set; }
    }
}