using Utils.Game;

namespace ServerPlugins.Game.Components
{
    public class NavigationComponent : Component
    {
        public float StoppingDistance = 0;

        public TundraNetPosition Destination = TundraNetPosition.Create(0f, 0f, 0f);

        public bool IsDirty { get; set; }

        public void Navigate()
        {

        }

        public override void Update()
        {
            
        }

        public void Reset()
        {

        }
    }
}