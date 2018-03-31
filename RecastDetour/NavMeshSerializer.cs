using System.IO;
using Newtonsoft.Json;
using dtStatus = System.UInt32;

namespace RecastDetour
{
    public static class NavMeshSerializer 
    {
        public static void Serialize(string path, Detour.Detour.dtNavMeshCreateParams mesh)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(mesh));
        }

        public static Detour.Detour.dtNavMeshCreateParams Deserialize(string path)
        {
            return JsonConvert.DeserializeObject<Detour.Detour.dtNavMeshCreateParams>(File.ReadAllText(path));
        }

        public static Detour.Detour.dtNavMeshQuery CreateMeshQuery(Detour.Detour.dtNavMeshCreateParams meshCreateParams)
        {
            Detour.Detour.dtRawTileData navData;
            if (!Detour.Detour.dtCreateNavMeshData(meshCreateParams, out navData))
            {
                return null;
            }

            var m_navMesh = new Detour.Detour.dtNavMesh();
            dtStatus status;

            status = m_navMesh.init(navData, (int)Detour.Detour.dtTileFlags.DT_TILE_FREE_DATA);
            if (Detour.Detour.dtStatusFailed(status))
            {
                return null;
            }

            var m_navQuery = new Detour.Detour.dtNavMeshQuery();
            status = m_navQuery.init(m_navMesh, 2048);
            if (Detour.Detour.dtStatusFailed(status))
            {
                return null;
            }

            return m_navQuery;
        }
    }
}