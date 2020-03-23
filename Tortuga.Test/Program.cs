using System.Threading.Tasks;
using System.Numerics;

namespace Tortuga.Test
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //create new scene
            var scene = new Core.Scene();

            //camera
            {
                var entity = new Core.Entity();
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
                var transform = await entity.AddComponent<Components.Transform>();
                transform.Position = new Vector3(0, 0, -10);
                transform.IsStatic = false;
                //add mesh component
                var mesh = await entity.AddComponent<Components.RenderMesh>();
                mesh.Material = bricksMaterial;
                await mesh.SetMesh(sphereOBJ); //this operation is async and might not be done instantly

                scene.AddEntity(entity);
            }

            //sphere 2z
            {
                var entity = new Core.Entity();
                var transform = await entity.AddComponent<Components.Transform>();
                transform.Position = new Vector3(3, 0, -10);
                transform.IsStatic = false;
                //add mesh component
                var mesh = await entity.AddComponent<Components.RenderMesh>();
                mesh.Material = bricksMaterial;
                await mesh.SetMesh(sphereOBJ); //this operation is async and might not be done instantly

                scene.AddEntity(entity);
            }
            var block = new Graphics.UI.UiRenderable();
            block.PositionXConstraint = new Graphics.UI.PercentConstraint(1.0f) - new Graphics.UI.PixelConstraint(310.0f);
            block.PositionYConstraint = new Graphics.UI.PixelConstraint(10.0f);
            block.ScaleXConstraint = new Graphics.UI.PixelConstraint(300.0f);
            block.ScaleYConstraint = new Graphics.UI.PercentConstraint(1.0f) - new Graphics.UI.PixelConstraint(20.0f);
            block.BorderRadius = 20;
            block.Background = System.Drawing.Color.FromArgb(200, 5, 5, 5);
            scene.AddUserInterface(block);

            //add systems to the scene
            scene.AddSystem<Systems.RenderingSystem>();
            scene.AddSystem<AutoRotator>();
            scene.AddSystem<LightMovement>();

            Engine.Instance.LoadScene(scene); //set this scene as currently active
            await Engine.Instance.Run();
        }
    }
}
