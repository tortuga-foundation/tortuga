using Tortuga.Graphics.API;

namespace Tortuga
{
    public class Engine
    {
        public static Engine Instance => _instance;
        internal VulkanInstance Vulkan => _vulkan;

        private static Engine _instance;
        private VulkanInstance _vulkan;

        public Engine()
        {
            if (Engine._instance != null)
                throw new System.Exception("only 1 engine can be active at once");
            Engine._instance = this;
            this._vulkan = new Graphics.API.VulkanInstance();
        }

        public void Run()
        {
            while (true)
            {

            }
        }
    }
}