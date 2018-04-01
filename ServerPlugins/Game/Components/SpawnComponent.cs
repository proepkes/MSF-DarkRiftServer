using System.Collections.Generic;
using System.Linq;
using DarkRift;
using Utils;
using Utils.Packets;

namespace ServerPlugins.Game.Components
{
    public class SpawnComponent : Component
    {
        public override void Start()
        {
            //Warp agent to initial position
            Entity.GetComponent<NavigationComponent>().Warp(Entity.Position);
        }

        private readonly HashSet<uint> _currentObservers = new HashSet<uint>();
        public override void Update(float delta)
        {
            //Send Spawn-notification when an observer was added
            foreach (var observer in Entity.Observers.Where(player => !_currentObservers.Contains(player.ID)))
            {
                //TODO: Include pending navigation if Entity.NavMeshComponent hasPath
                observer.Client.SendMessage(Message.Create(MessageTags.SpawnEntity,
                    new EntityPacket
                    {
                        NetworkEntity = Entity,
                        //if the observer is the player himself, he has authority, 
                        //check this field on client-side to either spawn player-prefab or networkview-prefab
                        HasAuthority = observer.ID == Entity.ID
                    }), SendMode.Reliable);
                
                _currentObservers.Add(observer.ID);

            }
        }
    }
}