using DarkRift;

namespace Utils.Messages.Responses
{
    public class RegisterRoomSuccessMessage : ResponseMessage
    {
        public int RoomID;

        public override void Deserialize(DeserializeEvent e)
        {
            base.Deserialize(e);
            RoomID = e.Reader.ReadInt32();
        }

        public override void Serialize(SerializeEvent e)
        {
            base.Serialize(e);
            e.Writer.Write(RoomID);
        }
    }
}