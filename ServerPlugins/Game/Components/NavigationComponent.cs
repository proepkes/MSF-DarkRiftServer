using DarkRift;
using RecastDetour;
using RecastDetour.Detour;
using Utils;
using Utils.Game;
using Utils.Packets;

namespace ServerPlugins.Game.Components
{
    public class NavigationComponent : Component
    {
        private readonly NavMeshAgent _agent = new NavMeshAgent();
        TundraVector3 lastDestination; 

        public NavMeshQuery NavMeshQuery
        {
            get => _agent.NavMeshQuery;
            set => _agent.NavMeshQuery = value;
        }

        public float Speed
        {
            get => _agent.Speed;
            set => _agent.Speed = value;
        }

        public bool HasPath => _agent.HasPath;

        public void Warp(TundraVector3 position)
        {
            _agent.Position = position;
        }

        public void SetDestination(TundraVector3 destination)
        {
            _agent.SetDestination(destination);
        }

        public override void Update(float deltaT)
        {
            if (_agent.Destination != lastDestination)
            {
                var packet = new AckNavigateToPacket
                {
                    EntityID = Entity.ID,
                    Path = _agent.CurrentPath,
                    Speed = _agent.Speed,
                    StoppingDistance = _agent.StoppingDistance
                };

                foreach (var observer in Entity.Observers)
                {
                    observer.Client.SendMessage(Message.Create(MessageTags.NavigateTo, packet), SendMode.Unreliable);
                }

                lastDestination = _agent.Destination;

                Entity.Game.Log("Published: " + packet, LogType.Info);
            }

            if (_agent.CurrentPath != null)
            {
                _agent.Integrate(deltaT);
            }
        }
        
        public void Reset()
        {
        }
    }
}