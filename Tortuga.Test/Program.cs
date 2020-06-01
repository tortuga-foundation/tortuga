using System.Threading.Tasks;
using System.Numerics;

namespace Tortuga.Test
{
    class Program
    {
        public class RotatorSystem : Core.BaseSystem
        {
            private float _rotation = 0.0f;

            public override Task EarlyUpdate()
            {
                return Task.Run(() => { });
            }

            public override Task LateUpdate()
            {
                return Task.Run(() => { });
            }

            public override void OnDisable()
            {
            }

            public override void OnEnable()
            {
            }

            public override Task Update()
            {
                return Task.Run(() =>
                {
                    var meshes = MyScene.GetComponents<Graphics.Renderer>();
                    foreach(var mesh in meshes)
                    {
                        var transform = mesh.MyEntity.GetComponent<Core.Transform>();
                        if (transform != null)
                        {
                            transform.Rotation = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), _rotation);
                            _rotation += Time.DeltaTime * 1.0f;
                        }
                    }
                });
            }
        }

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
                camera.RenderTarget = Graphics.Camera.TypeOfRenderTarget.Detail;
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
                renderer.MaterialData = new Graphics.Material("Assets/Shaders/Default/MRT.vert", "Assets/Shaders/Default/MRT.frag");
                var colorTexture = await Graphics.Texture.Load("Assets/Images/Bricks/Color.jpg");
                var normalTexture = await Graphics.Texture.Load("Assets/Images/Bricks/Normal.jpg");
                await renderer.MaterialData.SetColor(colorTexture.Pixels, colorTexture.Width, colorTexture.Height);
                await renderer.MaterialData.SetNormal(normalTexture.Pixels, normalTexture.Width, normalTexture.Height);
                scene.AddEntity(entity);
            }

            scene.AddSystem<Audio.AudioSystem>();
            scene.AddSystem<Graphics.RenderingSystem>();
            scene.AddSystem<RotatorSystem>();

            Engine.Instance.LoadScene(scene);
            await Engine.Instance.Run();
        }
    }
}
