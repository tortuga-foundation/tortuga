using Tortuga.Graphics;
using Tortuga.Graphics.API;
using Vulkan;
using System.Threading.Tasks;
using System.Collections.Generic;
using Tortuga.Input;
using Tortuga.Utils.SDL2;

namespace Tortuga
{
    /// <summary>
    /// Engine Singleton class, this is required to run the engine
    /// </summary>
    public class Engine
    {
        /// <summary>
        /// Referance to engine singleton instance
        /// </summary>
        public static Engine Instance => _instance;
        /// <summary>
        /// Main window created by the engine
        /// </summary>
        public Window MainWindow => _mainWindow;
        internal VulkanInstance Vulkan => _vulkan;
        internal Device MainDevice => _vulkan.Devices[0];
        internal RenderPass MainRenderPass => _mainRenderPass;
        internal DescriptorSetLayout CameraDescriptorLayout => _cameraDescriptorLayout;
        internal DescriptorSetLayout ModelDescriptorLayout => _modelDescriptorLayout;
        internal DescriptorSetLayout UiCameraDescriptorLayout => _uiCameraDescriptorLayout;
        internal DescriptorSetLayout UiBaseDescriptorLayout => _uiBaseDescriptorLayout;

        private static Engine _instance = new Engine();
        private VulkanInstance _vulkan;
        private Window _mainWindow;
        private RenderPass _mainRenderPass;
        private DescriptorSetLayout _cameraDescriptorLayout;
        private DescriptorSetLayout _modelDescriptorLayout;
        private DescriptorSetLayout _uiCameraDescriptorLayout;
        private DescriptorSetLayout _uiBaseDescriptorLayout;

        /// <summary>
        /// Returns currently active scene
        /// </summary>
        public Core.Scene CurrentScene => _activeScene;
        private Core.Scene _activeScene;

        /// <summary>
        /// Engine constructor
        /// </summary>
        public Engine()
        {
            //make sure this is a singleton
            if (Engine._instance != null)
                throw new System.Exception("only 1 engine can be active at once");

            //setup vulkan
            Engine._instance = this;
            this._vulkan = new VulkanInstance();

            //setup window
            var windowFlags = SDL_WindowFlags.AllowHighDpi;
            if (Settings.Window.Type == Settings.Window.WindowType.ResizeableWindow)
                windowFlags |= SDL_WindowFlags.Resizable;
            else if (Settings.Window.Type == Settings.Window.WindowType.Fullscreen)
                windowFlags |= SDL_WindowFlags.Fullscreen;
            else if (Settings.Window.Type == Settings.Window.WindowType.Borderless)
                windowFlags |= SDL_WindowFlags.Borderless;
            _mainWindow = new Window("tortuga", 0, 0, 1920, 1080, windowFlags);

            //setup render pass
            _mainRenderPass = new RenderPass();

            //setup camera uniform buffer layout
            _cameraDescriptorLayout = new DescriptorSetLayout(new DescriptorSetCreateInfo[]
            {
                new DescriptorSetCreateInfo
                {
                    stage = VkShaderStageFlags.Vertex,
                    type = VkDescriptorType.UniformBuffer
                }
            });
            _modelDescriptorLayout = new DescriptorSetLayout(new DescriptorSetCreateInfo[]
            {
                new DescriptorSetCreateInfo
                {
                    stage = VkShaderStageFlags.Vertex,
                    type = VkDescriptorType.UniformBuffer
                }
            });
            _uiCameraDescriptorLayout = new DescriptorSetLayout(new DescriptorSetCreateInfo[]{
                new DescriptorSetCreateInfo
                {
                    stage = VkShaderStageFlags.Vertex,
                    type = VkDescriptorType.UniformBuffer
                }
            });
            _uiBaseDescriptorLayout = new DescriptorSetLayout(new DescriptorSetCreateInfo[]{
                new DescriptorSetCreateInfo
                {
                    stage = VkShaderStageFlags.Vertex | VkShaderStageFlags.Fragment,
                    type = VkDescriptorType.UniformBuffer
                }
            });

            //initialize input event system
            InputSystem.Initialize();
        }

        /// <summary>
        /// Main engine loop
        /// </summary>
        /// <returns>Returns task, if not using async await then please use task.Wait()</returns>
        public Task Run()
        {
            return Task.Run(() =>
            {
                Time.StopWatch = new System.Diagnostics.Stopwatch();
                Time.StopWatch.Start();
                while (this._mainWindow.Exists)
                {
                    try
                    {
                        Time.LastFramesTicks = Time.StopWatch.ElapsedTicks;
                        this._mainWindow.PumpEvents();
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
                        //clean up marked for removal
                        var removalTasks = new List<Task>();
                        foreach (var entity in _activeScene.Entities)
                            removalTasks.Add(entity.RemoveAllMarkedForRemoval());
                        Task.WaitAll(removalTasks.ToArray());
                        //Store Time elapsed
                        Time.DeltaTime = (Time.StopWatch.ElapsedTicks - Time.LastFramesTicks) / 10000000.0f;
                    }
                    catch (System.Exception e)
                    {
                        System.Console.WriteLine(e.ToString());
                    }
                }
                MainDevice.WaitForDevice();
            });
        }

        /// <summary>
        /// Load a specific scene
        /// </summary>
        /// <param name="scene">Scene to load</param>
        public void LoadScene(Core.Scene scene)
        {
            if (_activeScene != null && _activeScene != new Core.Scene())
                UnloadScene();

            _activeScene = scene;
            foreach (var system in this._activeScene.Systems)
                system.Value.OnEnable();
        }

        /// <summary>
        /// Unload a scene / clear all entities
        /// </summary>
        public void UnloadScene()
        {
            foreach (var system in this._activeScene.Systems)
                system.Value.OnDisable();

            _activeScene = new Core.Scene();
        }
    }
}