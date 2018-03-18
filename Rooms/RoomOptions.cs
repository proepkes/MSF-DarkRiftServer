using DarkRift;

namespace Rooms
{
    /// <summary>
    /// List of options, which are sent to master server during registration
    /// </summary>
    public class RoomOptions : IDarkRiftSerializable
    {
        /// <summary>
        /// Name of the room
        /// </summary>
        public string Name = "Unnamed";

        /// <summary>
        /// IP of the machine on which the room was created
        /// (Only used in the <see cref="RoomController.DefaultAccessProvider"/>)
        /// </summary>
        public string RoomIp = "";

        /// <summary>
        /// Port, required to access the room 
        /// (Only used in the <see cref="RoomController.DefaultAccessProvider"/>)
        /// </summary>
        public int RoomPort = -1;

        /// <summary>
        /// If true, room will appear in public listings
        /// </summary>
        public bool IsPublic;

        /// <summary>
        /// If 0 - player number is not limited
        /// </summary>
        public int MaxPlayers = 0;

        /// <summary>
        /// Room password
        /// </summary>
        public string Password = "";

        /// <summary>
        /// Number of seconds, after which unconfirmed (pending) accesses will removed
        /// to allow new players. Make sure it's long enought to allow player to load gameplay scene
        /// </summary>
        public float AccessTimeoutPeriod = 10;

        /// <summary>
        /// If set to false, users will no longer be able to request access directly.
        /// This is useful when you want players to get accesses through other means, for example
        /// through Lobby module,
        /// </summary>
        public bool AllowUsersRequestAccess = true;

        public void Deserialize(DeserializeEvent e)
        {
            Name = e.Reader.ReadString();
            RoomIp = e.Reader.ReadString();
            RoomPort = e.Reader.ReadInt32();
            IsPublic = e.Reader.ReadBoolean();
            MaxPlayers = e.Reader.ReadInt32();
            Password = e.Reader.ReadString();
            AccessTimeoutPeriod = e.Reader.ReadSingle();
            AllowUsersRequestAccess = e.Reader.ReadBoolean();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(Name);
            e.Writer.Write(RoomIp);
            e.Writer.Write(RoomPort);
            e.Writer.Write(IsPublic);
            e.Writer.Write(MaxPlayers);
            e.Writer.Write(Password);
            e.Writer.Write(AccessTimeoutPeriod);
            e.Writer.Write(AllowUsersRequestAccess);
        }
    }
}