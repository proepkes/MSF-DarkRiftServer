using DarkRift;
using System.Text;

namespace Utils.Messages.Requests
{
    public class RequestResetPasswordMessage : RequestMessage
    {
        public string EMail;
        public string Code;
        public string NewPassword;

        public override void Deserialize(DeserializeEvent e)
        {
            base.Deserialize(e);
            EMail = e.Reader.ReadString(Encoding.Unicode);
            Code = e.Reader.ReadString(Encoding.Unicode);
            NewPassword = e.Reader.ReadString(Encoding.Unicode);
        }

        public override void Serialize(SerializeEvent e)
        {
            base.Serialize(e);
            e.Writer.Write(EMail, Encoding.Unicode);
            e.Writer.Write(Code, Encoding.Unicode);
            e.Writer.Write(NewPassword, Encoding.Unicode);
        }
    }
}