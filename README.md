# Tortuga

Tortua is an open source game engine built using C# dot net core 3.0

![.NET Core](https://github.com/tortuga-foundation/tortuga/workflows/.NET%20Core/badge.svg?branch=master)

![IMG](Assets/Images/Render/Bricks.png)

## Core Features

- Multi-Threaded Rendering
- Entity Component System (With Component Behaviour)
- PBR Shader (Metalness Workflow)
- Event Based Input System
- Material JSON Object
- Full Linux Support

## Prerequisites

- Dot Net Core 3.0
- Vulkan
- SDL (With Vulkan Support)

## How to Run

1. `dotnet restore tortuga.sln`
2. `dotnet build tortuga.sln`
3. `./Tortuga.Test/bin/Debug/netcoreapp3.0/Tortuga.Test.dll`

## Example

#### Sample Code:
```c#
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
var sphereOBJ = await OBJLoader.Load("Assets/Models/Sphere.obj");
//load bricks material
var bricksMaterial = await Utils.MaterialLoader.Load("Assets/Material/Bricks.json");

//light
{
    var entity = new Core.Entity();
    var transform = await entity.AddComponent<Components.Transform>();
    transform.Position = new Vector3(0, 0, -7);
    //add light component
    var light = await entity.AddComponent<Components.Light>();
    light.Intensity = 1.0f;
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
```

#### Material JSON
```json
{
  "Light": true,
  "Shaders": {
    "Vertex": "Assets/Shaders/Default/Default.vert",
    "Fragment": "Assets/Shaders/Default/Default.frag"
  },
  "DescriptorSets": [
    {
      "Type": "UniformData",
      "Name": "PBR",
      "Bindings": [
        {
          "Values": [
            {
              "Type": "Int",
              "Value": 0
            }
          ]
        }
      ]
    },
    {
      "Type": "SampledImage2D",
      "Name": "Textures",
      "Bindings": [
        {
          "Value": "Assets/Images/Bricks/Albedo.jpg",
          "MipLevel": 1
        },
        {
          "Value": "Assets/Images/Bricks/Normal.jpg",
          "MipLevel": 1
        },
        {
          "Value": [
            "Assets/Images/Bricks/Metalness.jpg",
            "Assets/Images/Bricks/Roughness.jpg",
            "Assets/Images/Bricks/AmbientOclusion.jpg"
          ],
          "MipLevel": 1
        }
      ]
    }
  ]
}


```
