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
            {
                var entity = new Core.Entity();
                var camera = await entity.AddComponent<Graphics.Camera>();
                camera.RenderTarget = Graphics.Camera.TypeOfRenderTarget.DeferredRendering;
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
                var colorTexture = await Graphics.Texture.Load("Assets/Images/Metal/Color.jpg");
                var normalTexture = await Graphics.Texture.Load("Assets/Images/Metal/Normal.jpg");
                var detailTexture = await Graphics.Texture.Load("Assets/Images/Metal/Metal.jpg");
                detailTexture.CopyChannel(await Graphics.Texture.Load("Assets/Images/Metal/Roughness.jpg"), Graphics.Texture.Channel.G);
                detailTexture.CopyChannel(Graphics.Texture.SingleColor(System.Drawing.Color.White), Graphics.Texture.Channel.B);
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

            scene.AddSystem<Audio.AudioSystem>();
            scene.AddSystem<Graphics.RenderingSystem>();
            scene.AddSystem<RotatorSystem>();

            Engine.Instance.LoadScene(scene);
            await Engine.Instance.Run();
        }
    }
}
