using DarkRift;

namespace SpawnerHandler.Packets
{
    public class SpawnedProcessStartedPacket : IDarkRiftSerializable
    {
        public int SpawnId;
        public int ProcessId;
        public string CmdArgs;

        public void Deserialize(DeserializeEvent e)
        {
            SpawnId = e.Reader.ReadInt32();
            ProcessId = e.Reader.ReadInt32();
            CmdArgs = e.Reader.ReadString();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(SpawnId);
            e.Writer.Write(ProcessId);
            e.Writer.Write(CmdArgs);
        }
    }
}