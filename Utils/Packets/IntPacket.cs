using DarkRift;

namespace Utils.Packets
{
    public class IntPacket : IDarkRiftSerializable
    {
        public int Data;

        public void Deserialize(DeserializeEvent e)
        {
            Data = e.Reader.ReadInt32();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(Data);
        }
    }
}