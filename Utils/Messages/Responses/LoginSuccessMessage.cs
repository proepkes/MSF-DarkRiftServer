using DarkRift;

namespace Utils.Messages.Responses
{
    public class LoginSuccessMessage : ResponseMessage
    {
        public bool IsAdmin;
        public bool IsGuest;

        public override void Deserialize(DeserializeEvent e)
        {
            base.Deserialize(e);
            IsAdmin = e.Reader.ReadBoolean();
            IsGuest = e.Reader.ReadBoolean();
        }

        public override void Serialize(SerializeEvent e)
        {
            base.Serialize(e);
            e.Writer.Write(IsAdmin);
            e.Writer.Write(IsGuest);
        }
    }
}