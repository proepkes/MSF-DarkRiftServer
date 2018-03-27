using DarkRift;

namespace Utils.Packets
{
    public class BytePacket : IDarkRiftSerializable
    {
        public byte Data;
        public void Deserialize(DeserializeEvent e)
        {
            Data = e.Reader.ReadByte();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(Data);
        }
    }
}