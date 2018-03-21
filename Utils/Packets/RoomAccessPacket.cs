using DarkRift;

namespace Utils.Packets
{
    public class RoomAccessPacket : IDarkRiftSerializable
    {
        public int RoomId;
        public string RoomIp;
        public int RoomPort;
        public string SceneName = "";
        public string Token;

        public void Deserialize(DeserializeEvent e)
        {
            Token = e.Reader.ReadString();
            RoomIp = e.Reader.ReadString();
            RoomPort = e.Reader.ReadInt32();
            RoomId = e.Reader.ReadInt32();
            SceneName = e.Reader.ReadString();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(Token);
            e.Writer.Write(RoomIp);
            e.Writer.Write(RoomPort);
            e.Writer.Write(RoomId);
            e.Writer.Write(SceneName);
        }

        public override string ToString()
        {
            return $"[RoomAccessPacket| PublicAddress: {RoomIp + ":" + RoomPort}, RoomId: {RoomId}, Token: {Token}]";
        }
    }
}