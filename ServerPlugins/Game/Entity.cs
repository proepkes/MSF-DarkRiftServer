using System.Collections.Generic;
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

        public readonly IClient Client;

        public readonly List<Entity> Observers = new List<Entity>();

        private readonly List<Component> _components = new List<Component>();

        public TundraNetPosition Position { get; set; } = TundraNetPosition.Create(0f, 0f, 0f);

        public Entity(IClient client)
        {
            Client = client;
        }

        public T AddComponent<T>() where T: Component, new()
        {
            var component = new T { Entity = this };
            _components.Add(component);
            return component;
        }

        public void Start()
        {
            foreach (var component in _components)
            {
                component.Start();
            }
        }
        public void Update()
        {
            foreach (var component in _components)
            {
                component.Update();
            }
        }
    }
}