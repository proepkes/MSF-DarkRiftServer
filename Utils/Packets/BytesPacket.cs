using DarkRift;

namespace Utils.Packets
{
    public class BytesPacket : IDarkRiftSerializable
    {
        public byte[] Data;
        public void Deserialize(DeserializeEvent e)
        {
            Data = e.Reader.ReadBytes();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(Data);
        }
    }
}