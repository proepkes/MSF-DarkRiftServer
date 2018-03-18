using DarkRift;

namespace Rooms.Packets
{
    public class UsernameAndPeerIdPacket : IDarkRiftSerializable
    {
        public string Username = "";
        public int PeerId;

        public override string ToString()
        {
            return string.Format("[Username: {0}, Peer ID: {1}]", Username, PeerId);
        }

        public void Deserialize(DeserializeEvent e)
        {
            Username = e.Reader.ReadString();
            PeerId = e.Reader.ReadInt32();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(Username);
            e.Writer.Write(PeerId);
        }
    }
}