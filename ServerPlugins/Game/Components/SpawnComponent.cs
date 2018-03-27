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
                        //if the observer is the player himself, he has authority, 
                        //check this field on client-side to either spawn player-prefab or networkview-prefab
                        HasAuthority = observer.ID == Entity.ID
                    }), SendMode.Reliable);
            }
        }
    }
}