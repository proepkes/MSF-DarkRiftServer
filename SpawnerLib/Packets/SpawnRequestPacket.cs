using DarkRift;

namespace SpawnerLib.Packets
{
    public class SpawnRequestPacket : IDarkRiftSerializable
    {
        public int SpawnerId;
        public int SpawnTaskID;
        public string SpawnCode = "";
        public string SceneName = "";

        public void Deserialize(DeserializeEvent e)
        {
            SpawnerId = e.Reader.ReadInt32();
            SpawnTaskID = e.Reader.ReadInt32();
            SpawnCode = e.Reader.ReadString();
            SceneName = e.Reader.ReadString();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(SpawnerId);
            e.Writer.Write(SpawnTaskID);
            e.Writer.Write(SpawnCode);
            e.Writer.Write(SceneName);
        }
    }
}