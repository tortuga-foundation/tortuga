using System;

namespace Tortuga.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var engine = new Engine();

            //create new scene
            var scene = new Core.Scene();

            var camera = new Core.Entity();
            camera.AddComponent<Components.Camera>();
            scene.AddEntity(camera);

            engine.LoadScene(scene);
            engine.Run();
        }
    }
}
