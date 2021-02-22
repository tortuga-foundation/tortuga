# Tortuga

Tortua is an open source game engine built using C# dot net core 3.0

![.NET Core](https://github.com/tortuga-foundation/tortuga/workflows/.NET%20Core/badge.svg?branch=master)

![IMG](https://raw.githubusercontent.com/tortuga-foundation/tortuga/master/Assets/Images/Render/Bricks.png)

## Prerequisites

- Dot Net Core 3.0
- Vulkan
- GlsLang Tools
- SDL2
- Open AL
- Bullet Physics Library

```
sudo apt install -y libopenal1 libsdl2-2.0-0 libvulkan1 glslang-tools libgdiplus
```

## Using the package in your project

You can use nuget to install the package `TortugaEngine`

`nuget install TortugaEngine`

## How to Run

1. `git clone https://github.com/tortuga-foundation/tortuga.git`
2. `cd tortuga`
3. `dotnet restore tortuga.sln`
4. `dotnet build tortuga.sln`
5. `./Tortuga.Test/bin/Debug/netcoreapp3.0/Tortuga.Test.dll`

## Example

#### Sample Code:
```c#
//setup sdl input system
Engine.Instance.AddModule<Input.InputModule>();
//setup vulkan instance
Engine.Instance.AddModule<Graphics.GraphicsModule>();
//setup open al
Engine.Instance.AddModule<Audio.AudioModule>();

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
    renderer.MaterialData = await Graphics.Material.Load("Assets/Materials/Bricks.json");
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

//add systems to the scene
scene.AddSystem<Audio.AudioSystem>();
scene.AddSystem<Graphics.RenderingSystem>();

//load scene
Engine.Instance.LoadScene(scene);
//run main loop
await Engine.Instance.Run();
```

#### Material JSON
```json
{
  "Type": "Material",
  "Shaders": {
    "VertexPath": "Assets/Shaders/Default/MRT.vert",
    "FragmentPath": "Assets/Shaders/Default/MRT.frag"
  },
  "DescriptorSets": [
    {
      "Type": "DescriptorSet",
      "Name": "TEXTURES",
      "Bindings": [
        {
          "Type": "Binding",
          "Stage": "Fragment",
          "DescriptorType": "CombinedImageSampler",
          "Value": {
            "Type": "Image",
            "Data": "Assets/Images/Bricks/Color.jpg"
          }
        },
        {
          "Type": "Binding",
          "Stage": "Fragment",
          "DescriptorType": "CombinedImageSampler",
          "Value": {
            "Type": "Image",
            "Data": "Assets/Images/Bricks/Normal.jpg"
          }
        },
        {
          "Type": "Binding",
          "Stage": "Fragment",
          "DescriptorType": "CombinedImageSampler",
          "Value": {
            "Type": "ImageChannels",
            "Data": {
              "R": "Assets/Images/Bricks/Metal.jpg",
              "G": "Assets/Images/Bricks/Roughness.jpg",
              "B": "Assets/Images/Bricks/AmbientOclusion.jpg"
            }
          }
        }
      ]
    },
    {
      "Type": "DescriptorSet",
      "Name": "MATERIAL",
      "Bindings": [
        {
          "Type": "Binding",
          "Stage": "Vertex",
          "DescriptorType": "UniformBuffer",
          "Value": {
            "Type": "Int32",
            "Data": 1
          }
        }
      ]
    }
  ]
}
```
