using DarkRift;

namespace Utils.Messages.Requests
{
    public class RegisterSpawnedProcessMessage : IDarkRiftSerializable
    {
        public string SpawnCode;
        public int SpawnTaskID;

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