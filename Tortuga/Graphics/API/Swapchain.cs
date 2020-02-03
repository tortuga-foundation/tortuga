using System;
using Vulkan;
using static Vulkan.VulkanNative;

namespace Tortuga.Graphics.API
{
    internal class Swapchain
    {
        private bool[] _queuesSupportingPresentation;
        public unsafe Swapchain(Device device, Window window)
        {
            _queuesSupportingPresentation = new bool[device.QueueFamilyProperties.Count];
            for (int i = 0; i < device.QueueFamilyProperties.Count; i++)
            {
                VkBool32 isSupported = false;
                if (vkGetPhysicalDeviceSurfaceSupportKHR(
                    device.PhysicalDevice,
                    0,
                    window.Surface,
                    &isSupported) != VkResult.Success
                )
                    throw new Exception("failed to check if device supports presentation");
                _queuesSupportingPresentation[i] = isSupported;
            }

            if (Array.FindIndex(
                _queuesSupportingPresentation,
                0,
                _queuesSupportingPresentation.Length,
                b => b) == -1
            )
                throw new NotSupportedException("device does not support presentation");

            var swapchainInfo = VkSwapchainCreateInfoKHR.New();
        }
    }
}