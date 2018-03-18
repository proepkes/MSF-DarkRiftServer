using System.Text;
using DarkRift;

namespace Utils.Messages.Requests
{
    public class RequestEmailConfirmationMessage : RequestWithStringMessage
    {
        public string Code;

        public void Deserialize(DeserializeEvent e)
        {
            base.Deserialize(e);
            Code = e.Reader.ReadString(Encoding.Unicode);
        }

        public void Serialize(SerializeEvent e)
        {
            base.Serialize(e);
            e.Writer.Write(Code, Encoding.Unicode);
        }
    }
}