using System.Collections;
using System.Collections.Generic;
using DarkRift;
using Utils.Game;

namespace Utils
{
    public class SmoothPath : IDarkRiftSerializable, IEnumerable<TundraNetPosition>
    {
        public const int MAX_POLYS = 256;
        public const int MAX_SMOOTH = 2048;

        public int PointsCount = 0;
        public readonly List<TundraNetPosition> Points = new List<TundraNetPosition>();
        public void Deserialize(DeserializeEvent e)
        {
            PointsCount = e.Reader.ReadInt32();
            for (int i = 0; i < PointsCount; ++i)
            {
                Points.Add(e.Reader.ReadSerializable<TundraNetPosition>());
            }
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(Points.Count);
            foreach (var point in Points)
            {
                e.Writer.Write(point);
            }
        }

        public IEnumerator<TundraNetPosition> GetEnumerator()
        {
            return Points.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}