using DarkRift;

namespace SpawnerHandler.Packets
{
    public class RegisterSpawnedProcessPacket : IDarkRiftSerializable
    {
        public int SpawnId;
        public string SpawnCode;

        public void Deserialize(DeserializeEvent e)
        {
            SpawnId = e.Reader.ReadInt32();
            SpawnCode = e.Reader.ReadString();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(SpawnId);
            e.Writer.Write(SpawnCode);
        }
    }
}