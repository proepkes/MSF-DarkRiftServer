using DarkRift;

namespace Utils
{
    public class SmoothPath : IDarkRiftSerializable
    {
        public const int MAX_POLYS = 256;
        public const int MAX_SMOOTH = 2048;

        public int PointsCount = 0;
        public readonly float[] Points = new float[MAX_SMOOTH * 3];
        public void Deserialize(DeserializeEvent e)
        {
            PointsCount = e.Reader.ReadInt32();
            for (int i = 0; i < PointsCount*3; ++i)
            {
                Points[i] = e.Reader.ReadSingle();
            }
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(PointsCount);
            for (int i = 0; i < PointsCount * 3; ++i)
            {
                e.Writer.Write(Points[i]);
            }
        }
    }
}