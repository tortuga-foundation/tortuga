using System.Threading.Tasks;
using System.Numerics;
using Tortuga.Graphics;

namespace Tortuga.Test
{
    class Program
    {
        // public class RotatorSystem : Core.BaseSystem
        // {
        //     private float _rotation = 0.0f;
        //     private Vector3 _position = new Vector3(0, 0, -5);
        //     public override Task Update()
        //     {
        //         return Task.Run(() =>
        //         {
        //             var meshes = MyScene.GetComponents<Graphics.Renderer>();
        //             foreach (var mesh in meshes)
        //             {
        //                 var transform = mesh.MyEntity.GetComponent<Core.Transform>();
        //                 if (transform != null)
        //                 {
        //                     transform.Rotation = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), _rotation);
        //                     _rotation += Time.DeltaTime * 1.0f;
        //                     transform.Position = _position;
        //                 }
        //             }
        //             if (Input.InputModule.IsKeyDown(Input.KeyCode.W))
        //                 _position.Z += Time.DeltaTime * 2.0f;
        //             if (Input.InputModule.IsKeyDown(Input.KeyCode.S))
        //                 _position.Z -= Time.DeltaTime * 2.0f;
        //             if (Input.InputModule.IsKeyDown(Input.KeyCode.D))
        //                 _position.X += Time.DeltaTime * 2.0f;
        //             if (Input.InputModule.IsKeyDown(Input.KeyCode.A))
        //                 _position.X -= Time.DeltaTime * 2.0f;
        //         });
        //     }
        // }

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
            {
                var entity = new Core.Entity();
                var transform = entity.GetComponent<Core.Transform>();
                transform.Position = new Vector3(0, 0, -5);
                var renderer = await entity.AddComponent<Graphics.MeshRenderer>();
                renderer.Mesh = new Graphics.Mesh();
                await renderer.Mesh.LoadObj("Assets/Models/Sphere.obj");
                renderer.Material = new Material();
                renderer.Material.SetShaders(
                    "Assets/Shaders/Default/MRT.vert",
                    "Assets/Shaders/Default/MRT.frag"
                );
                renderer.Material.InsertKey(
                    "TEXTURES",
                    new Graphics.API.DescriptorLayout(
                        Engine.Instance.GetModule<GraphicsModule>().GraphicsService.PrimaryDevice,
                        new System.Collections.Generic.List<Graphics.API.DescriptorBindingInfo>
                        {
                            new Graphics.API.DescriptorBindingInfo
                            {
                                DescriptorCounts = 1,
                                DescriptorType = Vulkan.VkDescriptorType.CombinedImageSampler,
                                Index = 0,
                                ShaderStageFlags = Vulkan.VkShaderStageFlags.Fragment
                            },
                            new Graphics.API.DescriptorBindingInfo
                            {
                                DescriptorCounts = 1,
                                DescriptorType = Vulkan.VkDescriptorType.CombinedImageSampler,
                                Index = 1,
                                ShaderStageFlags = Vulkan.VkShaderStageFlags.Fragment
                            },
                            new Graphics.API.DescriptorBindingInfo
                            {
                                DescriptorCounts = 1,
                                DescriptorType = Vulkan.VkDescriptorType.CombinedImageSampler,
                                Index = 2,
                                ShaderStageFlags = Vulkan.VkShaderStageFlags.Fragment
                            }
                        }
                    )
                );
                renderer.Material.InsertKey(
                    "MATERIAL",
                    new Graphics.API.DescriptorLayout(
                        Engine.Instance.GetModule<GraphicsModule>().GraphicsService.PrimaryDevice,
                        new System.Collections.Generic.List<Graphics.API.DescriptorBindingInfo>
                        {
                            new Graphics.API.DescriptorBindingInfo
                            {
                                DescriptorCounts = 1,
                                DescriptorType = Vulkan.VkDescriptorType.UniformBuffer,
                                Index = 0,
                                ShaderStageFlags = Vulkan.VkShaderStageFlags.All
                            }
                        }
                    )
                );
                {
                    var colorTexture = new Texture();
                    await colorTexture.Load("Assets/Images/Bricks/Color.jpg");
                    renderer.Material.BindImage("TEXTURES", 0, colorTexture);
                }
                {
                    var normalTexture = new Texture();
                    await normalTexture.Load("Assets/Images/Bricks/Normal.jpg");
                    renderer.Material.BindImage("TEXTURES", 1, normalTexture);
                }
                {
                    var metalTexture = new Texture();
                    await metalTexture.Load("Assets/Images/Bricks/Metal.jpg");
                    var roughness = new Texture();
                    await roughness.Load("Assets/Images/Bricks/Roughness.jpg");
                    var aoTexture = new Texture();
                    await aoTexture.Load("Assets/Images/Bricks/AmbientOclusion.jpg");

                    metalTexture.CopyChannel(roughness, TextureChannelFlags.G);
                    metalTexture.CopyChannel(roughness, TextureChannelFlags.B);
                    renderer.Material.BindImage("TEXTURES", 2, metalTexture);
                }
                {
                    renderer.Material.BindBuffer("MATERIAL", 0, new int[] { 1 });
                }

                // renderer.MeshData = await Graphics.Mesh.Load("Assets/Models/Sphere.obj");
                // renderer.MaterialData = await Graphics.Material.Load("Assets/Materials/Bricks.json");
                scene.AddEntity(entity);
            }

            // //light
            // {
            //     var entity = new Core.Entity();
            //     var light = await entity.AddComponent<Graphics.Light>();
            //     scene.AddEntity(entity);
            // }

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
            //scene.AddSystem<RotatorSystem>();

            Engine.Instance.LoadScene(scene);
            await Engine.Instance.Run();
        }
    }
}
