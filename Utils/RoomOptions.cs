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
        public string WorldName;
        public string RoomName;
        public string Region;


        public void Deserialize(DeserializeEvent e)
        {
            WorldName = e.Reader.ReadString();
            RoomName = e.Reader.ReadString();
            IsPublic = e.Reader.ReadBoolean();
            MaxPlayers = e.Reader.ReadInt32();
            Region = e.Reader.ReadString();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(WorldName);
            e.Writer.Write(RoomName);
            e.Writer.Write(IsPublic);
            e.Writer.Write(MaxPlayers);
            e.Writer.Write(Region);
        }
    }
}