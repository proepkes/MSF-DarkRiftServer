using System.Text;
using DarkRift;

namespace Utils.Packets
{
    public class StringPacket : IDarkRiftSerializable
    {
        public string Data;

        public void Deserialize(DeserializeEvent e)
        {
            Data = e.Reader.ReadString(Encoding.Unicode);
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(Data, Encoding.Unicode);
        }
    }
}