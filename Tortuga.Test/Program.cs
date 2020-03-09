﻿using System.Threading.Tasks;
using System.Numerics;
using System.Drawing;

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
            {
                var entity = new Core.Entity();
                var camera = await entity.AddComponent<Components.Camera>();
                camera.FieldOfView = 90;
                scene.AddEntity(entity);
            }

            //load obj model
            var sphereOBJ = await Utils.OBJLoader.Load("Assets/Models/Sphere.obj");
            //load bricks material
            var bricksMaterial = await Utils.MaterialLoader.Load("Assets/Material/Bricks.json");

            //user interface
            {
                var button = new Core.Entity();
                var ui = await button.AddComponent<Components.UserInterface>();
                ui.Position = new Vector2(150, 150);
                ui.Scale = new Vector2(200, 200);
                ui.BorderRadius = 50.0f;
                scene.AddEntity(button);
            }

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
                light.Color = Color.White;
                scene.AddEntity(entity);
            }

            //sphere 1
            {
                var entity = new Core.Entity();
                var transform = await entity.AddComponent<Components.Transform>();
                transform.Position = new Vector3(0, 0, -10);
                transform.IsStatic = false;
                //add mesh component
                var mesh = await entity.AddComponent<Components.Mesh>();
                await mesh.SetVertices(sphereOBJ.ToGraphicsVertices);
                await mesh.SetIndices(sphereOBJ.ToGraphicsIndex);
                mesh.ActiveMaterial = bricksMaterial;

                scene.AddEntity(entity);
            }

            //add systems to the scene
            scene.AddSystem<Systems.RenderingSystem>();
            scene.AddSystem<AutoRotator>();
            scene.AddSystem<LightMovement>();

            engine.LoadScene(scene); //set this scene as currently active
            await engine.Run();
        }
    }
}
