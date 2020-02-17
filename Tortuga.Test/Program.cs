using System.Threading.Tasks;
using System.Numerics;

namespace Tortuga.Test
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var engine = new Engine();

            //create new scene
            var scene = new Core.Scene();

            //camera
            var camera = new Core.Entity();
            await camera.AddComponent<Components.Camera>();
            scene.AddEntity(camera);

            //entity
            var triangle = new Core.Entity();
            var transform = await triangle.AddComponent<Components.Transform>();
            transform.Position = new Vector3(0, 0, -5);
            var mesh = await triangle.AddComponent<Components.Mesh>();
            scene.AddEntity(triangle);
            await mesh.SetVertices(new Graphics.Vertex[]{
                new Graphics.Vertex(){ Position = new Vector3(0, -0.5f, 0) },
                new Graphics.Vertex(){ Position = new Vector3(0.5f, 0, 0) },
                new Graphics.Vertex(){ Position = new Vector3(-0.5f, 0, 0) }
            });
            await mesh.SetIndices(new uint[] { 0, 1, 2 });

            scene.AddSystem<Systems.RenderingSystem>();
            var acr = scene.AddSystem<Systems.AutoCameraResolution>();
            acr.Scale = 0.5f; //camera should render with 50% of the window resolution

            engine.LoadScene(scene);
            engine.Run();
        }
    }
}
