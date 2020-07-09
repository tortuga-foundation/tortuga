using System.Threading.Tasks;
using System.Numerics;

namespace Tortuga.Test
{
    class Program
    {
        public class RotatorSystem : Core.BaseSystem
        {
            private float _rotation = 0.0f;
            private Vector3 _position = new Vector3(0, 0, -5);
            public override Task Update()
            {
                return Task.Run(() =>
                {
                    var meshes = MyScene.GetComponents<Graphics.Renderer>();
                    foreach (var mesh in meshes)
                    {
                        var transform = mesh.MyEntity.GetComponent<Core.Transform>();
                        if (transform != null)
                        {
                            transform.Rotation = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), _rotation);
                            _rotation += Time.DeltaTime * 1.0f;
                            transform.Position = _position;
                        }
                    }
                    if (Input.InputModule.IsKeyDown(Input.KeyCode.W))
                        _position.Z += Time.DeltaTime * 2.0f;
                    if (Input.InputModule.IsKeyDown(Input.KeyCode.S))
                        _position.Z -= Time.DeltaTime * 2.0f;
                    if (Input.InputModule.IsKeyDown(Input.KeyCode.D))
                        _position.X += Time.DeltaTime * 2.0f;
                    if (Input.InputModule.IsKeyDown(Input.KeyCode.A))
                        _position.X -= Time.DeltaTime * 2.0f;
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
            Input.InputModule.OnApplicationClose += () => Engine.Instance.IsRunning = false;

            //camera
            Graphics.Camera mainCamera;
            {
                var entity = new Core.Entity();
                mainCamera = await entity.AddComponent<Graphics.Camera>();
                mainCamera.RenderTarget = Graphics.Camera.TypeOfRenderTarget.DeferredRendering;
                scene.AddEntity(entity);
            }

            //mesh
            {
                var entity = new Core.Entity();
                var transform = entity.GetComponent<Core.Transform>();
                transform.Position = new Vector3(0, 0, -5);
                var renderer = await entity.AddComponent<Graphics.Renderer>();
                renderer.MeshData = await Graphics.Mesh.Load("Assets/Models/Sphere.obj");
                renderer.MaterialData = new Graphics.Material("Assets/Shaders/Default/MRT.vert", "Assets/Shaders/Default/MRT.frag");
                await renderer.MaterialData.SetShading(Graphics.ShadingType.Smooth);
                var colorTexture = await Graphics.Texture.Load("Assets/Images/Bricks/Color.jpg");
                var normalTexture = await Graphics.Texture.Load("Assets/Images/Bricks/Normal.jpg");
                var detailTexture = await Graphics.Texture.Load("Assets/Images/Bricks/Metal.jpg");
                detailTexture.CopyChannel(await Graphics.Texture.Load("Assets/Images/Bricks/Roughness.jpg"), Graphics.Texture.Channel.G);
                detailTexture.CopyChannel(await Graphics.Texture.Load("Assets/Images/Bricks/AmbientOclusion.jpg"), Graphics.Texture.Channel.B);
                await renderer.MaterialData.SetColor(colorTexture.Pixels, colorTexture.Width, colorTexture.Height);
                await renderer.MaterialData.SetNormal(normalTexture.Pixels, normalTexture.Width, normalTexture.Height);
                await renderer.MaterialData.SetDetail(detailTexture.Pixels, detailTexture.Width, detailTexture.Height);
                scene.AddEntity(entity);
            }

            //light
            {
                var entity = new Core.Entity();
                var light = await entity.AddComponent<Graphics.Light>();
                scene.AddEntity(entity);
            }

            //user interface
            {
                var win = new UI.UiWindow();
                win.Position = new Vector2(100, 100);
                win.Scale = new Vector2(500, 500);
                scene.AddUserInterface(win);
                var windowContent = new UI.UiRenderable();
                windowContent.RenderFromCamera = mainCamera;
                windowContent.PositionXConstraint = new UI.PercentConstraint(0.0f);
                windowContent.PositionYConstraint = new UI.PercentConstraint(0.0f);
                windowContent.ScaleXConstraint = new UI.PercentConstraint(1.0f);
                windowContent.ScaleYConstraint = new UI.PercentConstraint(1.0f);
                win.Content.Add(windowContent);
            }

            scene.AddSystem<Audio.AudioSystem>();
            scene.AddSystem<Graphics.RenderingSystem>();
            scene.AddSystem<RotatorSystem>();

            Engine.Instance.LoadScene(scene);
            await Engine.Instance.Run();
        }
    }
}
