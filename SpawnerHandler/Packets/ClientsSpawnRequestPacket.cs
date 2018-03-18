using System.Collections.Generic;
using DarkRift;
using Utils.IO;

namespace SpawnerHandler.Packets
{
    public class ClientsSpawnRequestPacket : IDarkRiftSerializable
    {
        public string Region = "";

        public void Deserialize(DeserializeEvent e)
        {
            Region = e.Reader.ReadString();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(Region);
        }
    }
}