using DarkRift;

namespace Utils.Messages.Responses
{
    public class RegisterSpawnerSuccessMessage : ResponseMessage
    {
        public int SpawnerID;

        public override void Deserialize(DeserializeEvent e)
        {
            base.Deserialize(e);
            SpawnerID = e.Reader.ReadInt32();
        }

        public override void Serialize(SerializeEvent e)
        {
            base.Serialize(e);
            e.Writer.Write(SpawnerID);
        }
    }
}