using DarkRift;

namespace SpawnerLib.Packets
{
    public class SpawnFinalizationPacket : IDarkRiftSerializable
    {
        public int SpawnId;

        public void Deserialize(DeserializeEvent e)
        {
            SpawnId = e.Reader.ReadInt32();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(SpawnId);
        }
    }
}