using DarkRift;

namespace Utils.Messages.Responses
{
    public class RequestSpawnFromMasterToSpawnerFailedMessage : RequestFailedMessage
    {
        public int SpawnTaskID;

        public override void Serialize(SerializeEvent e)
        {
            base.Serialize(e);
            e.Writer.Write(SpawnTaskID);
        }

        public override void Deserialize(DeserializeEvent e)
        {
            base.Deserialize(e);
            SpawnTaskID = e.Reader.ReadInt32();
        }
    }
}