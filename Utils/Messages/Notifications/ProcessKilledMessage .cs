using DarkRift;

namespace Utils.Messages.Notifications
{
    public class ProcessKilledMessage : NotificationMessage
    {
        public int SpawnerID;
        public int SpawnTaskID;

        public override void Deserialize(DeserializeEvent e)
        {
            base.Deserialize(e);
            SpawnerID = e.Reader.ReadInt32();
            SpawnTaskID = e.Reader.ReadInt32();
        }

        public override void Serialize(SerializeEvent e)
        {
            base.Serialize(e);
            e.Writer.Write(SpawnerID);
            e.Writer.Write(SpawnTaskID);
        }
    }
}