using DarkRift;
using Utils.Game;

namespace Utils.Packets
{
    public class AckNavigateToPacket : IDarkRiftSerializable
    {
        public uint EntityID;
        public TundraNetPosition Destination;
        public float StoppingDistance;

        public void Deserialize(DeserializeEvent e)
        {
            EntityID = e.Reader.ReadUInt32();
            Destination = e.Reader.ReadSerializable<TundraNetPosition>();
            StoppingDistance = e.Reader.ReadSingle();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(EntityID);
            e.Writer.Write(Destination);
            e.Writer.Write(StoppingDistance);
        }
    }
}