using DarkRift;
using Utils;
using Utils.Packets;

namespace ServerPlugins.Game.Components
{
    public class SpawnComponent : Component
    {
        public override void Start()
        {
            foreach (var observer in Entity.Observers)
            {
                observer.Client.SendMessage(Message.Create(MessageTags.SpawnEntity,
                    new SpawnEntityPacket
                    {
                        ID = Entity.ID,
                        Position = Entity.Position,
                        HasAuthority = observer == Entity //if the observer is the player himself, he has authority
                    }), SendMode.Reliable);
            }
        }
    }
}