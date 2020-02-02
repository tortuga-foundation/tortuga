using System;
using System.Runtime.InteropServices;
using Vulkan;
using static Vulkan.VulkanNative;

namespace Tortuga.Graphics.API
{
    internal class VulkanInstance
    {
        public VkInstance Handle => _instanceHandle;
        private VkInstance _instanceHandle;

        public unsafe VulkanInstance()
        {
            //vulkan extensions
            var instanceExtensions = new NativeList<IntPtr>();
            instanceExtensions.Add(Strings.VK_KHR_SURFACE_EXTENSION_NAME);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                instanceExtensions.Add(Strings.VK_KHR_XLIB_SURFACE_EXTENSION_NAME);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                instanceExtensions.Add(Strings.VK_KHR_WIN32_SURFACE_EXTENSION_NAME);
            else
                throw new PlatformNotSupportedException("this platform is not supported");


            //vulkan validation layers
            var validationLayer = new NativeList<IntPtr>();
            if (Settings.Vulkan.DebugLevel != Settings.Vulkan.DebugType.None)
                validationLayer.Add(Strings.StandardValidationLayeName);

            //create vulkan info
            var instanceInfo = VkInstanceCreateInfo.New();
            instanceInfo.enabledExtensionCount = instanceExtensions.Count;
            instanceInfo.ppEnabledExtensionNames = (byte**)instanceExtensions.Data;
            instanceInfo.enabledLayerCount = validationLayer.Count;
            instanceInfo.ppEnabledLayerNames = (byte**)validationLayer.Data;

            var instance = new VkInstance();
            if (vkCreateInstance(&instanceInfo, null, &instance) != VkResult.Success)
                throw new Exception("failed to initialize vulkan");
            this._instanceHandle = instance;
        }
    }
}