using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DarkRift;
using DarkRift.Server;
using ServerPlugins.Game.Components;
using ServerPlugins.Game.Entities;
using ServerPlugins.Game.Levels;
using ServerPlugins.SharpNav;
using ServerPlugins.SharpNav.Crowds;
using ServerPlugins.SharpNav.Geometry;
using ServerPlugins.SharpNav.Pathfinding;

namespace ServerPlugins.Game
{
    /// <summary>
    ///     This Plugin goes to the spawned server
    /// </summary>
    public class GamePlugin : ServerPluginBase
    {
        //Reserve EntityID 0 for null, (example: no target selected)
        private uint _nextEntityID = 1;

        public readonly Dictionary<uint, Entity> Entities;
        private readonly ConcurrentQueue<Entity> _spawnQueue;
        private readonly ConcurrentQueue<Entity> _despawnQueue;

        public override bool ThreadSafe => false;
        public bool Running { get; set; }
        public int Tickrate { get; set; }

        public event Action Started;

        private long _frameCounter = 0;

        private NavPoint startPt;
        private Heightfield heightfield;
        private ObjModel level;
        public Crowd Crowd;
        public PolyMesh PolyMesh;
        public PolyMeshDetail PolyMeshDetail;
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
            level = new ObjModel("Levels/" + levelName + ".obj");
           
            GenerateNavMesh();
            WriteEvent("Adding Monster at " + startPt.Position + " on polygon " + startPt.Polygon, LogType.Info);
            AddEntity(new Monster {Name = "Monster", Position = startPt.Position});
        }

        public void AddEntity(Entity entity)
        {
            entity.ID = _nextEntityID++;
            entity.Game = this;
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

            int updateTime = (int) (1 / (float) Tickrate * 1000);
            while (Running)
            {
                var time = DateTime.Now.Ticks;

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
                            WriteEvent("Adding new Player " + newPlayer.ID + " as observer to " + unit.ID,
                                LogType.Info);
                            unit.Observers.Add(newPlayer);
                        }
                    }

                    entity.Start();
                }

                while (_despawnQueue.Count > 0 && _despawnQueue.TryDequeue(out var entity))
                {
                    Entities.Remove(entity.ID);
                    entity.Destroy();
                }

                int deltaT = (int) (DateTime.Now.Ticks - time);

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

            foreach (var entity in Entities.Values)
            {
                entity.Update();
            }

            foreach (var entity in Entities.Values)
            {
                entity.LateUpdate();
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

        #region NavMesh

        private void GenerateNavMesh()
        {
            WriteEvent("Generating NavMesh", LogType.Info);

            //level.SetBoundingBoxOffset(new SVector3(settings.CellSize * 0.5f, settings.CellHeight * 0.5f, settings.CellSize * 0.5f));
            var levelTris = level.GetTriangles();
            var triEnumerable = TriangleEnumerable.FromTriangle(levelTris, 0, levelTris.Length);
            BBox3 bounds = triEnumerable.GetBoundingBox();

            var settings = NavMeshGenerationSettings.Default;
            settings.AgentHeight = 2;
            settings.AgentRadius = .5f;
            settings.MaxClimb = 0.5f;
            settings.CellSize = .2f;
            heightfield = new Heightfield(bounds, NavMeshGenerationSettings.Default);
            
            heightfield.RasterizeTriangles(levelTris, Area.Default);
            heightfield.FilterLedgeSpans(settings.VoxelAgentHeight, settings.VoxelMaxClimb);


            heightfield.FilterLowHangingWalkableObstacles(settings.VoxelMaxClimb);


            heightfield.FilterWalkableLowHeightSpans(settings.VoxelAgentHeight);


            var compactHeightfield = new CompactHeightfield(heightfield, settings);


            compactHeightfield.Erode(settings.VoxelAgentRadius);


            compactHeightfield.BuildDistanceField();


            compactHeightfield.BuildRegions(0, settings.MinRegionSize, settings.MergedRegionSize);


            var contourSet = compactHeightfield.BuildContourSet(settings);


            PolyMesh = new PolyMesh(contourSet, settings);


            PolyMeshDetail = new PolyMeshDetail(PolyMesh, compactHeightfield, settings);
            
            //Generate Pathfinding
            var buildData = new NavMeshBuilder(PolyMesh, PolyMeshDetail, new SharpNav.Pathfinding.OffMeshConnection[0], settings);

            var tiledNavMesh = new TiledNavMesh(buildData);
            var navMeshQuery = new NavMeshQuery(tiledNavMesh, 2048);

            //Find random start and end points on the poly mesh
            /*int startRef;
			navMeshQuery.FindRandomPoint(out startRef, out startPos);*/

            var center = new Vector3(10, 0, 0);
            var extents = new Vector3(5, 5, 5);
            navMeshQuery.FindNearestPoly(ref center, ref extents, out startPt);


            //Pathfinding with multiple units
            Crowd = new Crowd(300, 0.6f, ref tiledNavMesh);


            WriteEvent("Navmesh generated.", LogType.Info);
            WriteEvent("Rasterized " + level.GetTriangles().Length + " triangles.", LogType.Info);
            WriteEvent("Generated " + contourSet.Count + " regions.", LogType.Info);
            WriteEvent("PolyMesh contains " + PolyMesh.VertCount + " vertices in " + PolyMesh.PolyCount +" polys.", LogType.Info);
            WriteEvent("PolyMeshDetail contains " + PolyMeshDetail.VertCount + " vertices and " +
                       PolyMeshDetail.TrisCount + " tris in " + PolyMeshDetail.MeshCount + " meshes.", LogType.Info);


        }


        #endregion
    }
}