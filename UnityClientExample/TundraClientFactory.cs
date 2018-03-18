using UnityClientExample;

namespace Tundra
{
    public static class TundraClientFactory
    {
        public static ITundraClient Create(IDarkRiftUnityClient darkRiftUnityClient)
        {
            return new TundraClient(darkRiftUnityClient);
        }
    }
}