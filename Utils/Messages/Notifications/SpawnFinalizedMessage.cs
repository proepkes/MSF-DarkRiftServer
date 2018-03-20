using DarkRift;

namespace SpawnerLib.Packets
{
    public class SpawnFinalizedMessage : IDarkRiftSerializable
    {
        public int SpawnTaskID;
        public int RoomID;

        public void Deserialize(DeserializeEvent e)
        {
            SpawnTaskID = e.Reader.ReadInt32();
            RoomID = e.Reader.ReadInt32();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(SpawnTaskID);
            e.Writer.Write(RoomID);
        }
    }
}