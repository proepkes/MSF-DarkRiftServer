using DarkRift;
using Utils.Game;

namespace Utils.Packets
{
    public class EntityPacket : IDarkRiftSerializable
    {
        public NetworkEntity NetworkEntity;
        public bool HasAuthority;

        public void Deserialize(DeserializeEvent e)
        {
            NetworkEntity = e.Reader.ReadSerializable<NetworkEntity>();
            HasAuthority = e.Reader.ReadBoolean();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(NetworkEntity);
            e.Writer.Write(HasAuthority);
        }
    }
}