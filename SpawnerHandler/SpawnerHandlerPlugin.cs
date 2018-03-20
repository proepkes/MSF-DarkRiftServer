using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DarkRift;
using DarkRift.Server;
using SpawnerLib;
using SpawnerLib.Packets;
using Utils;
using Utils.Messages.Notifications;
using Utils.Messages.Requests;
using Utils.Messages.Responses;

namespace SpawnerHandler
{
    /// <summary>
    ///     This plugin runs on the master and contains the references to all spawners.
    ///     It also creates spawn-tasks and delegates them to an available spawner.
    /// </summary>
    public class SpawnerHandlerPlugin : Plugin
    {
        //ClientID -> SpawnTask (only contains ClientSpawnRequests)
        private readonly Dictionary<int, SpawnTask> _pendingSpawnTasks;

        private readonly List<RegisteredSpawner> _registeredSpawners;

        private readonly List<SpawnTask> _spawnTasks;
        private int _nextSpawnerId;
        private int _nextSpawnTaskId;

        public override Version Version => new Version(1, 0, 0);
        public override bool ThreadSafe => true;

        public bool EnableClientSpawnRequests { get; set; }
        public int QueueUpdateFrequency { get; set; }

        public SpawnerHandlerPlugin(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            _spawnTasks = new List<SpawnTask>();
            _registeredSpawners = new List<RegisteredSpawner>();
            _pendingSpawnTasks = new Dictionary<int, SpawnTask>();

            QueueUpdateFrequency = Convert.ToInt32(pluginLoadData.Settings.Get(nameof(QueueUpdateFrequency)));
            EnableClientSpawnRequests =
                Convert.ToBoolean(pluginLoadData.Settings.Get(nameof(EnableClientSpawnRequests)));

            ClientManager.ClientConnected += OnClientConnected;
            ClientManager.ClientDisconnected += OnClientDisconnected;

            Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(QueueUpdateFrequency);

                    foreach (var spawner in _registeredSpawners)
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
            });
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
                    case MessageTags.RegisterSpawnedProcess:
                        HandleRegisterSpawnedProcess(e.Client, message);
                        break;
                    case MessageTags.RequestSpawnFromClientToMaster:
                        HandleClientsSpawnRequest(e.Client, message);
                        break;
                    case MessageTags.RequestSpawnFromMasterToSpawnerSuccess:
                        //Spawner started a new process
                        HandleRequestSpawnFromMasterToSpawnerSuccess(e.Client, message);
                        break;
                    case MessageTags.RequestSpawnFromMasterToSpawnerFailed:
                        //Cancel SpawnTask
                        HandleRequestSpawnFromMasterToSpawnerFailed(e.Client, message);
                        break;
                    case MessageTags.NotifySpawnerKilledProcess:
                        HandleNotifySpawnerKilledProcess(e.Client, message);
                        break;
                    case MessageTags.CompleteSpawnProcess:
                        HandleCompleteSpawnProcess(e.Client, message);
                        break;
                }
            }
        }

        private void HandleCompleteSpawnProcess(IClient client, Message message)
        {
            var data = message.Deserialize<SpawnFinalizedMessage>();
            if (data != null)
            {
                var task = _spawnTasks.FirstOrDefault(spawnTask => spawnTask.ID == data.SpawnTaskID);

                if (task == null)
                {
                    WriteEvent("Process tried to complete to an unknown task", LogType.Error);
                    client.SendMessage(
                        Message.Create(MessageTags.CompleteSpawnProcessFailed,
                            new FailedMessage {Status = ResponseStatus.Error, Reason = "Unknown task"}),
                        SendMode.Reliable);
                    return;
                }

                if (task.RegisteredClient.ID != client.ID)
                {
                    WriteEvent("Spawned process tried to complete spawn task, but it's not the same peer who registered to the task", LogType.Error);
                    client.SendMessage(
                        Message.Create(MessageTags.CompleteSpawnProcessFailed,
                            new FailedMessage { Status = ResponseStatus.Unauthorized, Reason = "Client mismatch." }),
                        SendMode.Reliable);
                    return;
                }

                task.OnFinalized(data);

                client.SendMessage(Message.CreateEmpty(MessageTags.CompleteSpawnProcessSuccess), SendMode.Reliable);
            }
        }

        private void HandleNotifySpawnerKilledProcess(IClient client, Message message)
        {
            var data = message.Deserialize<ProcessKilledMessage>();
            if (data != null)
            {
                var task = _spawnTasks.FirstOrDefault(spawnTask => spawnTask.ID == data.SpawnTaskID);

                if (task == null)
                    return;

                task.OnProcessKilled();
            }
        }

        private void HandleRegisterSpawnedProcess(IClient client, Message message)
        {
            var data = message.Deserialize<RegisterSpawnedProcessMessage>();
            if (data != null)
            {
                var task = _spawnTasks.FirstOrDefault(spawnTask => spawnTask.ID == data.SpawnTaskID);
                if (task == null)
                {
                    client.SendMessage(
                        Message.Create(MessageTags.RegisterSpawnedProcessFailed,
                            new FailedMessage {Reason = "Invalid spawn task", Status = ResponseStatus.Failed}),
                        SendMode.Reliable);
                    WriteEvent(
                        "Process tried to register to an unknown task. Client: " + client.RemoteTcpEndPoint.Address,
                        LogType.Warning);
                    return;
                }

                if (task.UniqueCode != data.SpawnCode)
                {
                    client.SendMessage(
                        Message.Create(MessageTags.RegisterSpawnedProcessFailed,
                            new FailedMessage {Reason = "Unauthorized", Status = ResponseStatus.Unauthorized}),
                        SendMode.Reliable);
                    WriteEvent(
                        "Spawned process tried to register, but failed due to mismaching unique code. Client: " +
                        client.RemoteTcpEndPoint.Address, LogType.Warning);
                    return;
                }

                task.OnRegistered(client);

                client.SendMessage(Message.CreateEmpty(MessageTags.RegisterSpawnedProcessSuccess), SendMode.Reliable);
            }
        }

        private void HandleRequestSpawnFromMasterToSpawnerFailed(IClient client, Message message)
        {
            var data = message.Deserialize<SpawnFromMasterToSpawnerFailedMessage>();
            if (data != null)
            {
                var task = _spawnTasks.FirstOrDefault(spawnTask => spawnTask.ID == data.SpawnTaskID);
                if (task != null)
                    task.Abort();
                WriteEvent("Spawn request was not handled. Status: " + data.Status + " | " + data.Reason,
                    LogType.Warning);
            }
        }

        private void OnClientConnected(object sender, ClientConnectedEventArgs e)
        {
            e.Client.MessageReceived += OnClientMessageReceived;
        }


        private void OnClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            var spawner =
                _registeredSpawners.FirstOrDefault(registeredSpawner => registeredSpawner.Client.ID == e.Client.ID);
            if (spawner != null)
            {
                WriteEvent("Spawner " + spawner + " disconnected.", LogType.Info);

                _spawnTasks.RemoveAll(task => task.Spawner.ID == spawner.ID);

                // Remove the spawner from all spawners
                _registeredSpawners.Remove(spawner);
            }
            else
            {
                //spawn-tasks can only be requested by player-clients
                if (_pendingSpawnTasks.ContainsKey(e.Client.ID)) _pendingSpawnTasks.Remove(e.Client.ID);
            }
        }

        private void HandleRequestSpawnFromMasterToSpawnerSuccess(IClient client, Message message)
        {
            var data = message.Deserialize<SpawnFromMasterToSpawnerSuccessMessage>();
            if (data != null)
            {
                var task = _spawnTasks.FirstOrDefault(spawnTask => spawnTask.ID == data.SpawnTaskID);
                if (task != null) task.OnProcessStarted();
            }
        }

        private void HandleClientsSpawnRequest(IClient client, Message message)
        {
            var data = message.Deserialize<SpawnFromClientToMasterMessage>();
            if (data != null)
            {
                if (!CanClientSpawn(client, data))
                {
                    // Client can't spawn
                    client.SendMessage(Message.Create(MessageTags.RequestSpawnFromClientToMasterFailed,
                            new FailedMessage
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
                            new FailedMessage
                            {
                                Status = ResponseStatus.Failed,
                                Reason = "You already have an active request"
                            }),
                        SendMode.Reliable);
                    return;
                }

                // Get the spawn task
                var task = Spawn(data.Region, data.WorldName, data.RoomName, data.MaxPlayers, data.IsPublic);

                if (task == null)
                {
                    client.SendMessage(Message.Create(MessageTags.RequestSpawnFromClientToMasterFailed,
                            new FailedMessage
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
                task.StatusChanged += status =>
                {
                    if (client.IsConnected)
                        client.SendMessage(Message.Create(MessageTags.SpawnStatusChanged, new SpawnStatusPacket
                        {
                            SpawnTaskID = task.ID,
                            Status = status
                        }), SendMode.Reliable);
                };

                client.SendMessage(
                    Message.Create(MessageTags.RequestSpawnFromClientToMasterSuccess,
                        new ClientSpawnSuccessMessage { TaskID = task.ID, Status = ResponseStatus.Success}),
                    SendMode.Reliable);
            }
        }

        private void HandleRegisterSpawner(IClient client, Message message)
        {
            if (!HasCreationPermissions(client))
            {
                client.SendMessage(Message.Create(MessageTags.RegisterSpawnerFailed, new FailedMessage
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

                WriteEvent("Master registered a new spawner: " + spawner, LogType.Info);
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
            _registeredSpawners.Add(spawner);

            return spawner;
        }

        public virtual SpawnTask Spawn(string region, string world, string room, int maxPlayers, bool isPublic)
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

            var task = new SpawnTask(GenerateSpawnTaskId(), availableSpawner, world, room, maxPlayers, isPublic);

            _spawnTasks.Add(task);

            availableSpawner.AddTaskToQueue(task);

            Dispatcher.InvokeWait(() => WriteEvent("Spawner was found, and spawn task created: " + task, LogType.Trace));

            return task;
        }

        private List<RegisteredSpawner> GetFilteredSpawners(string region)
        {
            return GetSpawners(region);
        }

        private List<RegisteredSpawner> GetSpawners(string region)
        {
            // If region is not provided, retrieve all spawners
            if (string.IsNullOrEmpty(region))
                return _registeredSpawners;

            return GetSpawnersInRegion(region);
        }

        private List<RegisteredSpawner> GetSpawnersInRegion(string region)
        {
            return _registeredSpawners.Where(s => s.Options.Region == region).ToList();
        }

        private bool HasCreationPermissions(IClient client)
        {
            //TODO: spawner-authentication
            return true;
        }

        public int GenerateSpawnerId()
        {
            return _nextSpawnerId++;
        }

        public int GenerateSpawnTaskId()
        {
            return _nextSpawnTaskId++;
        }

        private bool CanClientSpawn(IClient client, SpawnFromClientToMasterMessage data)
        {
            //TODO: Setting: Only allow logged in clients to request a spawn & check here
            return EnableClientSpawnRequests;
        }
    }
}