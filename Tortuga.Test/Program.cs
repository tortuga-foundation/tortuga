﻿using System.Threading.Tasks;
using System.Numerics;
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

            engine.LoadScene(scene);
            await engine.Run();
        }
    }
}
