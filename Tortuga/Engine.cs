using Tortuga.Graphics;
using Tortuga.Graphics.API;

namespace Tortuga
{
    public class Engine
    {
        public static Engine Instance => _instance;
        internal VulkanInstance Vulkan => _vulkan;
        internal Device MainDevice => _vulkan.Devices[0];
        public Window MainWindow => _mainWindow;

        private static Engine _instance;
        private VulkanInstance _vulkan;
        private Window _mainWindow;

        public Engine()
        {
            if (Engine._instance != null)
                throw new System.Exception("only 1 engine can be active at once");
            Engine._instance = this;
            this._vulkan = new VulkanInstance();
            _mainWindow = new Window("tortuga", 50, 50, 1920, 1080, Veldrid.Sdl2.SDL_WindowFlags.AllowHighDpi, true);
        }

        public void Run()
        {
            while (this._mainWindow.Exists)
            {
                this._mainWindow.PumpEvents();
            }
        }
    }
}