using DarkRift.Server;
using Utils.Game;

namespace ServerPlugins.Game.Components
{
    public abstract class Component
    {
        public Entity Entity;

        public virtual void Start() { }
        public virtual void Update() { }
    }
}