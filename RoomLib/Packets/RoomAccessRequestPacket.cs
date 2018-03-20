using DarkRift;

namespace RoomLib.Packets
{
    public class RoomAccessRequestPacket : IDarkRiftSerializable
    {
        public string Password = "";
        public int RoomId;

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