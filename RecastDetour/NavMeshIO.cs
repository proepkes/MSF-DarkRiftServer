using System.IO;
using Newtonsoft.Json;
using dtStatus = System.UInt32;

namespace Pathfinding.Serialization
{
    public static class NavMeshSerializer 
    {
        public static void Serialize(string path, Detour.dtNavMeshCreateParams mesh)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(mesh));
        }

        public static Detour.dtNavMeshCreateParams Deserialize(string path)
        {
            return JsonConvert.DeserializeObject<Detour.dtNavMeshCreateParams>(File.ReadAllText(path));
        }

        public static Detour.dtNavMeshQuery CreateMeshQuery(Detour.dtNavMeshCreateParams meshCreateParams)
        {
            Detour.dtRawTileData navData;
            if (!Detour.dtCreateNavMeshData(meshCreateParams, out navData))
            {
                return null;
            }

            var m_navMesh = new Detour.dtNavMesh();
            dtStatus status;

            status = m_navMesh.init(navData, (int)Detour.dtTileFlags.DT_TILE_FREE_DATA);
            if (Detour.dtStatusFailed(status))
            {
                return null;
            }

            var m_navQuery = new Detour.dtNavMeshQuery();
            status = m_navQuery.init(m_navMesh, 2048);
            if (Detour.dtStatusFailed(status))
            {
                return null;
            }

            return m_navQuery;
        }
    }
}