using DarkRift;
using Utils;
using Utils.Packets;

namespace ServerPlugins.Game.Components
{
    public class SpawnComponent : Component
    {
        public override void Start()
        {
            Entity.Client.SendMessage(Message.Create(MessageTags.SpawnEntity, new SpawnEntityPacket { ID = Entity.ID, Position = Entity.Position }), SendMode.Reliable);
            foreach (var observer in Entity.Observers)
            {
                observer.Client.SendMessage(Message.Create(MessageTags.SpawnEntity, new SpawnEntityPacket { ID = Entity.ID, Position = Entity.Position }), SendMode.Reliable);
            }
        }
    }
}