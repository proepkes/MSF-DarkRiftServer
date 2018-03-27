using DarkRift;
using ServerPlugins.SharpNav.Crowds;
using ServerPlugins.SharpNav.Geometry;
using Utils;
using Utils.Game;
using Utils.Packets;

namespace ServerPlugins.Game.Components
{
    public class NavigationComponent : Component
    {
        private int agentID;
        private Agent agent;

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
            var agentParams = new AgentParams
            {
                Radius = .5f,
                Height = 1f,
                MaxAcceleration = 8,
                MaxSpeed = 3f,
                CollisionQueryRange = 4f,
                PathOptimizationRange = 15f,
                SeparationWeight = 3f,
                UpdateFlags = UpdateFlags.Separation | UpdateFlags.OptimizeTopo
            };

            agentID = Entity.Game.Crowd.AddAgent(Entity.Position, agentParams);
            if (agentID >= 0)
            {
                agent = Entity.Game.Crowd.GetAgent(agentID);
            }
            else
            {
                Entity.Game.Log("Failed to create a Nav-Agent for Entity " + Entity, LogType.Info);
            }
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

        public Vector3 Velocity => agent.Vel;

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