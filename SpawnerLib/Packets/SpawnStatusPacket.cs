using DarkRift;
using Utils;

namespace SpawnerLib.Packets
{
    public class SpawnStatusPacket : IDarkRiftSerializable
    {
        public int SpawnTaskID;
        public SpawnStatus Status;

        public void Deserialize(DeserializeEvent e)
        {
            SpawnTaskID = e.Reader.ReadInt32();
            Status = (SpawnStatus)e.Reader.ReadInt32();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(SpawnTaskID);
            e.Writer.Write((int)Status);
        }
    }
}