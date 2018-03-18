using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DarkRift;
using DarkRift.Server;
using SpawnerHandler.Packets;
using Utils;
using Utils.Messages;
using Utils.Messages.Response;

namespace SpawnerHandler
{

    /// <summary>
    /// This plugin contains the references to all spawners, creates spawn-tasks and delegates them to an available spawner
    /// </summary>
    public class SpawnerHandlerPlugin : Plugin
    {
        public delegate void SpawnedProcessRegistrationHandler(SpawnTask task, IClient peer);

        private int _spawnerId = 0;
        private int _spawnTaskId = 0;

        public override Version Version => new Version(1, 0, 0);
        public override bool ThreadSafe => true;

        public event Action<RegisteredSpawner> SpawnerRegistered;
        public event Action<RegisteredSpawner> SpawnerDestroyed;
        public event SpawnedProcessRegistrationHandler SpawnedProcessRegistered;

        //ClientID -> SpawnTask
        private readonly Dictionary<int, SpawnTask> _pendingSpawnTasks;

        //TaskID -> RegisteredSpawner
        private readonly Dictionary<int, RegisteredSpawner> _spawners;

        //TaskID -> SpawnTask
        private readonly Dictionary<int, SpawnTask> _spawnTasks;

        public bool EnableClientSpawnRequests { get; set; }
        public int QueueUpdateFrequency { get; set; }

        public SpawnerHandlerPlugin(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            _spawnTasks = new Dictionary<int, SpawnTask>();
            _spawners = new Dictionary<int, RegisteredSpawner>();
            _pendingSpawnTasks = new Dictionary<int, SpawnTask>();

            EnableClientSpawnRequests = Convert.ToBoolean(pluginLoadData.Settings.Get(nameof(EnableClientSpawnRequests)));
            QueueUpdateFrequency = Convert.ToInt32(pluginLoadData.Settings.Get(nameof(QueueUpdateFrequency)));

            ClientManager.ClientConnected += OnClientConnected;
            ClientManager.ClientDisconnected += OnClientDisconnected;

            Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(QueueUpdateFrequency);

                    foreach (var spawner in _spawners.Values)
                    {
                        try
                        {
                            spawner.UpdateQueue();
                        }
                        catch (Exception e)
                        {
                            Dispatcher.InvokeWait(() =>
                            {
                                WriteEvent("Failed to update spawnerqueue", LogType.Error, e);
                            });
                        }
                    }
                }
            });
        }

        private void OnClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            if (_spawners.ContainsKey(e.Client.ID))
            {
                WriteEvent("Spawner " + _spawners[e.Client.ID] + " disconnected.", LogType.Info);

                // Remove the spawner from all spawners
                _spawners.Remove(e.Client.ID);
            }

            //_pendingSpawnTasks is only to check for multiple spawnrequests only, so it is safe to remove the client from the collection
            if (_pendingSpawnTasks.ContainsKey(e.Client.ID))
            {
                _pendingSpawnTasks.Remove(e.Client.ID);
            }
        }

        private void OnClientConnected(object sender, ClientConnectedEventArgs e)
        {
            e.Client.MessageReceived += OnClientMessageReceived;
        }

        private void OnClientMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            using (var message = e.GetMessage())
            {
                switch (message.Tag)
                {
                    case MessageTags.RegisterSpawner:
                        HandleRegisterSpawner(e.Client, message);
                        break;
                    case MessageTags.RequestSpawnFromClientToMaster:
                        HandleClientsSpawnRequest(e.Client, message);
                        break;
                }
            }
        }

        private void HandleClientsSpawnRequest(IClient client, Message message)
        {
            var data = message.Deserialize<RequestSpawnFromClientToMasterMessage>();
            if (data != null)
            {
                if (!CanClientSpawn(client, data))
                {
                    // Client can't spawn
                    client.SendMessage(Message.Create(MessageTags.RequestSpawnFromClientToMasterFailed,
                            new RequestFailedMessage
                            {
                                Status = ResponseStatus.Unauthorized,
                                Reason = "Unauthorized"
                            }),
                        SendMode.Reliable);
                    return;
                }

                if (_pendingSpawnTasks.ContainsKey(client.ID) && !_pendingSpawnTasks[client.ID].IsDoneStartingProcess)
                {
                    // Client has unfinished request
                    client.SendMessage(Message.Create(MessageTags.RequestSpawnFromClientToMasterFailed,
                            new RequestFailedMessage
                            {
                                Status = ResponseStatus.Failed,
                                Reason = "You already have an active request"
                            }),
                        SendMode.Reliable);
                    return;
                }

                // Get the spawn task
                var task = Spawn(data.Region);

                if (task == null)
                {   
                    client.SendMessage(Message.Create(MessageTags.RequestSpawnFromClientToMasterFailed,
                            new RequestFailedMessage
                            {
                                Status = ResponseStatus.Failed,
                                Reason = "All the servers are busy.Try again later"
                            }),
                        SendMode.Reliable);
                    return;
                }

                task.Requester = client;

                // Save the task
                _pendingSpawnTasks[client.ID] = task;

                // Listen to status changes
                task.StatusChanged += (status) =>
                {
                    if (client.IsConnected)
                    {
                        // Send status update
                        client.SendMessage(Message.Create(MessageTags.SpawnStatusChanged, new SpawnStatusPacket
                        {
                            SpawnId = task.SpawnId,
                            Status = status
                        }), SendMode.Reliable);
                    }
                };

                client.SendMessage(Message.Create(MessageTags.RequestSpawnFromClientToMasterSuccess, new RequestClientSpawnSuccess {TaskID = task.SpawnId, Status = ResponseStatus.Success}), SendMode.Reliable);
            }
        }

        private void HandleRegisterSpawner(IClient client, Message message)
        {
            if (!HasCreationPermissions(client))
            {
                client.SendMessage(Message.Create(MessageTags.RegisterSpawnerFailed, new RequestFailedMessage
                {
                    Status = ResponseStatus.Unauthorized,
                    Reason = "Insufficient permissions"

                }), SendMode.Reliable);
                return;
            }

            var options = message.Deserialize<SpawnerOptions>();
            if (options != null)
            {
                var spawner = CreateSpawner(client, options);

                // Respond with spawner id
                client.SendMessage(Message.Create(MessageTags.RegisterSpawnerSuccess,
                        new RegisterSpawnerSuccessMessage
                        {
                            Status = ResponseStatus.Success,
                            SpawnerID = spawner.ID
                        }),
                    SendMode.Reliable);
            }
        }

        private RegisteredSpawner CreateSpawner(IClient client, SpawnerOptions options)
        {
            var spawner = new RegisteredSpawner(GenerateSpawnerId(), client, options);
            
            // Add the spawner to a list of all spawners
            _spawners[client.ID] = spawner;

            // Invoke the event
            if (SpawnerRegistered != null)
                SpawnerRegistered.Invoke(spawner);

            return spawner;
        }

        public virtual SpawnTask Spawn(string region = "")
        {
            var spawners = GetFilteredSpawners(region);

            if (spawners.Count < 0)
            {
                WriteEvent("No spawner was returned after filtering. " +
                            (string.IsNullOrEmpty(region) ? "" : "Region: " + region), LogType.Warning);
                return null;
            }

            // Order from least busy server
            var orderedSpawners = spawners.OrderByDescending(s => s.CalculateFreeSlotsCount());
            var availableSpawner = orderedSpawners.FirstOrDefault(s => s.CanSpawnAnotherProcess());

            // Ignore, if all of the spawners are busy
            if (availableSpawner == null)
                return null;

            return Spawn(availableSpawner);
        }

        public virtual SpawnTask Spawn(RegisteredSpawner spawner)
        {
            var task = new SpawnTask(GenerateSpawnTaskId(), spawner);

            _spawnTasks[task.SpawnId] = task;

            spawner.AddTaskToQueue(task);

            WriteEvent("Spawner was found, and spawn task created: " + task, LogType.Trace);

            return task;
        }

        public virtual List<RegisteredSpawner> GetFilteredSpawners(string region)
        {
            return GetSpawners(region);
        }
        public virtual List<RegisteredSpawner> GetSpawners()
        {
            return GetSpawners(null);
        }

        public virtual List<RegisteredSpawner> GetSpawners(string region)
        {
            // If region is not provided, retrieve all spawners
            if (string.IsNullOrEmpty(region))
                return _spawners.Values.ToList();

            return GetSpawnersInRegion(region);
        }
        public virtual List<RegisteredSpawner> GetSpawnersInRegion(string region)
        {
            return _spawners.Values
                .Where(s => s.Options.Region == region)
                .ToList();
        }

        private bool HasCreationPermissions(IClient client)
        {
            //TODO: spawner-authentication
            return true;
        }
        public int GenerateSpawnerId()
        {
            return _spawnerId++;
        }
        public int GenerateSpawnTaskId()
        {
            return _spawnTaskId++;
        }

        protected virtual bool CanClientSpawn(IClient client, RequestSpawnFromClientToMasterMessage data)
        {
            return EnableClientSpawnRequests;
        }
    }
}

