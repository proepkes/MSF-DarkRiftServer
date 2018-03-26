using DarkRift;

namespace Utils.Game
{
    public class TundraNetPosition : IDarkRiftSerializable
    {
        public static TundraNetPosition Create(float x, float y, float z)
        {
            return new TundraNetPosition { X = x, Y = y, Z = z};
        }

        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public void Deserialize(DeserializeEvent e)
        {
            X = e.Reader.ReadSingle();
            Y = e.Reader.ReadSingle();
            Z = e.Reader.ReadSingle();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(X);
            e.Writer.Write(Y);
            e.Writer.Write(Z);
        }
    }
}