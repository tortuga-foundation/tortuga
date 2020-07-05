# Tortuga

Tortua is an open source game engine built using C# dot net core 3.0

![.NET Core](https://github.com/tortuga-foundation/tortuga/workflows/.NET%20Core/badge.svg?branch=master)

![IMG](https://raw.githubusercontent.com/tortuga-foundation/tortuga/master/Assets/Images/Render/Bricks.png)

Roadmap: https://trello.com/b/McNszhI0/tortuga

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
//setup engine and load required modules
var engine = new Engine();
engine.AddModule<Audio.AudioModule>();
engine.AddModule<Input.InputModule>();
engine.AddModule<Graphics.GraphicsModule>();

//create new scene
var scene = new Core.Scene();

//audio mixer
var mixer = new Audio.MixerGroup();
mixer.Gain = 2.0f;
mixer.AddEffect(new Audio.Effect.Echo());

//on key down
Input.InputModule.OnKeyDown += (Input.KeyCode key, Input.ModifierKeys modifiers) =>
{
    System.Console.WriteLine(key.ToString());
};

//create window
var window = new Graphics.Window(
    "Tortuga",
    0, 0,
    1920, 1080,
    Graphics.WindowType.Window
);
//if window is closed then quit
Input.InputModule.OnWindowClose += (uint windowId) =>
{
    if (window.WindowIdentifier == windowId)
        engine.IsRunning = false;
};
//if application is quit then stop
Input.InputModule.OnApplicationClose += () =>
{
    engine.IsRunning = false;
};

//entity
{
    var entity = new Core.Entity();
    var camera = await entity.AddComponent<Graphics.Camera>();
    camera.RenderToWindow = window; //render this camera on the window
    scene.AddEntity(entity);
}

scene.AddSystem<Audio.AudioSystem>();
scene.AddSystem<Graphics.RenderingSystem>();

engine.LoadScene(scene);
await engine.Run();
```

#### Material JSON
```json
{
  "Type": "Material",
  "IsInstanced": false,
  "Shaders": {
    "Vertex": "Assets/Shaders/Default/Default.vert",
    "Fragment": "Assets/Shaders/Default/Default.frag"
  },
  "DescriptorSets": [
    {
      "Type": "UniformData",
      "Name": "LIGHT"
    },
    {
      "Type": "UniformData",
      "Name": "Data",
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
          "Image": "Assets/Images/Bricks/Albedo.jpg",
          "MipLevel": 1
        },
        {
          "Image": "Assets/Images/Bricks/Normal.jpg",
          "MipLevel": 1
        },
        {
          "BuildImage": {
            "R": "Assets/Images/Bricks/Metalness.jpg",
            "G": "Assets/Images/Bricks/Roughness.jpg",
            "B": "Assets/Images/Bricks/AmbientOclusion.jpg"
          },
          "MipLevel": 1
        }
      ]
    }
  ]
}
```
