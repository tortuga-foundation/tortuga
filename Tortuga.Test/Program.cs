﻿using System.Threading.Tasks;
using System.Numerics;
using System.Drawing;
using Tortuga.Utils;

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
            var camera = new Core.Entity();
            await camera.AddComponent<Components.Camera>();
            scene.AddEntity(camera);

            //load obj model
            var cube = new OBJLoader("Assets/Models/Monkey.obj");

            //light
            var light = new Core.Entity();
            var lTransform = await light.AddComponent<Components.Transform>();
            lTransform.Position = new Vector3(0, 5, -10);
            var lComp = await light.AddComponent<Components.Light>();
            lComp.Intensity = 1.0f;
            lComp.Range = 1.0f;
            lComp.Type = Components.Light.LightType.Point;
            lComp.Color = Color.White;
            scene.AddEntity(light);

            //entity
            var triangle = new Core.Entity();
            var transform = await triangle.AddComponent<Components.Transform>();
            transform.Position = new Vector3(0, 0, -10);
            transform.IsStatic = false;
            var mesh = await triangle.AddComponent<Components.Mesh>();
            scene.AddEntity(triangle);
            await mesh.SetVertices(cube.ToGraphicsVertices);
            await mesh.SetIndices(cube.ToGraphicsIndex);

            scene.AddSystem<Systems.RenderingSystem>();
            scene.AddSystem<AutoRotator>();
            var acr = scene.AddSystem<Systems.AutoCameraResolution>();
            acr.Scale = 0.5f; //camera should render with 50% of the window resolution

            //user interface test
            {
                var userInterface = new Core.Entity();
                var uTransform = await userInterface.AddComponent<Components.Transform>();
                uTransform.Scale = Vector3.One * 0.1f;
                var ui = await userInterface.AddComponent<Components.UserInterface>();
                scene.AddEntity(userInterface);
            }

            engine.LoadScene(scene);
            await engine.Run();
        }
    }
}
