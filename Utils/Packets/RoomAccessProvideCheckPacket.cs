using DarkRift;

namespace Utils.Packets
{
    public class RoomAccessProvideCheckPacket : IDarkRiftSerializable
    {
        public int ClientID;
        public int RoomID;

        public void Deserialize(DeserializeEvent e)
        {
            ClientID = e.Reader.ReadInt32();
            RoomID = e.Reader.ReadInt32();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(ClientID);
            e.Writer.Write(RoomID);
        }
    }
}