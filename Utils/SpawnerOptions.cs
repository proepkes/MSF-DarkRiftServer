using DarkRift;

namespace Utils
{
    public class SpawnerOptions : IDarkRiftSerializable
    {
        public string MachineIp = "xxx.xxx.xxx.xxx";

        public int MaxProcesses;

        public string Region = "International";

        public void Deserialize(DeserializeEvent e)
        {
            MachineIp = e.Reader.ReadString();
            MaxProcesses = e.Reader.ReadInt32();
            Region = e.Reader.ReadString();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(MachineIp);
            e.Writer.Write(MaxProcesses);
            e.Writer.Write(Region);
        }

        public override string ToString()
        {
            return $"PublicIp: {MachineIp}, MaxProcesses: {MaxProcesses}, Region: {Region}";
        }
    }
}