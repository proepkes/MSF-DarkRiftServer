
using DarkRift;

namespace SpawnerLib.Packets
{
    public class KillSpawnedProcessPacket : IDarkRiftSerializable
    {
        public int SpawnerId;
        public int SpawnId;
        
        public void Deserialize(DeserializeEvent e)
        {
            SpawnerId = e.Reader.ReadInt32();
            SpawnId = e.Reader.ReadInt32();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(SpawnerId);
            e.Writer.Write(SpawnId);
        }
    }
}