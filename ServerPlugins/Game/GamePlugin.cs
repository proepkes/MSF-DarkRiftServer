using System;
using System.Collections.Generic;
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

        private readonly Queue<Entity> _spawnQueue;
        private readonly Queue<Entity> _despawnQueue;

        public override bool ThreadSafe => false;
        public bool Running { get; set; }
        public int Tickrate { get; set; }

        public event Action Started;

        private long _frameCounter = 0;

        private List<Entity> _entities;

        public GamePlugin(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            Tickrate = Convert.ToInt32(pluginLoadData.Settings.Get(nameof(Tickrate)));

            _spawnQueue = new Queue<Entity>();
            _despawnQueue = new Queue<Entity>();
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
                while (_spawnQueue.Count > 0)
                {
                    var entity = _spawnQueue.Dequeue();
                    //TODO: replace foreach with area-of-interest
                    foreach (var unit in _entities)
                    {
                        entity.Observers.Add(unit);
                    }
                    _entities.Add(entity);

                    entity.Start();
                }

                while (_despawnQueue.Count > 0)
                {
                    var entity = _despawnQueue.Dequeue();
                    _entities.Remove(entity);
                }

                var time = DateTime.Now.Ticks;
                UpdateGame();

                int deltaT = (int) (DateTime.Now.Ticks - time);
                if (deltaT < updateTime)
                {
                    Thread.Sleep(updateTime - deltaT);
                }

                ++_frameCounter;
            }
        }

        private void UpdateGame()
        {
            if (_frameCounter % Tickrate == 0) //Send position every second
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
    }
}