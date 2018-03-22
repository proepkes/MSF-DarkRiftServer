using DarkRift.Server;

namespace WorldEngine
{
    public class TundraNetEntity
    {
        public TundraNetEntity(IClient client)
        {
            Client = client;
        }

        public int ID { get; set; }

        public IClient Client { get; set; }
    }
}