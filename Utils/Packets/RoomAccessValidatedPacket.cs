using DarkRift;

namespace Utils.Packets
{
    public class RoomAccessValidatedPacket : IDarkRiftSerializable
    {
        public int MasterClientID;
        public int ClientID;


        public void Deserialize(DeserializeEvent e)
        {
            MasterClientID = e.Reader.ReadInt32();
            ClientID = e.Reader.ReadInt32();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(MasterClientID);
            e.Writer.Write(ClientID);
        }
    }
}