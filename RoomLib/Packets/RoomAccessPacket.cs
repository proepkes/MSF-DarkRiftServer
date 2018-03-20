using DarkRift;

namespace RoomLib.Packets
{
    public class RoomAccessPacket : IDarkRiftSerializable
    {
        public string RoomIp;
        public int RoomPort;
        public string Token;
        public int RoomId;
        public string SceneName = "";

        public override string ToString()
        {
            return string.Format("[RoomAccessPacket| PublicAddress: {0}, RoomId: {1}, Token: {2}]",
                RoomIp + ":" + RoomPort, RoomId, Token);

        }

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
    }
}