using DarkRift;
using Utils.Game;

namespace Utils.Packets
{
    public class SpawnEntityPacket : IDarkRiftSerializable
    {
        public uint ID;
        public TundraNetPosition Position;

        public void Deserialize(DeserializeEvent e)
        {
            ID = e.Reader.ReadUInt16();
            Position = e.Reader.ReadSerializable<TundraNetPosition>();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(ID);
            e.Writer.Write(Position);
        }
    }
}