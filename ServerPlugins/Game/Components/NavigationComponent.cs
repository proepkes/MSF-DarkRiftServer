using System.Numerics;
using DarkRift;
using Utils;
using Utils.Game;
using Utils.Packets;

namespace ServerPlugins.Game.Components
{
    public class NavigationComponent : Component
    {
        private int agentID;

        public float StoppingDistance = 0;

        public Vector3 Destination { get; private set; }

        public bool IsDirty { get; set; }

        public void SetDestination(Vector3 destination)
        {
            IsDirty = true;
            Destination = destination;
        }

        public override void Start()
        {
        }

        public void Navigate()
        {
            //TODO: Authoritative Destination
            //Currently we just relay the current destination
            //The idea is to just send a destination and let client & server use the same pathfinding
            //So we don't need to sync & lerp position and rotations

            var destination = TundraNetPosition.Create(Destination.X, Destination.Y, Destination.Z);
            foreach (var observer in Entity.Observers)
            {
                observer.Client.SendMessage(Message.Create(MessageTags.NavigateTo,
                    new AckNavigateToPacket
                    {
                        EntityID = Entity.ID,
                        Destination = destination,
                        StoppingDistance = StoppingDistance
                    }), SendMode.Reliable);
            }

            IsDirty = false;
        }

        //TODO: Big todo.. pathfinding (SharpNav)
        public override void Update()
        {
            
        }

        public void Reset()
        {

        }

        public Vector3 Velocity => Vector3.Zero;

        public float RemainingDistance 
        {
            get
            {
                var direction = Destination - Entity.Position;
                direction.Y = 0;
                return direction.Length();
            }
        }
    }
}