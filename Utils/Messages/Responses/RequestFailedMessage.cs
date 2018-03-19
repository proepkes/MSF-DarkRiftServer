using DarkRift;
using System.Text;

namespace Utils.Messages.Response
{
    public class RequestFailedMessage : ResponseMessage
    {
        public string Reason;

        public override void Deserialize(DeserializeEvent e)
        {
            base.Deserialize(e);
            Reason = e.Reader.ReadString(Encoding.Unicode);
        }

        public override void Serialize(SerializeEvent e)
        {
            base.Serialize(e);
            e.Writer.Write(Reason);
        }
    }
}