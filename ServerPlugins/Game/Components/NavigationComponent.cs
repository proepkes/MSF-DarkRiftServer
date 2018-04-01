using System.Linq;
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

        public float Speed = 3f;
        public float StoppingDistance = 0;
        
        public NavMeshQuery NavMeshQuery;

        public bool HasPath
        {
            get { return currentPath != null; }
        }

        private int currentPathIndex;
        private SmoothPath currentPath;
        private TundraVector3 currentDestination;

        public void Navigate(TundraVector3 to)
        {
            currentPath = Pathfinder.ComputeSmoothPath(NavMeshQuery, Entity.Position, to);
            if (currentPath.Points.Count > 0)
            {
                currentPathIndex = 0;
                currentDestination = currentPath.Points[0];
                Entity.Game.Log("Destination: " + currentPath.Last(), LogType.Info);
                //Entity.Position = currentPath.Last();
                foreach (var observer in Entity.Observers)
                {
                    observer.Client.SendMessage(Message.Create(MessageTags.NavigateTo,
                        new AckNavigateToPacket
                        {
                            EntityID = Entity.ID,
                            Path = currentPath,
                            StoppingDistance = StoppingDistance
                        }), SendMode.Reliable);
                }
            }
        }

       // public Vector3 Velocity => Vector3.Zero;

        //public float RemainingDistance 
        //{
        //    get
        //    {
        //        var direction = Destination - Entity.Position;
        //        direction.Y = 0;
        //        return direction.Length();
        //    }
        //}

        public override void Update(float deltaT)
        {
            base.Update(deltaT);
            if (currentPath != null)
            {
                
                var dest = currentPath.Points.Last();
                var maxDistance = Speed * deltaT;
                Entity.Position = TundraVector3.MoveTowards(Entity.Position, dest, maxDistance);
            }

        }
        
        public void Reset()
        {
        }
    }
}