namespace Tortuga.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var engine = new Engine();

            //create new scene
            var scene = new Core.Scene();

            //camera
            var camera = new Core.Entity();
            camera.AddComponent<Components.Camera>();
            scene.AddEntity(camera);

            //entity
            var triangle = new Core.Entity();
            triangle.AddComponent<Components.Mesh>();
            scene.AddEntity(triangle);

            scene.AddSystem<Systems.RenderingSystem>();
            var acr = scene.AddSystem<Systems.AutoCameraResolution>();
            acr.Scale = 0.5f; //camera should render with 50% of the window resolution

            engine.LoadScene(scene);
            engine.Run();
        }
    }
}
