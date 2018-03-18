using System.Collections.Generic;
using System.Linq;
using DarkRift;
using DarkRift.Server;
using SpawnerHandler.Packets;
using Utils;

namespace SpawnerHandler
{
    public class RegisteredSpawner
    {
        public delegate void KillRequestCallback(bool isKilled);

        public static int MaxConcurrentRequests = 8;

        public int ID { get; set; }
        public IClient Client { get; set; }
        public SpawnerOptions Options { get; set; }

        private readonly Queue<SpawnTask> _queue;
        private readonly HashSet<SpawnTask> _startingProcesses;

        public int ProcessesRunning { get; private set; }

        private HashSet<SpawnTask> _beingSpawned;

        public RegisteredSpawner(int id, IClient client, SpawnerOptions options)
        {
            ID = id;
            Client = client;
            Options = options;

            _queue = new Queue<SpawnTask>();
            _beingSpawned = new HashSet<SpawnTask>();
        }

        public int CalculateFreeSlotsCount()
        {
            return Options.MaxProcesses - _queue.Count - ProcessesRunning;
        }

        public bool CanSpawnAnotherProcess()
        {
            // Unlimited
            if (Options.MaxProcesses == 0)
                return true;

            // Spawner is busy
            if (_queue.Count + ProcessesRunning >= Options.MaxProcesses)
                return false;

            return true;
        }

        public void AddTaskToQueue(SpawnTask task)
        {
            _queue.Enqueue(task);
        }

        public void UpdateQueue()
        {
            // Ignore if there's no connection with the peer
            if (!Client.IsConnected)
                return;

            // Ignore if nothing's in the queue
            if (_queue.Count == 0)
                return;

            if (_beingSpawned.Count >= MaxConcurrentRequests)
            {
                // If we're currently at the maximum available concurrent spawn count
                var finishedSpawns = _beingSpawned.Where(s => s.IsDoneStartingProcess);

                // Remove finished spawns
                foreach (var finishedSpawn in finishedSpawns)
                    _beingSpawned.Remove(finishedSpawn);
            }

            // If we're still at the maximum concurrent requests
            if (_beingSpawned.Count >= MaxConcurrentRequests)
                return;

            var task = _queue.Dequeue();

            var data = new SpawnRequestPacket
            {
                SpawnerId = ID,
                SpawnId = task.SpawnId,
                SpawnCode = task.UniqueCode
            };

            Client.SendMessage(Message.Create(MessageTags.RequestSpawnFromMasterToSpawner, data), SendMode.Reliable);
        }

        public void SendKillRequest(int spawnId, KillRequestCallback callback)
        {
            var packet = new KillSpawnedProcessPacket()
            {
                SpawnerId = ID,
                SpawnId = spawnId
            };

            Client.SendMessage(Message.Create(MessageTags.KillSpawn, packet), SendMode.Reliable); 
        }

        public void UpdateProcessesCount(int newCount)
        {
            ProcessesRunning = newCount;
        }

        public void OnProcessKilled()
        {
            ProcessesRunning -= 1;
        }

        public void OnProcessStarted()
        {
            ProcessesRunning += 1;
        }
    }
}