using DarkRift;

namespace SpawnerHandler.Packets
{
    public class SpawnStatusPacket : IDarkRiftSerializable
    {
        public int SpawnId;
        public SpawnStatus Status;

        public void Deserialize(DeserializeEvent e)
        {
            SpawnId = e.Reader.ReadInt32();
            Status = (SpawnStatus)e.Reader.ReadInt32();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(SpawnId);
            e.Writer.Write((int)Status);
        }
    }
}