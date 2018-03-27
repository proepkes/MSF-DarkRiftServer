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
            //Currently we just relay the current destination
            //The idea is to just send a destination and let client & server use the same pathfinding
            //So we don't need to sync & lerp position and rotations
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

        //TODO: Big todo.. pathfinding, (SharpNav?)
        public override void Update()
        {
            
        }

        public void Reset()
        {

        }
    }
}