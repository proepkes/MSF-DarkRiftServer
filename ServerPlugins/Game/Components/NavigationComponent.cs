using DarkRift;
using Utils;
using Utils.Game;
using Utils.Packets;

namespace ServerPlugins.Game.Components
{
    public class NavigationComponent : Component
    {
        public float StoppingDistance = 0;

        public TundraNetPosition Destination = TundraNetPosition.Create(0f, 0f, 0f);

        public bool IsDirty { get; set; }

        public void Navigate()
        {
            //TODO: Authoritative Destination
            foreach (var observer in Entity.Observers)
            {
                observer.Client.SendMessage(Message.Create(MessageTags.NavigateTo,
                    new AckNavigateToPacket
                    {
                        EntityID = Entity.ID,
                        Destination = Destination,
                        StoppingDistance = StoppingDistance
                    }), SendMode.Reliable);
            }

            IsDirty = false;
        }

        //TODO: Big todo.. pathfinding D:
        public override void Update()
        {
            
        }

        public void Reset()
        {

        }
    }
}