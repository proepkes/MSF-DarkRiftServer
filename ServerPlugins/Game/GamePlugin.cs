using System;
using System.Threading;
using DarkRift;
using DarkRift.Server;
using Utils;

namespace ServerPlugins.Game
{
    /// <summary>
    ///     This Plugin goes to the spawned server
    /// </summary>
    public class GamePlugin : ServerPluginBase
    {
        public bool Running { get; set; }
        public int Tickrate { get; set; }


        public event Action Started;

        private long frameCounter = 0;

        public GamePlugin(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            Tickrate = Convert.ToInt32(pluginLoadData.Settings.Get(nameof(Tickrate)));
        }

        public void Start()
        {
            Running = true;

            Started?.Invoke();

            while (Running)
            {
                var time = DateTime.Now.Ticks;
                UpdateGame();
                var deltaT = DateTime.Now.Ticks - time;
                if (deltaT < Tickrate)
                {
                    Thread.Sleep(Tickrate - (int) deltaT);
                }

                ++frameCounter;
            }
        }

        private void UpdateGame()
        {
            //if(frameCounter % Tickrate == 0) //Log every second
            //    WriteEvent("Updating game! Frame: " + frameCounter, LogType.Info);
        }

        public void Stop()
        {
            Running = false;
        }
    }
}