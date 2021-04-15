using System.Threading.Tasks;
using System.Numerics;
using Tortuga.Graphics;
using System.Collections.Generic;

namespace Tortuga.Test
{
    class Program
    {
        public class CameraMovement : Core.BaseSystem
        {
            public override Task EarlyUpdate()
            {
                return Task.Run(() =>
                {
                    var cameras = MyScene.GetComponents<Graphics.Camera>();
                    foreach (var camera in cameras)
                    {
                        var transform = camera.MyEntity.GetComponent<Core.Transform>();
                        if (transform != null)
                        {
                            var position = transform.Position;
                            if (Input.InputModule.IsKeyDown(Input.KeyCode.W))
                                position.Z -= Time.DeltaTime * 2.0f;
                            if (Input.InputModule.IsKeyDown(Input.KeyCode.S))
                                position.Z += Time.DeltaTime * 2.0f;
                            if (Input.InputModule.IsKeyDown(Input.KeyCode.D))
                                position.X += Time.DeltaTime * 2.0f;
                            if (Input.InputModule.IsKeyDown(Input.KeyCode.A))
                                position.X -= Time.DeltaTime * 2.0f;

                            transform.Position = position;
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
            //setup open al
            //Engine.Instance.AddModule<Audio.AudioModule>();

            //create new scene
            var scene = new Core.Scene();
            Input.InputModule.OnApplicationClose += () => Engine.Instance.IsRunning = false;

            //create a window
            var window = new Graphics.Window(
                "Tortuga",
                0, 0,
                1920, 1080
            );

            //camera
            Graphics.Camera mainCamera;
            {
                var entity = new Core.Entity();
                mainCamera = await entity.AddComponent<Graphics.Camera>();
                //set camera's render target to be the window
                mainCamera.RenderTarget = window;
                scene.AddEntity(entity);
            }

            //mesh
            var mesh = await AssetLoader.LoadObj("Assets/Models/Sphere.obj");
            var material = await AssetLoader.LoadMaterial("Assets/Materials/Bricks.instanced.jsonc");
            for (int i = -3; i < 4; i++)
            {
                for (int j = -3; j < 4; j++)
                {
                    var entity = new Core.Entity();

                    //attach transform
                    var transform = entity.GetComponent<Core.Transform>();
                    transform.Position = new Vector3(i * 5, j * 5, -40);

                    //attach mesh renderer
                    var renderer = await entity.AddComponent<Graphics.MeshRenderer>();

                    //setup mesh
                    renderer.Mesh = mesh;

                    //setup material
                    renderer.Material = material;
                    scene.AddEntity(entity);
                }
            }

            //light
            {
                var entity = new Core.Entity();

                //attach light
                var light = await entity.AddComponent<Graphics.Light>();
                scene.AddEntity(entity);
            }

            // //user interface
            // {
            //     var win = new UI.UiWindow();
            //     win.Position = new Vector2(100, 100);
            //     win.Scale = new Vector2(500, 500);
            //     scene.AddUserInterface(win);
            //     var windowContent = new UI.UiRenderable();
            //     windowContent.RenderFromCamera = mainCamera;
            //     windowContent.PositionXConstraint = new UI.PercentConstraint(0.0f);
            //     windowContent.PositionYConstraint = new UI.PercentConstraint(0.0f);
            //     windowContent.ScaleXConstraint = new UI.PercentConstraint(1.0f);
            //     windowContent.ScaleYConstraint = new UI.PercentConstraint(1.0f);
            //     win.Content.Add(windowContent);
            // }

            //scene.AddSystem<Audio.AudioSystem>();
            scene.AddSystem<Graphics.RenderingSystem>();
            scene.AddSystem<CameraMovement>();

            Engine.Instance.LoadScene(scene);
            await Engine.Instance.Run();
        }
    }
}
