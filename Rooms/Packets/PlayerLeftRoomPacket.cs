using DarkRift;

namespace Rooms.Packets
{
    public class PlayerLeftRoomPacket : IDarkRiftSerializable
    {
        public int PeerId;
        public int RoomId;

        public void Deserialize(DeserializeEvent e)
        {
            PeerId = e.Reader.ReadInt32();
            RoomId = e.Reader.ReadInt32();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(PeerId);
            e.Writer.Write(RoomId);
        }
    }
}