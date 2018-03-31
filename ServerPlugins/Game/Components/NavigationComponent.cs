using System.Numerics;
using DarkRift;
using RecastDetour.Detour;
using Utils;
using Utils.Game;
using Utils.Packets;

namespace ServerPlugins.Game.Components
{
    public class NavigationComponent : Component
    {
        private int agentID;

        public float StoppingDistance = 0;

        public TundraNetPosition Destination { get; private set; }

        public bool IsDirty { get; set; }

        public NavMeshQuery NavMeshQuery;

        public void SetDestination(TundraNetPosition destination)
        {
            IsDirty = true;
            Destination = destination;
        }

        public void Navigate()
        {
            var path = Pathfinder.ComputeSmoothPath(NavMeshQuery, Entity.Position, Destination);
            foreach (var observer in Entity.Observers)
            {
                observer.Client.SendMessage(Message.Create(MessageTags.NavigateTo,
                    new AckNavigateToPacket
                    {
                        EntityID = Entity.ID,
                        Path = path,
                        StoppingDistance = StoppingDistance
                    }), SendMode.Reliable);
            }

            IsDirty = false;
        }

        public Vector3 Velocity => Vector3.Zero;

        //public float RemainingDistance 
        //{
        //    get
        //    {
        //        var direction = Destination - Entity.Position;
        //        direction.Y = 0;
        //        return direction.Length();
        //    }
        //}

        public void Reset()
        {
        }
    }
}