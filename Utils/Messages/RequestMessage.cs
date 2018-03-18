using DarkRift;

namespace Utils.Messages
{
    public abstract class RequestMessage : IDarkRiftSerializable
    {

        public virtual void Deserialize(DeserializeEvent e)
        {
        }

        public virtual void Serialize(SerializeEvent e)
        {
        }
    }
}