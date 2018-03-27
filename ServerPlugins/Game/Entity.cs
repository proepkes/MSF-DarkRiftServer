using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using DarkRift;
using DarkRift.Server;
using ServerPlugins.Game.Components;
using Utils;
using Utils.Game;
using Utils.Packets;

namespace ServerPlugins.Game
{
    public class Entity
    {
        private EntityState tmpState;
        public readonly List<Player> Observers = new List<Player>();
        private readonly List<Component> _components = new List<Component>();

        public uint ID;

        public GamePlugin Game;

        public Entity Target;

        public EntityState State = EntityState.Idle;

        public int Health = 100;

        public TundraNetPosition Position { get; set; } = TundraNetPosition.Create(0f, 0f, 0f);

        public T AddComponent<T>() where T: Component, new()
        {
            var component = new T { Entity = this };
            _components.Add(component);
            return component;
        }

        public T GetComponent<T>() where T : Component
        {
            return (T)_components.FirstOrDefault(component => component is T);
        }

        public virtual void Start()
        {
            foreach (var component in _components)
            {
                component.Start();
            }
        }
        public virtual void Update()
        {
            tmpState = State;
            foreach (var component in _components)
            {
                component.Update();
            }
        }

        public void LateUpdate()
        {
            //Send update with state has changed in Update
            if (tmpState != State)
            {
                foreach (var observer in Observers)
                {
                    observer.Client.SendMessage(
                        Message.Create(MessageTags.ChangeState, new ChangStatePacket {EntityID = ID, State = State}),
                        SendMode.Reliable);
                }
            }
        }

        public virtual void Destroy()
        {
            foreach (var component in _components)
            {
                component.Destroy();
            }

        }
    }
}