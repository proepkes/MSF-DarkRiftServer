using DarkRift;
using Utils.Game;

namespace Utils.Packets
{
    public class PositionPacket : IDarkRiftSerializable
    {
        public TundraNetPosition Position { get; set; }

        public void Deserialize(DeserializeEvent e)
        {
            throw new System.NotImplementedException();
        }

        public void Serialize(SerializeEvent e)
        {
            throw new System.NotImplementedException();
        }
    }
}