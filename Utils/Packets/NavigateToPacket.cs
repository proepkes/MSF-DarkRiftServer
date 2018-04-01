using DarkRift;
using Utils.Game;

namespace Utils.Packets
{
    public class NavigateToPacket : IDarkRiftSerializable
    {
        public TundraVector3 Destination;
        public float StoppingDistance;

        public void Deserialize(DeserializeEvent e)
        {
            Destination = e.Reader.ReadSerializable<TundraVector3>();
            StoppingDistance = e.Reader.ReadSingle();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(Destination);
            e.Writer.Write(StoppingDistance);
        }
    }
}