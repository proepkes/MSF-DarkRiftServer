using System;
using System.Collections.Generic;
using DarkRift;
using DarkRift.Server;
using ServerPlugins.Game.Entities;
using Utils.Game;

namespace ServerPlugins.Game.Components
{
    public abstract class Component
    {
        public Entity Entity;

        public virtual void Start() { }
        public virtual void Update() { }
        public virtual void Destroy() { }
    }
}