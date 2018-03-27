using DarkRift;
using Utils.Game;

namespace Utils.Packets
{
    public class SpawnEntityPacket : IDarkRiftSerializable
    {
        public uint ID;
        public TundraNetPosition Position;
        public bool HasAuthority;

        public void Deserialize(DeserializeEvent e)
        {
            ID = e.Reader.ReadUInt32();
            Position = e.Reader.ReadSerializable<TundraNetPosition>();
            HasAuthority = e.Reader.ReadBoolean();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(ID);
            e.Writer.Write(Position);
            e.Writer.Write(HasAuthority);
        }
    }
}