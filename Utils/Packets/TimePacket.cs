using DarkRift;

namespace Utils.Packets
{
    public class TimePacket : IDarkRiftSerializable
    {
        public float Time;

        public void Deserialize(DeserializeEvent e)
        {
            Time = e.Reader.ReadSingle();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(Time);
        }
    }
}