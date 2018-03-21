using System.Text;
using DarkRift;

namespace Utils.Messages.Requests
{
    public class ConfirmEmailMessage : RequestFromUserMessage
    {
        public string Code;

        public override void Deserialize(DeserializeEvent e)
        {
            base.Deserialize(e);
            Code = e.Reader.ReadString(Encoding.Unicode);
        }

        public override void Serialize(SerializeEvent e)
        {
            base.Serialize(e);
            e.Writer.Write(Code, Encoding.Unicode);
        }
    }
}