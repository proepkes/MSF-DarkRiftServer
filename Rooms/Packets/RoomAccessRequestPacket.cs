using DarkRift;

namespace Rooms.Packets
{
    public class RoomAccessRequestPacket : IDarkRiftSerializable
    {
        public int RoomId;
        public string Password = "";

        public void Deserialize(DeserializeEvent e)
        {
            RoomId = e.Reader.ReadInt32();
            Password = e.Reader.ReadString();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(RoomId);
            e.Writer.Write(Password);
        }
    }
}