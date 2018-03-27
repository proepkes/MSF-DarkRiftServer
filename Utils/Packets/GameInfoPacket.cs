using System.Collections.Generic;
using DarkRift;

namespace Utils.Packets
{
    public class GameInfoPacket : IDarkRiftSerializable
    {
        public int Count;
        public List<GameInfo> Games;

        public void Deserialize(DeserializeEvent e)
        {
            Count = e.Reader.ReadInt32();
            Games = new List<GameInfo>();
            for (var i = 0; i < Count; i++)
            {
                Games.Add(e.Reader.ReadSerializable<GameInfo>());
            }
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(Games.Count);
            foreach (var game in Games)
            {
                e.Writer.Write(game);
            }
        }
    }

    public class GameInfo : IDarkRiftSerializable
    {
        public int ID;
        public string Name = "";
        public int MaxPlayers;
        public int OnlinePlayers;
        
        public override string ToString()
        {
            return string.Format("[GameInfo: id: {0}, address: {1}, players: {2}/{3}, type: {4}]",
                ID, OnlinePlayers, MaxPlayers);
        }

        public void Deserialize(DeserializeEvent e)
        {
            ID = e.Reader.ReadInt32();
            Name = e.Reader.ReadString();
            MaxPlayers = e.Reader.ReadInt32();
            OnlinePlayers = e.Reader.ReadInt32();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(ID);
            e.Writer.Write(Name);
            e.Writer.Write(MaxPlayers);
            e.Writer.Write(OnlinePlayers);
        }
    }
}