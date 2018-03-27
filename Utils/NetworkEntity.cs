using System.Text;
using DarkRift;
using Utils.Game;

namespace Utils
{
    public class NetworkEntity : IDarkRiftSerializable
    {
        public uint ID;
        public uint TargetID;
        public string Name = "Unknown";

        public EntityState State = EntityState.Idle;

        public float X, Y, Z;

        public int Health = 100;


        public void Deserialize(DeserializeEvent e)
        {
            ID = e.Reader.ReadUInt32();
            TargetID = e.Reader.ReadUInt32();
            Name = e.Reader.ReadString(Encoding.Unicode);
            State = (EntityState)e.Reader.ReadByte();
            X = e.Reader.ReadSingle();
            Y = e.Reader.ReadSingle();
            Z = e.Reader.ReadSingle();
            Health = e.Reader.ReadInt32();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(ID);
            e.Writer.Write(TargetID);
            e.Writer.Write(Name, Encoding.Unicode);
            e.Writer.Write((byte)State);
            e.Writer.Write(X);
            e.Writer.Write(Y);
            e.Writer.Write(Z);
            e.Writer.Write(Health);
        }
    }
}