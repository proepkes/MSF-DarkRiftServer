using System.Linq;
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

        public float Speed = 3f;
        public float StoppingDistance = 0;
        
        public NavMeshQuery NavMeshQuery;

        public bool HasPath
        {
            get { return currentPath != null; }
        }

        private int currentPathIndex;
        private SmoothPath currentPath;
        private TundraNetPosition currentDestination;

        public void Navigate(TundraNetPosition to)
        {
            currentPath = Pathfinder.ComputeSmoothPath(NavMeshQuery, Entity.Position, to);
            if (currentPath.Points.Count > 0)
            {
                currentPathIndex = 0;
                currentDestination = currentPath.Points[0];
                Entity.Game.Log("Destination: " + currentPath.Last(), LogType.Info);
                Entity.Position = currentPath.Last();
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

        public override void Update(float deltaT)
        {
            base.Update(deltaT);
            //if (currentPath != null)
            //{
            //    var dest = currentPath.Points.Last();
            //    var maxDistance = Speed * deltaT;

            //    Entity.Game.Log("Before: " + Entity.Position + " (MD: " + maxDistance + ")", LogType.Info);

            //    Entity.Position.X = dest.X + maxDistance;
            //    Entity.Position.Y = dest.Y + maxDistance;
            //    Entity.Position.Z = dest.Z + maxDistance;

            //    Entity.Game.Log("After: " + Entity.Position, LogType.Info);
            //}
            
        }
        
        public void Reset()
        {
        }
    }
}