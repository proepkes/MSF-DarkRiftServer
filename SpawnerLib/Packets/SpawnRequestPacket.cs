using DarkRift;

namespace SpawnerLib.Packets
{
    public class SpawnRequestPacket : IDarkRiftSerializable
    {
        public string SpawnCode;
        public int SpawnerId;
        public int SpawnTaskID;
        public int MaxPlayers;
        public string WorldName;
        public string RoomName;
        public bool IsPublic;

        public void Deserialize(DeserializeEvent e)
        {
            SpawnerId = e.Reader.ReadInt32();
            SpawnTaskID = e.Reader.ReadInt32();
            MaxPlayers = e.Reader.ReadInt32();
            SpawnCode = e.Reader.ReadString();
            WorldName = e.Reader.ReadString();
            RoomName = e.Reader.ReadString();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(SpawnerId);
            e.Writer.Write(SpawnTaskID);
            e.Writer.Write(MaxPlayers);
            e.Writer.Write(SpawnCode);
            e.Writer.Write(WorldName);
            e.Writer.Write(RoomName);
        }
    }
}