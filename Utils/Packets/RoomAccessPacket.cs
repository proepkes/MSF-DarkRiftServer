using DarkRift;

namespace Utils.Packets
{
    public class RoomAccessPacket : IDarkRiftSerializable
    {
        public int ClientID;
        public int RoomID;
        public string RoomIp;
        public int RoomPort;
        public string RoomName = "";
        public string Token;

        public void Deserialize(DeserializeEvent e)
        {
            ClientID = e.Reader.ReadInt32();
            Token = e.Reader.ReadString();
            RoomIp = e.Reader.ReadString();
            RoomPort = e.Reader.ReadInt32();
            RoomID = e.Reader.ReadInt32();
            RoomName = e.Reader.ReadString();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(ClientID);
            e.Writer.Write(Token);
            e.Writer.Write(RoomIp);
            e.Writer.Write(RoomPort);
            e.Writer.Write(RoomID);
            e.Writer.Write(RoomName);
        }

        public override string ToString()
        {
            return $"[RoomAccessPacket| PublicAddress: {RoomIp + ":" + RoomPort}, RoomId: {RoomID}, Token: {Token}]";
        }
    }
}