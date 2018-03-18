using DarkRift;

namespace Rooms.Packets
{
    public class RoomAccessProvideCheckPacket : IDarkRiftSerializable
    {
        public int PeerId;
        public int RoomId;
        public string Username = "";

        public void Deserialize(DeserializeEvent e)
        {
            PeerId = e.Reader.ReadInt32();
            RoomId = e.Reader.ReadInt32();
            Username = e.Reader.ReadString();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(PeerId);
            e.Writer.Write(RoomId);
            e.Writer.Write(Username);
        }
    }
}