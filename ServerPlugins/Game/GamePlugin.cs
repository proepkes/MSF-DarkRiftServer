using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using DarkRift;
using DarkRift.Server;
using RecastDetour;
using RecastDetour.Detour;
using ServerPlugins.Game.Components;
using ServerPlugins.Game.Entities;
using Utils.Game;

namespace ServerPlugins.Game
{
    /// <summary>
    ///     This Plugin goes to the spawned server
    /// </summary>
    public class GamePlugin : ServerPluginBase
    {
        public readonly int Tickrate;

        //Reserve EntityID 0 for null, (example: no target selected)
        private uint _nextEntityID = 1;

        public readonly Dictionary<uint, Entity> Entities;
        private readonly ConcurrentQueue<Entity> _spawnQueue;
        private readonly ConcurrentQueue<Entity> _despawnQueue;

        public override bool ThreadSafe => false;
        public bool Running { get; set; }

        public event Action Started;

        private long tick, tock;
        private long _frameCounter = 0;
        public NavMeshQuery NavMeshQuery;


        public GamePlugin(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            Tickrate = Convert.ToInt32(pluginLoadData.Settings.Get(nameof(Tickrate)));

            Entities = new Dictionary<uint, Entity>();
            _spawnQueue = new ConcurrentQueue<Entity>();
            _despawnQueue = new ConcurrentQueue<Entity>();
        }

        public void LoadLevel(string levelName)
        {
            NavMeshQuery = NavMeshSerializer.CreateMeshQuery(NavMeshSerializer.Deserialize("Levels/" + levelName + ".nav"));
            //The YOffset requires the world to has a valid NavMesh-Position at (0,0,0) 
            Pathfinder.YOffset = Pathfinder.GetClosestPointOnNavMesh(NavMeshQuery, TundraNetPosition.Create(0f, 0f, 0f)).Y;
            AddEntity(new Monster {Name = "Monster", Position = TundraNetPosition.Zero});
        }

        public void AddEntity(Entity entity)
        {
            entity.ID = _nextEntityID++;
            entity.Game = this;
            entity.Position = TundraNetPosition.Zero;
            entity.AddComponent<SpawnComponent>();
            var navComponent = entity.AddComponent<NavigationComponent>();
            navComponent.NavMeshQuery = NavMeshQuery;
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

            float updateTime = 1000 / (float)Tickrate;
            long lastTime = DateTime.Now.Ticks;

            while (Running)
            {
                long now = DateTime.Now.Ticks;
                var delta = (float)TimeSpan.FromTicks(now - lastTime).TotalSeconds;
                lastTime = now;

                while (_spawnQueue.Count > 0 && _spawnQueue.TryDequeue(out var entity))
                {
                    //TODO: replace foreach-over-all-entites with foreach-entity-in-AREA-OF-INTERESET

                    //let all players observe this unit 
                    foreach (Player player in Entities.Values.Where(e => e is Player))
                    {
                        WriteEvent("Adding " + player.ID + " as observer to " + entity.ID, LogType.Info);
                        entity.Observers.Add(player);
                    }

                    Entities.Add(entity.ID, entity);
                    if (entity is Player newPlayer)
                    {
                        //register the player to all units (around him, including himself), so they send him notifications when something updates
                        foreach (var unit in Entities.Values)
                        {
                            WriteEvent("Adding new Player " + newPlayer.ID + " as observer to " + unit.ID, LogType.Info);
                            unit.Observers.Add(newPlayer);
                        }
                    }

                    entity.Start();
                    //entity.GetComponent<NavigationComponent>().Navigate(TundraNetPosition.Create(5f, 0f, 5f));
                }

                while (_despawnQueue.Count > 0 && _despawnQueue.TryDequeue(out var entity))
                {
                    Entities.Remove(entity.ID);
                    entity.Destroy();
                }
                
                foreach (var entity in Entities.Values)
                {
                    entity.Update(delta);

                }

                foreach (var entity in Entities.Values)
                {
                    entity.LateUpdate();
                }

                var elapsed = (float) TimeSpan.FromTicks(DateTime.Now.Ticks - now).TotalMilliseconds;
                if (elapsed < updateTime)
                    Thread.Sleep((int) (updateTime - elapsed));
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