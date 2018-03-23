using System;
using System.Collections.Generic;
using DarkRift.Server;
using Utils;
using Utils.Messages.Notifications;

namespace ServerPlugins.SpawnerHandler
{
    /// <summary>
    ///     Represents a spawn request, and manages the state of request
    ///     from start to finalization
    /// </summary>
    public class SpawnTask
    {
        private SpawnStatus _status;

        protected List<Action<SpawnTask>> WhenDoneCallbacks;

        public RegisteredSpawner Spawner { get; }
        public RoomOptions Options { get; }

        public int ID { get; }

        public string UniqueCode { get; }

        public SpawnFinalizedMessage FinalizationMessage { get; private set; }

        public bool IsAborted => _status < SpawnStatus.None;

        public bool IsDoneStartingProcess => IsAborted || IsProcessStarted;

        public bool IsProcessStarted => Status >= SpawnStatus.WaitingForProcess;

        public SpawnStatus Status
        {
            get => _status;
            set
            {
                _status = value;

                if (StatusChanged != null)
                    StatusChanged.Invoke(_status);

                if (_status >= SpawnStatus.Finalized || _status < SpawnStatus.None)
                    NotifyDoneListeners();
            }
        }

        /// <summary>
        ///     Peer, who registered a started process for this task
        ///     (for example, a game server)
        /// </summary>
        public IClient RegisteredClient { get; private set; }

        /// <summary>
        ///     Who requested to spawn
        ///     (most likely clients peer)
        ///     Can be null
        /// </summary>
        public IClient Requester { get; set; }

        public SpawnTask(int id, RegisteredSpawner spawner, RoomOptions options)
        {
            ID = id;

            Spawner = spawner;
            Options = options;

            UniqueCode = Security.CreateRandomString(6);
            WhenDoneCallbacks = new List<Action<SpawnTask>>();
        }

        public event Action<SpawnStatus> StatusChanged;

        public void OnProcessStarted()
        {
            if (!IsAborted && Status < SpawnStatus.WaitingForProcess)
            {
                Status = SpawnStatus.WaitingForProcess;
                Spawner.OnProcessStarted();
            }
        }

        public void OnProcessKilled()
        {
            Status = SpawnStatus.Killed;
            Spawner.OnProcessKilled();
        }

        public void OnRegistered(IClient clientWhoRegistered)
        {
            RegisteredClient = clientWhoRegistered;

            if (!IsAborted && Status < SpawnStatus.ProcessRegistered) Status = SpawnStatus.ProcessRegistered;
        }

        public void OnFinalized(SpawnFinalizedMessage finalizedMessage)
        {
            FinalizationMessage = finalizedMessage;
            if (!IsAborted && Status < SpawnStatus.Finalized) Status = SpawnStatus.Finalized;
        }

        public override string ToString()
        {
            return $"[SpawnTask: id - {ID}]";
        }

        protected void NotifyDoneListeners()
        {
            foreach (var callback in WhenDoneCallbacks) callback.Invoke(this);

            WhenDoneCallbacks.Clear();
        }

        /// <summary>
        ///     Callback will be called when spawn task is aborted or completed
        ///     (game server is opened)
        /// </summary>
        /// <param name="callback"></param>
        public SpawnTask WhenDone(Action<SpawnTask> callback)
        {
            WhenDoneCallbacks.Add(callback);
            return this;
        }

        public void Abort()
        {
            if (Status >= SpawnStatus.Finalized)
                return;

            Status = SpawnStatus.Aborting;

            KillSpawnedProcess();
        }

        public void KillSpawnedProcess()
        {
            Spawner.SendKillRequest(ID, killed =>
            {
                Status = SpawnStatus.Aborted;

                if (!killed)
                    throw new Exception("Spawned Process might not have been killed");
            });
        }
    }
}