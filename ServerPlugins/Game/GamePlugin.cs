using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DarkRift;
using DarkRift.Server;
using ServerPlugins.Game.Components;
using ServerPlugins.Room;
using Utils;

namespace ServerPlugins.Game
{
    /// <summary>
    ///     This Plugin goes to the spawned server
    /// </summary>
    public class GamePlugin : ServerPluginBase
    {
        private uint _nextEntityID;

        private readonly ConcurrentQueue<Entity> _spawnQueue;
        private readonly ConcurrentQueue<Entity> _despawnQueue;

        public override bool ThreadSafe => false;
        public bool Running { get; set; }
        public int Tickrate { get; set; }

        public event Action Started;

        private long _frameCounter = 0;

        private List<Entity> _entities;

        public GamePlugin(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            Tickrate = Convert.ToInt32(pluginLoadData.Settings.Get(nameof(Tickrate)));

            _spawnQueue = new ConcurrentQueue<Entity>();
            _despawnQueue = new ConcurrentQueue<Entity>();
        }

        protected override void Loaded(LoadedEventArgs args)
        {
            base.Loaded(args);
            _entities = new List<Entity>();
        }

        public void AddEntity(Entity entity)
        {
            entity.ID = _nextEntityID++;
            entity.AddComponent<SpawnComponent>();
            entity.AddComponent<NavigationComponent>();
            _spawnQueue.Enqueue(entity);
        }

        public void RemoveEntity(Entity entity)
        {
            _despawnQueue.Enqueue(entity);
        }
        public void Start()
        {
            Running = true;

            Started?.Invoke();

            int updateTime = (int) (1 / (float)Tickrate * 1000);
            while (Running)
            {
                var time = DateTime.Now.Ticks;

                while (_spawnQueue.Count > 0 && _spawnQueue.TryDequeue(out var entity))
                {
                    //TODO: replace foreach-over-all-entites with foreach-entity-in-AREA-OF-INTERESET
                    lock (_entities)
                    {
                        
                        //let all players observe this unit 
                        foreach (Player player in _entities.Where(e => e is Player))
                        {
                            entity.Observers.Add(player);
                        }

                        _entities.Add(entity);
                        if (entity is Player newPlayer)
                        {
                            //register the player to all units (around him), so they send him notifications when something updates (player observes himself too)
                            foreach (var unit in _entities)
                            {
                                unit.Observers.Add(newPlayer);
                            }
                        }
                    }
                    entity.Start();
                }

                while (_despawnQueue.Count > 0 && _despawnQueue.TryDequeue(out var entity))
                {
                    lock (_entities)
                    {
                        _entities.Remove(entity);
                    }
                    entity.Destroy();
                }

                int deltaT = (int)(DateTime.Now.Ticks - time);

                UpdateGame();

                if (deltaT < updateTime)
                {
                    Thread.Sleep(updateTime - deltaT);
                }

                ++_frameCounter;
            }
        }

        private void UpdateGame()
        {
            if (_frameCounter % Tickrate == 0) //~1 second elapsed
            {
                WriteEvent("Seconds elapsed (approx.): " + _frameCounter / Tickrate, LogType.Info);
            }

            foreach (var entity in _entities)
            {
                entity.Update();
            }
        }

        public void Stop()
        {
            Running = false;
        }

        public void Log(string message, LogType logType)
        {

            WriteEvent(message, logType);
        }
    }
}