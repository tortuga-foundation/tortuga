using System.Threading.Tasks;
using System.Numerics;
using System.Drawing;
using Tortuga.Utils;

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

            //load obj model
            var sphere = new OBJLoader("Assets/Models/Sphere.obj");

            //light
            {
                var e = new Core.Entity();
                var transform = await e.AddComponent<Components.Transform>();
                transform.Position = new Vector3(0, 0, -7);
                var light = await e.AddComponent<Components.Light>();
                light.Intensity = 1.0f;
                light.Type = Components.Light.LightType.Point;
                light.Color = Color.White;
                scene.AddEntity(e);
            }

            //sphere 1
            {
                var e = new Core.Entity();
                var transform = await e.AddComponent<Components.Transform>();
                transform.Position = new Vector3(0, 0, -10);
                transform.IsStatic = false;
                var mesh = await e.AddComponent<Components.Mesh>();
                await mesh.SetVertices(sphere.ToGraphicsVertices);
                await mesh.SetIndices(sphere.ToGraphicsIndex);
                mesh.ActiveMaterial = Graphics.Material.Load("Assets/Material/Bricks.json");
                scene.AddEntity(e);
            }

            scene.AddSystem<Systems.RenderingSystem>();
            scene.AddSystem<AutoRotator>();
            scene.AddSystem<LightMovement>();

            engine.LoadScene(scene);
            await engine.Run();
        }
    }
}
