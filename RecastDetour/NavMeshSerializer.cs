using System.IO;
using Newtonsoft.Json;
using RecastDetour.Detour;
using dtStatus = System.UInt32;

namespace RecastDetour
{
    public static class NavMeshSerializer 
    {
        public static void Serialize(string path, NavMeshCreateParams mesh)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(mesh));
        }

        public static NavMeshCreateParams Deserialize(string path)
        {
            return JsonConvert.DeserializeObject<NavMeshCreateParams>(File.ReadAllText(path));
        }

        public static NavMeshQuery CreateMeshQuery(NavMeshCreateParams meshCreateParams)
        {
            Detour.Detour.dtRawTileData navData;
            if (!Detour.Detour.dtCreateNavMeshData(meshCreateParams, out navData))
            {
                return null;
            }

            var m_navMesh = new NavMesh();
            dtStatus status;

            status = m_navMesh.init(navData, (int)Detour.Detour.dtTileFlags.DT_TILE_FREE_DATA);
            if (Detour.Detour.dtStatusFailed(status))
            {
                return null;
            }

            var m_navQuery = new NavMeshQuery();
            status = m_navQuery.init(m_navMesh, 2048);
            if (Detour.Detour.dtStatusFailed(status))
            {
                return null;
            }

            return m_navQuery;
        }
    }
}