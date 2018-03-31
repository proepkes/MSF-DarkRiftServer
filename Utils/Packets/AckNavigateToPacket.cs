using DarkRift;
using Utils.Game;

namespace Utils.Packets
{
    public class AckNavigateToPacket : IDarkRiftSerializable
    {
        public uint EntityID;
        public SmoothPath Path;
        public float StoppingDistance;

        public void Deserialize(DeserializeEvent e)
        {
            EntityID = e.Reader.ReadUInt32();
            Path = e.Reader.ReadSerializable<SmoothPath>();
            StoppingDistance = e.Reader.ReadSingle();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(EntityID);
            e.Writer.Write(Path);
            e.Writer.Write(StoppingDistance);
        }
    }
}