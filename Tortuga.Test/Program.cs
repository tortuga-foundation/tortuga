﻿using System.Threading.Tasks;
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
                entity.Name = "Camera";
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
                var block = new Graphics.UI.UiRenderable();
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
                layout.ScaleXConstraint = new Graphics.UI.PercentConstraint(1.0f);
                layout.ScaleYConstraint = new Graphics.UI.PixelConstraint(200.0f);
                layout.Spacing = 0.0f;
                block.Add(layout);

                for (int i = 0; i < scene.Entities.Count; i++)
                {
                    var entity = scene.Entities[i];
                    var button = new Graphics.UI.UiButton();
                    button.ScaleXConstraint = new Graphics.UI.PercentConstraint(1.0f);
                    button.ScaleYConstraint = new Graphics.UI.PixelConstraint(40);
                    button.BorderRadius = 0.0f;
                    button.Text.FontSize = 10.0f;
                    button.Text.HorizontalAlignment = Graphics.UI.UiHorizontalAlignment.Left;
                    button.Text.PositionXConstraint = new Graphics.UI.PixelConstraint(10.0f);
                    button.Text.ScaleXConstraint = new Graphics.UI.PercentConstraint(1.0f) - new Graphics.UI.PixelConstraint(20.0f);
                    if (i % 2 == 0)
                        button.NormalBackground = System.Drawing.Color.FromArgb(255, 10, 10, 10);
                    else
                        button.NormalBackground = System.Drawing.Color.FromArgb(255, 30, 30, 30);
                    button.Text.Text = entity.Name;
                    layout.Add(button);
                }
            }

            //add systems to the scene
            scene.AddSystem<Systems.RenderingSystem>();
            scene.AddSystem<AutoRotator>();
            scene.AddSystem<LightMovement>();

            Engine.Instance.LoadScene(scene); //set this scene as currently active
            await Engine.Instance.Run();
        }
    }
}
