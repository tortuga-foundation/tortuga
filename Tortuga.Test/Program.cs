using System.Threading.Tasks;
using System.Numerics;

namespace Tortuga.Test
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //setup sdl input system
            Engine.Instance.AddModule<Input.InputModule>();
            //setup vulkan instance
            Engine.Instance.AddModule<Graphics.GraphicsModule>();

            //create new scene
            var scene = new Core.Scene();

            Input.InputModule.OnKeyDown += (Input.KeyCode key, Input.ModifierKeys modifiers) =>
            {
                System.Console.WriteLine(key.ToString());
            };

            var window = new Graphics.Window(
                "Tortuga",
                0, 0,
                1920, 1080,
                Graphics.WindowType.Window
            );
            Input.InputModule.OnWindowClose += (uint windowId) =>
            {
                if (window.WindowIdentifier == windowId)
                    Engine.Instance.IsRunning = false;
            };
            Input.InputModule.OnApplicationClose += () => Engine.Instance.IsRunning = false;

            //camera
            {
                var entity = new Core.Entity();
                var camera = await entity.AddComponent<Graphics.Camera>();
                camera.RenderToWindow = window;
                scene.AddEntity(entity);
            }

            //mesh
            {
                var entity = new Core.Entity();
                var transform = entity.GetComponent<Core.Transform>();
                transform.Position = new Vector3(0, 0, -3);
                var renderer = await entity.AddComponent<Graphics.Renderer>();
                renderer.MeshData = await Graphics.Mesh.Load("Assets/Models/Sphere.obj");
                renderer.MaterialData = new Graphics.Material();
                scene.AddEntity(entity);
            }

            scene.AddSystem<Audio.AudioSystem>();
            scene.AddSystem<Graphics.RenderingSystem>();

            Engine.Instance.LoadScene(scene);
            await Engine.Instance.Run();
        }
    }
}
