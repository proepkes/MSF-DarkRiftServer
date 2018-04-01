using DarkRift;
using Utils.Game;

namespace Utils.Packets
{
    public class NavigateToPacket : IDarkRiftSerializable
    {
        public TundraVector3 Destination;

        public void Deserialize(DeserializeEvent e)
        {
            Destination = e.Reader.ReadSerializable<TundraVector3>();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(Destination);
        }
    }
}