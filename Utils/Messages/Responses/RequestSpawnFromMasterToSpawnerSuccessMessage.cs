using System.Text;
using DarkRift;

namespace Utils.Messages.Response
{
    public class RequestSpawnFromMasterToSpawnerSuccessMessage : ResponseMessage
    {
        public int SpawnID;
        public int ProcessID;
        public string Arguments;

        public override void Serialize(SerializeEvent e)
        {
            base.Serialize(e);
            e.Writer.Write(SpawnID);
            e.Writer.Write(ProcessID);
            e.Writer.Write(Arguments, Encoding.Unicode);
        }

        public override void Deserialize(DeserializeEvent e)
        {
            base.Deserialize(e);
            SpawnID = e.Reader.ReadInt32();
            ProcessID = e.Reader.ReadInt32();
            Arguments = e.Reader.ReadString(Encoding.Unicode);
        }
    }
}