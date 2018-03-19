using DarkRift;

namespace Utils.Messages.Requests
{
    public class RequestSpawnFromClientToMasterMessage : IDarkRiftSerializable
    {
        public string Region = "";

        public void Deserialize(DeserializeEvent e)
        {
            Region = e.Reader.ReadString();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(Region);
        }
    }
}