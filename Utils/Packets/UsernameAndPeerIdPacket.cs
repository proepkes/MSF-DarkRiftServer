using DarkRift;

namespace Utils.Packets
{
    public class UsernameAndPeerIdPacket : IDarkRiftSerializable
    {
        public int ClientID;
        public string Username = "";

        public void Deserialize(DeserializeEvent e)
        {
            Username = e.Reader.ReadString();
            ClientID = e.Reader.ReadInt32();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(Username);
            e.Writer.Write(ClientID);
        }

        public override string ToString()
        {
            return string.Format("[Username: {0}, Peer ID: {1}]", Username, ClientID);
        }
    }
}