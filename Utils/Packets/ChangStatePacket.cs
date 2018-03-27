using DarkRift;
using Utils.Game;

namespace Utils.Packets
{
    public class ChangStatePacket : IDarkRiftSerializable
    {
        public uint EntityID;
        public EntityState State;

        public void Deserialize(DeserializeEvent e)
        {
            EntityID = e.Reader.ReadUInt32();
            State = (EntityState) e.Reader.ReadByte();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(EntityID);
            e.Writer.Write((byte) State);
        }
    }
}