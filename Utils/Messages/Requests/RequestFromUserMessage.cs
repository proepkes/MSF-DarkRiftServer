using System.Text;
using DarkRift;

namespace Utils.Messages.Requests
{
    public class RequestFromUserMessage : RequestMessage
    {
        public string EMail;

        public override void Deserialize(DeserializeEvent e)
        {
            base.Deserialize(e);
            EMail = e.Reader.ReadString(Encoding.Unicode);
        }

        public override void Serialize(SerializeEvent e)
        {
            base.Serialize(e);
            e.Writer.Write(EMail, Encoding.Unicode);
        }
    }
}