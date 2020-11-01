namespace Tortuga.Graphics.API
{
    internal static class VulkanService
    {
        public static VulkanInstance Vulkan
        {
            get
            {
                if (_vulkan == null)
                    _vulkan = new VulkanInstance();
                
                return _vulkan;
            }
        }
        private static VulkanInstance _vulkan;

        public static Device MainDevice
        {
            get
            {
                if (_mainDevice == null)
                {
                    //todo: score each gpu and get the best one
                    _mainDevice = Vulkan.Devices[0];
                }

                return _mainDevice;
            }
        }
        private static Device _mainDevice;

        public static void Init()
        {
            _vulkan = new VulkanInstance();
        }
    }
}