using DarkRift;
using DarkRift.Client;
using DarkRift.Server;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using SpawnerHandler.Packets;
using Utils;
using Utils.Messages.Notifications;
using Utils.Messages.Response;
using MessageReceivedEventArgs = DarkRift.Client.MessageReceivedEventArgs;

namespace Spawner
{
    public class SpawnerPlugin : Plugin
    {
        private static readonly object ProcessLock = new object();
        private static readonly Dictionary<int, Process> Processes = new Dictionary<int, Process>();

        private int _spawnerId;
        private readonly Queue<int> _freePorts;
        private int _lastPortTaken = -1;
        private DarkRiftClient _client;

        public override Version Version => new Version(1, 0, 0);
        public override bool ThreadSafe => true;

        public IPAddress MasterIpAddress { get; set; }
        public int MasterPort { get; set; }

        public string SpawnerIpAddress { get; set; }
        public int SpawnerStartPort { get; set; }
        public int MaxProcesses { get; set; }
        public string ExecutablePath { get; set; }
        public string Region { get; set; }

        public bool AutoStartSpawner { get; set; }


        public SpawnerPlugin(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            _freePorts = new Queue<int>();

            MasterIpAddress = IPAddress.Parse(pluginLoadData.Settings.Get(nameof(MasterIpAddress)));
            MasterPort = Convert.ToInt32(pluginLoadData.Settings.Get(nameof(MasterPort)));

            SpawnerIpAddress = pluginLoadData.Settings.Get(nameof(SpawnerIpAddress));
            SpawnerStartPort = Convert.ToInt32(pluginLoadData.Settings.Get(nameof(SpawnerStartPort)));
            MaxProcesses = Convert.ToInt32(pluginLoadData.Settings.Get(nameof(MaxProcesses)));
            ExecutablePath = pluginLoadData.Settings.Get(nameof(ExecutablePath));
            Region = pluginLoadData.Settings.Get(nameof(Region));

            AutoStartSpawner = Convert.ToBoolean(pluginLoadData.Settings.Get(nameof(AutoStartSpawner)));
        }

        protected override void Loaded(LoadedEventArgs args)
        {
            base.Loaded(args);
            _client = new DarkRiftClient();
            _client.ConnectInBackground(MasterIpAddress, MasterPort, IPVersion.IPv4, OnConnectedToMaster);
        }

        private void OnConnectedToMaster(Exception exception)
        {
            if (exception != null)
            {
                WriteEvent("Connection to master failed", LogType.Fatal, exception);
                return;
            }

            _client.MessageReceived += OnMessageFromMaster;

            if (AutoStartSpawner)
            {
                _client.SendMessage(Message.Create(MessageTags.RegisterSpawner, new SpawnerOptions
                {
                    Region = Region,
                    MachineIp = SpawnerIpAddress,
                    MaxProcesses = MaxProcesses
                }), SendMode.Reliable);
            }
        }

        private void OnMessageFromMaster(object sender, MessageReceivedEventArgs e)
        {
            using (var message = e.GetMessage())
            {
                switch (message.Tag)
                {
                    case MessageTags.RegisterSpawnerSuccess:
                        HandleRegisterSpawnerSuccess(message);
                        break;
                    case MessageTags.RegisterSpawnerFailed:
                        HandleRegisterSpawnerFailed(message);
                        break;
                    case MessageTags.RequestSpawnFromMasterToSpawner:
                        HandleRequestSpawnFromMaster(message);
                        break;
                }
            }
        }

        private void HandleRequestSpawnFromMaster(Message message)
        {
            var data = message.Deserialize<SpawnRequestPacket>();
            if (data != null)
            {
                var port = GetAvailablePort();
                var startProcessInfo = new ProcessStartInfo(ExecutablePath)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    Arguments = //"-batchmode -nographics " +
                                $"{ArgNames.LoadScene} {data.SceneName} " +
                                $"{ArgNames.MasterIp} {MasterIpAddress} " +
                                $"{ArgNames.MasterPort} {MasterPort} " +
                                $"{ArgNames.SpawnId} {data.SpawnId} " +
                                $"{ArgNames.AssignedPort} {port} " +
                                $"{ArgNames.MachineIp} {SpawnerIpAddress} " +
                                $"{ArgNames.SpawnCode} \"{data.SpawnCode}\" "
                };

                WriteEvent("Starting process with args: " + startProcessInfo.Arguments, LogType.Info);

                var processStarted = false;

                try
                {
                    new Thread(() =>
                    {
                        try
                        {
                            using (var process = Process.Start(startProcessInfo))
                            {
                                WriteEvent("Process started. Spawn Id: " + data.SpawnId + ", pid: " + process.Id, LogType.Info);
                                processStarted = true;

                                lock (ProcessLock)
                                {
                                    // Save the process
                                    Processes[data.SpawnId] = process;
                                }

                                var processId = process.Id;

                                // Notify server that we've successfully handled the request
                                Dispatcher.InvokeWait(() =>
                                {
                                    _client.SendMessage(Message.Create(MessageTags.RequestSpawnFromMasterToSpawnerSuccess,
                                            new RequestSpawnFromMasterToSpawnerSuccessMessage { SpawnID = data.SpawnId, ProcessID = processId, Arguments = startProcessInfo.Arguments, Status = ResponseStatus.Success}), SendMode.Reliable);

                                });

                                process.WaitForExit();
                            }
                        }
                        catch (Exception e)
                        {
                            if (!processStarted)
                                Dispatcher.InvokeWait(() =>
                                {
                                    _client.SendMessage(Message.Create(MessageTags.RequestSpawnFromMasterToSpawnerFailed,
                                        new RequestFailedMessage { Reason = e.ToString(), Status = ResponseStatus.Failed }), SendMode.Reliable);

                                    WriteEvent("Failed to start a process at: '" + ExecutablePath + "'", LogType.Fatal, e);
                                });
                        }
                        finally
                        {
                            lock (ProcessLock)
                            {
                                // Remove the process
                                Processes.Remove(data.SpawnId);
                            }
                            Dispatcher.InvokeWait(() =>
                            {
                                // Release the port number
                                _freePorts.Enqueue(port);

                                WriteEvent("Process killed with spawn id: " + data.SpawnId, LogType.Fatal);
                                _client.SendMessage(Message.Create(MessageTags.NotifySpawnerKilledProcess,
                                    new SpawnerKilledProcessNotificationMessage { SpawnID = data.SpawnId, SpawnerID = _spawnerId }), SendMode.Reliable);
                            });
                        }

                    }).Start();
                }
                catch (Exception e)
                {
                    WriteEvent("ThreadException " + data.SpawnId, LogType.Fatal, e);
                }
            }
        }

        private void HandleRegisterSpawnerSuccess(Message message)
        {
            var data = message.Deserialize<RegisterSpawnerSuccessMessage>();
            if (data != null)
            {
                _spawnerId = data.SpawnerID;
                WriteEvent("Spawner " + Region + "-" + _spawnerId + " connected to master: " + MasterIpAddress + ":" + MasterPort, LogType.Info);
            }
        }

        private void HandleRegisterSpawnerFailed(Message message)
        {
            WriteEvent("Failed to register spawner", LogType.Error);
        }

        private int GetAvailablePort()
        {
            // Return a port from a list of available ports
            if (_freePorts.Count > 0)
                return _freePorts.Dequeue();

            if (_lastPortTaken < 0)
                _lastPortTaken = SpawnerStartPort;

            return _lastPortTaken++;
        }
    }
}
