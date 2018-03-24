using DarkRift;

namespace Utils.Packets
{
    public class FloatPacket : IDarkRiftSerializable
    {
        public float Data;

        public void Deserialize(DeserializeEvent e)
        {
            Data = e.Reader.ReadSingle();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(Data);
        }
    }
}