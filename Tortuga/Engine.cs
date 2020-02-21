using Tortuga.Graphics;
using Tortuga.Graphics.API;
using Vulkan;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Tortuga
{
    public class Engine
    {
        public static Engine Instance => _instance;
        internal VulkanInstance Vulkan => _vulkan;
        internal Device MainDevice => _vulkan.Devices[0];
        public Window MainWindow => _mainWindow;
        internal RenderPass MainRenderPass => _mainRenderPass;
        internal DescriptorSetLayout CameraDescriptorLayout => _cameraDescriptorLayout;

        private static Engine _instance;
        private VulkanInstance _vulkan;
        private Window _mainWindow;
        private RenderPass _mainRenderPass;
        private DescriptorSetLayout _cameraDescriptorLayout;

        private Core.Scene _activeScene;
        private Global _global;

        public Engine()
        {
            if (Engine._instance != null)
                throw new System.Exception("only 1 engine can be active at once");
            Engine._instance = this;
            this._vulkan = new VulkanInstance();
            Veldrid.Sdl2.SDL_WindowFlags windowFlags = Veldrid.Sdl2.SDL_WindowFlags.AllowHighDpi;
            if (Settings.Window.Type == Settings.Window.WindowType.ResizeableWindow)
                windowFlags |= Veldrid.Sdl2.SDL_WindowFlags.Resizable;
            else if (Settings.Window.Type == Settings.Window.WindowType.Fullscreen)
                windowFlags |= Veldrid.Sdl2.SDL_WindowFlags.Fullscreen;
            else if (Settings.Window.Type == Settings.Window.WindowType.Borderless)
                windowFlags |= Veldrid.Sdl2.SDL_WindowFlags.Borderless;
            _mainWindow = new Window("tortuga", 0, 0, 1920, 1080, windowFlags, true);
            _mainRenderPass = new RenderPass();
            _cameraDescriptorLayout = new DescriptorSetLayout(new DescriptorSetCreateInfo[]
            {
                new DescriptorSetCreateInfo
                {
                    stage = VkShaderStageFlags.All,
                    type = VkDescriptorType.UniformBuffer
                }
            });
            _global = new Global();
            Input.Input.Initialize();
        }

        public Task Run()
        {
            return Task.Run(() =>
            {
                while (this._mainWindow.Exists)
                {
                    var events = this._mainWindow.PumpEvents();
                    Input.Input.ProcessEvents(events);
                    this._mainWindow.AcquireSwapchainImage();
                    if (_activeScene != null)
                    {
                        var tasks = new List<Task>();
                        foreach (var system in _activeScene.Systems.Values)
                            tasks.Add(system.Update());

                        foreach (var entity in _activeScene.Entities)
                        {
                            foreach (var component in entity.Components)
                                tasks.Add(component.Value.Update());
                        }
                        Task.WaitAll(tasks.ToArray());
                    }
                    this._mainWindow.Present();
                }
            });
        }

        public void LoadScene(Core.Scene scene)
        {
            _activeScene = scene;
        }

        public void UnloadScene(Core.Scene scene)
        {
            _activeScene = new Core.Scene();
        }
    }
}