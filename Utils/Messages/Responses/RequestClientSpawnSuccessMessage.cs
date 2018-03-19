using DarkRift;

namespace Utils.Messages.Response
{
    public class RequestClientSpawnSuccessMessage : ResponseMessage
    {
        public int TaskID;

        public override void Deserialize(DeserializeEvent e)
        {
            base.Deserialize(e);
            TaskID = e.Reader.ReadInt32();
        }

        public override void Serialize(SerializeEvent e)
        {
            base.Serialize(e);
            e.Writer.Write(TaskID);
        }
    }
}