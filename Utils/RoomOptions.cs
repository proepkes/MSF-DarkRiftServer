using DarkRift;

namespace Utils
{
    /// <summary>
    ///     List of options, which are sent to master server during registration
    /// </summary>
    public class RoomOptions : IDarkRiftSerializable
    {
        public bool IsPublic;

        public int MaxPlayers;

        public string RoomIp = "";

        public int RoomPort = -1;

        public string WorldName;
        public string RoomName;


        public void Deserialize(DeserializeEvent e)
        {
            WorldName = e.Reader.ReadString();
            RoomName = e.Reader.ReadString();
            RoomIp = e.Reader.ReadString();
            RoomPort = e.Reader.ReadInt32();
            IsPublic = e.Reader.ReadBoolean();
            MaxPlayers = e.Reader.ReadInt32();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(WorldName);
            e.Writer.Write(RoomName);
            e.Writer.Write(RoomIp);
            e.Writer.Write(RoomPort);
            e.Writer.Write(IsPublic);
            e.Writer.Write(MaxPlayers);
        }
    }
}