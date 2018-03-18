using DarkRift;

namespace Rooms.Packets
{
    public class RoomAccessValidatePacket : IDarkRiftSerializable
    {
        public string Token;
        public int RoomId;


        public void Deserialize(DeserializeEvent e)
        {
            Token = e.Reader.ReadString();
            RoomId = e.Reader.ReadInt32();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(Token);
            e.Writer.Write(RoomId);
        }
    }
}