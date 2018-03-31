using DarkRift;

namespace Pathfinding.Serialization
{
    public class SmoothPath : IDarkRiftSerializable
    {
        public const int MAX_POLYS = 256;
        public const int MAX_SMOOTH = 2048;

        public int m_nsmoothPath = 0;
        public float[] m_smoothPath = new float[MAX_SMOOTH * 3];
        public void Deserialize(DeserializeEvent e)
        {
            m_nsmoothPath = e.Reader.ReadInt32();
            m_smoothPath = e.Reader.ReadSingles();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(m_nsmoothPath);
            e.Writer.Write(m_smoothPath);
        }
    }
}