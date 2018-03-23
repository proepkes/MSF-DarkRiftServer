using DarkRift;

namespace Utils.Packets
{
    public class SpawnRequestPacket : IDarkRiftSerializable
    {
        public string SpawnCode;
        public int SpawnerId;
        public int SpawnTaskID;
        public RoomOptions Options;

        public void Deserialize(DeserializeEvent e)
        {
            SpawnerId = e.Reader.ReadInt32();
            SpawnTaskID = e.Reader.ReadInt32();
            SpawnCode = e.Reader.ReadString();
            Options = e.Reader.ReadSerializable<RoomOptions>();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(SpawnerId);
            e.Writer.Write(SpawnTaskID);
            e.Writer.Write(SpawnCode);
            e.Writer.Write(Options);
        }
    }
}