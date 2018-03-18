using DarkRift;

namespace Rooms.Packets
{
    public class SaveRoomOptionsPacket : IDarkRiftSerializable
    {
        public int RoomId;
        public RoomOptions Options;

        public void Deserialize(DeserializeEvent e)
        {
            RoomId = e.Reader.ReadInt32();
            Options = e.Reader.ReadSerializable<RoomOptions>();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(RoomId);
            e.Writer.Write(Options);
        }
    }
}