using DarkRift;

namespace Utils.Messages.Requests
{
    public class SpawnFromClientToMasterMessage : IDarkRiftSerializable
    {
        //The global (real-world) region. This gets parsed by the Master to decide which spawner should spawn the world
        public string Region = ""; 

        //A room has a WorldName-property. One world can have multiple rooms
        public string WorldName = "";

        //A single room within a world. In Unity this is the equivalent to a Scene
        public string RoomName = "";

        public int MaxPlayers = 1;
        public bool IsPublic;

        public void Deserialize(DeserializeEvent e)
        {
            Region = e.Reader.ReadString();
            WorldName = e.Reader.ReadString();
            RoomName = e.Reader.ReadString();
            MaxPlayers = e.Reader.ReadInt32();
            IsPublic = e.Reader.ReadBoolean();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(Region);
            e.Writer.Write(WorldName);
            e.Writer.Write(RoomName);
            e.Writer.Write(MaxPlayers);
            e.Writer.Write(IsPublic);
        }
    }
}