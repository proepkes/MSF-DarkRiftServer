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
        public uint ID;

        public EntityState State = EntityState.Idle;

        public GamePlugin Game;

        public readonly List<Player> Observers = new List<Player>();

        private readonly List<Component> _components = new List<Component>();

        public TundraNetPosition Position { get; set; } = TundraNetPosition.Create(0f, 0f, 0f);

        public Entity Target;

        public int Health = 100;

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
            foreach (var component in _components)
            {
                component.Update();
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