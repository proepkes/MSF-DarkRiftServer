using WorldEngine;

namespace WorldEngineTest
{
    class Program
    {
        static void Main(string[] args)
        {
            new Game(new Urho.ApplicationOptions("MyData")).Run();
            // For a console app Urho will create a Windows/macOS/Linux window using SDL 
        }
    }
}
