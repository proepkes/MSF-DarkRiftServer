using DarkRift;

namespace SpawnerLib
{
    public class SpawnerOptions : IDarkRiftSerializable
    {
        /// <summary>
        /// Public IP address of the machine, on which the spawner is running
        /// </summary>
        public string MachineIp = "xxx.xxx.xxx.xxx";

        /// <summary>
        /// Max number of processes that this spawner can handle. If 0 - unlimited
        /// </summary>
        public int MaxProcesses = 0;

        /// <summary>
        /// Region, to which the spawner belongs
        /// </summary>
        public string Region = "International";

        public override string ToString()
        {

            return string.Format("PublicIp: {0}, MaxProcesses: {1}, Region: {2}",
                MachineIp, MaxProcesses, Region);
        }

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
    }
}