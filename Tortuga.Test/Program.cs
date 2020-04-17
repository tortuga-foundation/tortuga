using System.Threading.Tasks;
using System.Numerics;

namespace Tortuga.Test
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Engine.Init();

            //create new scene
            var scene = new Core.Scene();

            //camera
            {
                var entity = new Core.Entity();
                entity.Name = "Camera";
                var listener = await entity.AddComponent<Components.AudioListener>();
                listener.Position = Vector3.Zero;
                listener.Velocity = Vector3.Zero;
                var camera = await entity.AddComponent<Components.Camera>();
                camera.FieldOfView = 90;
                scene.AddEntity(entity);
            }

            //load obj model
            var sphereOBJ = await Graphics.Mesh.Load("Assets/Models/Sphere.obj");
            //load bricks material
            var bricksMaterial = await Graphics.Material.Load("Assets/Material/Bricks.json");

            //light
            {
                var entity = new Core.Entity();
                entity.Name = "Light";
                var transform = await entity.AddComponent<Components.Transform>();
                transform.Position = new Vector3(0, 0, -7);
                transform.IsStatic = true;
                //add light component
                var light = await entity.AddComponent<Components.Light>();
                light.Intensity = 200.0f;
                light.Type = Components.Light.LightType.Point;
                light.Color = System.Drawing.Color.White;
                scene.AddEntity(entity);
            }

            //sphere 1
            {
                var entity = new Core.Entity();
                entity.Name = "sphere 1";
                var source = await entity.AddComponent<Components.AudioSource>();
                source.Position = Vector3.Zero;
                source.Velocity = Vector3.Zero;
                source.Is3D = true;
                source.Pitch = 1.0f;
                source.Gain = 1.0f;
                source.Loop = false;
                source.Clip = await Audio.AudioClip.Load("Assets/Audio/Sample1.wav");
                source.Play();
                var transform = await entity.AddComponent<Components.Transform>();
                transform.Position = new Vector3(0, 0, -10);
                transform.IsStatic = false;
                //add mesh component
                var mesh = await entity.AddComponent<Components.RenderMesh>();
                mesh.Material = bricksMaterial;
                await mesh.SetMesh(sphereOBJ); //this operation is async and might not be done instantly

                scene.AddEntity(entity);
            }

            //sphere 2
            {
                var entity = new Core.Entity();
                entity.Name = "sphere 2";
                var transform = await entity.AddComponent<Components.Transform>();
                transform.Position = new Vector3(3, 0, -10);
                transform.IsStatic = false;
                //add mesh component
                var mesh = await entity.AddComponent<Components.RenderMesh>();
                mesh.Material = bricksMaterial;
                await mesh.SetMesh(sphereOBJ); //this operation is async and might not be done instantly

                scene.AddEntity(entity);
            }

            //user interface
            {
                //create a new ui block element and add it to the scene
                var block = new Graphics.UI.UiBlock();
                scene.AddUserInterface(block);

                //setup block
                block.PositionXConstraint = new Graphics.UI.PercentConstraint(1.0f) - new Graphics.UI.PixelConstraint(310.0f);
                block.PositionYConstraint = new Graphics.UI.PixelConstraint(10.0f);
                block.ScaleXConstraint = new Graphics.UI.PixelConstraint(300.0f);
                block.ScaleYConstraint = new Graphics.UI.PercentConstraint(1.0f) - new Graphics.UI.PixelConstraint(20.0f);
                block.BorderRadius = 20;
                block.Background = System.Drawing.Color.FromArgb(200, 5, 5, 5);

                //create a vertical layout group
                var layout = new Graphics.UI.UiVerticalLayout();
                layout.PositionXConstraint = new Graphics.UI.PixelConstraint(0.0f);
                layout.PositionYConstraint = new Graphics.UI.PixelConstraint(20.0f);
                layout.ScaleXConstraint = new Graphics.UI.PercentConstraint(1.0f) - new Graphics.UI.PixelConstraint(5.0f);
                layout.ScaleYConstraint = new Graphics.UI.ContentAutoFitConstraint();
                layout.Spacing = 0.0f;

                var scrollRect = new Graphics.UI.UiScrollRect();
                scrollRect.PositionXConstraint = new Graphics.UI.PixelConstraint(0.0f);
                scrollRect.PositionYConstraint = new Graphics.UI.PixelConstraint(20.0f);
                scrollRect.ScaleXConstraint = new Graphics.UI.PercentConstraint(1.0f);
                scrollRect.ScaleYConstraint = new Graphics.UI.PercentConstraint(1.0f / 3.0f);
                scrollRect.Viewport = layout;
                block.Add(scrollRect);

                for (int i = 0; i < scene.Entities.Count; i++)
                {
                    int color = i % 2 == 0 ? 10 : 5;

                    var entity = scene.Entities[i];
                    var button = new Graphics.UI.UiButton();
                    button.ScaleXConstraint = new Graphics.UI.PercentConstraint(1.0f);
                    button.ScaleYConstraint = new Graphics.UI.PixelConstraint(40);
                    button.BorderRadius = 0.0f;
                    button.Text.FontSize = 10.0f;
                    button.Text.HorizontalAlignment = Graphics.UI.UiHorizontalAlignment.Left;
                    button.Text.PositionXConstraint = new Graphics.UI.PixelConstraint(10.0f);
                    button.Text.ScaleXConstraint = new Graphics.UI.PercentConstraint(1.0f) - new Graphics.UI.PixelConstraint(20.0f);
                    button.Text.Text = entity.Name;
                    button.Text.TextColor = System.Drawing.Color.White;
                    button.NormalBackground = System.Drawing.Color.FromArgb(255, color, color, color);
                    button.HoverBackground = System.Drawing.Color.FromArgb(255, 50, 50, 50);
                    layout.Add(button);
                }
            }

            //add systems to the scene
            scene.AddSystem<Systems.RenderingSystem>();
            scene.AddSystem<Systems.AudioSystem>();
            scene.AddSystem<AutoRotator>();
            scene.AddSystem<LightMovement>();

            Engine.Instance.LoadScene(scene); //set this scene as currently active
            await Engine.Instance.Run();
        }
    }
}
