using System.Text;
using DarkRift;

namespace Utils.Messages.Requests
{
    public class ResetPasswordMessage : RequestMessage
    {
        public string Code;
        public string EMail;
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