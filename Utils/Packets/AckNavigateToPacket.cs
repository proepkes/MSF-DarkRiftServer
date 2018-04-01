using DarkRift;
using Utils.Game;

namespace Utils.Packets
{
    public class AckNavigateToPacket : IDarkRiftSerializable
    {
        public uint EntityID;
        //TODO: Calculate same navmesh on all clients & use a shared Agent for pathfinding => only send Destination instead of complete path
        public SmoothPath Path;
        public float Speed;
        public float StoppingDistance;

        public void Deserialize(DeserializeEvent e)
        {
            EntityID = e.Reader.ReadUInt32();
            Path = e.Reader.ReadSerializable<SmoothPath>();
            Speed = e.Reader.ReadSingle();
            StoppingDistance = e.Reader.ReadSingle();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(EntityID);
            e.Writer.Write(Path);
            e.Writer.Write(Speed);
            e.Writer.Write(StoppingDistance);
        }

        public override string ToString()
        {
            return $"Entity: {EntityID} - {Path.PointsCount} Points with Speed {Speed} (StoppingDistance: {StoppingDistance})";
        }
    }
}