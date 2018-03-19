using DarkRift;

namespace SpawnerLib.Packets
{
    public class RegisterSpawnedProcessPacket : IDarkRiftSerializable
    {
        public int SpawnTaskID;
        public string SpawnCode;

        public void Deserialize(DeserializeEvent e)
        {
            SpawnTaskID = e.Reader.ReadInt32();
            SpawnCode = e.Reader.ReadString();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(SpawnTaskID);
            e.Writer.Write(SpawnCode);
        }
    }
}