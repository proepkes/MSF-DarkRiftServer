using DarkRift;

namespace RoomLib.Packets
{
    public class RoomAccessValidatePacket : IDarkRiftSerializable
    {
        public int RoomId;
        public string Token;


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