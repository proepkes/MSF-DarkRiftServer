using DarkRift;
using System.Text;

namespace Utils.Messages.Requests
{
    public class RequestWithStringMessage : RequestMessage
    {
        public string String;

        public override void Deserialize(DeserializeEvent e)
        {
            base.Deserialize(e);
            String = e.Reader.ReadString(Encoding.Unicode);
        }

        public override void Serialize(SerializeEvent e)
        {
            base.Serialize(e);
            e.Writer.Write(String, Encoding.Unicode);
        }
    }
}