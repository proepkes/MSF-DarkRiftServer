using RecastDetour.Detour;
using ServerPlugins.Game;
using Utils;
using Utils.Game;

namespace RecastDetour
{
    public class NavMeshAgent
    {
        public bool HasPath => CurrentPath != null;
        public SmoothPath CurrentPath { get; private set; }
        public TundraVector3 Destination { get; private set; }

        public TundraVector3 Position { get; set; }
        public NavMeshQuery NavMeshQuery { get; set; }
        public float Speed { get; set; }
        public float StoppingDistance { get; set; }

        private int _currentPathIndex;
        private TundraVector3 _currentDestination;


        public void SetDestination(TundraVector3 destination)
        {
            CurrentPath = Pathfinder.ComputeSmoothPath(NavMeshQuery, Position, destination);
            if (CurrentPath.Points.Count > 0)
            {
                _currentPathIndex = 0;
                _currentDestination = CurrentPath.Points[0];
                Destination = CurrentPath.Points[CurrentPath.PointsCount-1];
            }
            else
            {
                CurrentPath = null;
            }
        }

        public void Integrate(float deltaTime)
        {
            var maxDistance = Speed * deltaTime;
            if (TundraVector3.Distance(Position, _currentDestination) <= StoppingDistance)
            {
                if (_currentPathIndex < CurrentPath.PointsCount - 1)
                {
                    _currentDestination = CurrentPath.Points[++_currentPathIndex];
                    Position = TundraVector3.MoveTowards(Position, _currentDestination, maxDistance);
                }
                else
                {
                    CurrentPath = null;
                }
            }
            else
            {
                Position = TundraVector3.MoveTowards(Position, _currentDestination, maxDistance);
            }
        }
    }
}