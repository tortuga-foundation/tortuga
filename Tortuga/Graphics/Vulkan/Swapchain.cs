using System;
using Vulkan;
using static Vulkan.VulkanNative;

namespace Tortuga.Graphics
{
    public class Swapchain
    {
        public struct SwapchainSupportDetails
        {
            public VkSurfaceCapabilitiesKHR Capabilities;
            public NativeList<VkSurfaceFormatKHR> Formats;
            public NativeList<VkPresentModeKHR> PresentModes;
        }

        public VkSwapchainKHR SwapchainHandle => _swapchain;
        public SwapchainSupportDetails SupportDetails => _supportDetails;
        public VkSurfaceFormatKHR SurfaceFormat => _surfaceFormat;
        public VkPresentModeKHR PresentMode => _presentMode;
        public VkExtent2D Extent => _extent;

        private Device _device;
        private Window _window;
        private VkSwapchainKHR _swapchain;
        private SwapchainSupportDetails _supportDetails;
        private VkSurfaceFormatKHR _surfaceFormat;
        private VkPresentModeKHR _presentMode;
        private VkExtent2D _extent;

        public unsafe Swapchain(Device device, Window window)
        {
            this._device = device;
            this._window = window;

            //check if device supports presenting
            var deviceSupportsPresent = VkBool32.False;
            if (vkGetPhysicalDeviceSurfaceSupportKHR(
                this._device.PhysicalDevice,
                this._device.QueueFamilies.Graphics.Index,
                this._window.Surface,
                &deviceSupportsPresent
                ) != VkResult.Success
            )
                throw new Exception("failed to get if device supports presenting");

            if (deviceSupportsPresent != VkBool32.True)
                throw new Exception("device's graphics queue does not support presenting");

            this._supportDetails = this.GetSupportDetails();
            this._surfaceFormat = this.ChooseSurfaceFormat();
            this._presentMode = this.ChoosePresentMode();
            this._extent = this.ChooseExtent();

            var swapchainInfo = VkSwapchainCreateInfoKHR.New();
            swapchainInfo.surface = window.Surface;
            //swapchainInfo.

            VkSwapchainKHR swapchain;
            vkCreateSwapchainKHR(device.LogicalDevice, &swapchainInfo, null, &swapchain);

        }

        private unsafe SwapchainSupportDetails GetSupportDetails()
        {
            if (this._device == null || this._window == null)
                throw new Exception("device and window not specified");

            var data = new SwapchainSupportDetails();

            VkSurfaceCapabilitiesKHR capabilities;
            if (vkGetPhysicalDeviceSurfaceCapabilitiesKHR(this._device.PhysicalDevice, this._window.Surface, &capabilities) != VkResult.Success)
                throw new Exception("failed to get device surface capabilities");
            data.Capabilities = capabilities;

            uint formatCount;
            if (vkGetPhysicalDeviceSurfaceFormatsKHR(this._device.PhysicalDevice, this._window.Surface, &formatCount, null) != VkResult.Success)
                throw new Exception("failed to get physical device surface format");
            data.Formats = new NativeList<VkSurfaceFormatKHR>(formatCount);
            if (vkGetPhysicalDeviceSurfaceFormatsKHR(this._device.PhysicalDevice, this._window.Surface, &formatCount, (VkSurfaceFormatKHR*)data.Formats.Data.ToPointer()) != VkResult.Success)
                throw new Exception("failed to get physical device surface format");
            data.Formats.Count = formatCount;

            uint presentModesCount;
            if (vkGetPhysicalDeviceSurfacePresentModesKHR(this._device.PhysicalDevice, this._window.Surface, &presentModesCount, null) != VkResult.Success)
                throw new Exception("failed to get physical device present modes");
            data.PresentModes = new NativeList<VkPresentModeKHR>(presentModesCount);
            if (vkGetPhysicalDeviceSurfacePresentModesKHR(this._device.PhysicalDevice, this._window.Surface, &presentModesCount, (VkPresentModeKHR*)data.PresentModes.Data.ToPointer()) != VkResult.Success)
                throw new Exception("failed to get physical device present modes");
            data.PresentModes.Count = presentModesCount;

            return data;
        }

        private unsafe VkSurfaceFormatKHR ChooseSurfaceFormat()
        {
            if (this._supportDetails.Formats == null || this._supportDetails.Formats.Count == 0)
                throw new Exception("no support formats found");

            if (this._supportDetails.Formats.Count == 1 && this._supportDetails.Formats[0].format == VkFormat.Undefined)
                return new VkSurfaceFormatKHR()
                {
                    format = VkFormat.R8g8b8Unorm,
                    colorSpace = VkColorSpaceKHR.SrgbNonlinearKHR
                };

            for (int i = 0; i < this._supportDetails.Formats.Count; i++)
            {
                if (this._supportDetails.Formats[i].format == VkFormat.R8g8b8Unorm)
                    if (this._supportDetails.Formats[i].colorSpace == VkColorSpaceKHR.SrgbNonlinearKHR)
                        return this._supportDetails.Formats[i];
            }

            return this._supportDetails.Formats[0];
        }
        private unsafe VkPresentModeKHR ChoosePresentMode()
        {
            var bestMode = VkPresentModeKHR.FifoKHR;
            for (int i = 0; i < this._supportDetails.PresentModes.Count; i++)
            {
                if (this._supportDetails.PresentModes[i] == VkPresentModeKHR.MailboxKHR)
                    return this._supportDetails.PresentModes[i];

                if (this._supportDetails.PresentModes[i] == VkPresentModeKHR.ImmediateKHR)
                    bestMode = this._supportDetails.PresentModes[i];
            }
            return bestMode;
        }
        private unsafe VkExtent2D ChooseExtent()
        {
            if (this._supportDetails.Capabilities.currentExtent.width != uint.MaxValue)
                return this._supportDetails.Capabilities.currentExtent;

            var actualExtent = new VkExtent2D(this._window.NativeWindow.Width, this._window.NativeWindow.Height);
            actualExtent.width = Math.Clamp(actualExtent.width, this._supportDetails.Capabilities.minImageExtent.width, this._supportDetails.Capabilities.maxImageExtent.width);
            actualExtent.height = Math.Clamp(actualExtent.height, this._supportDetails.Capabilities.minImageExtent.height, this._supportDetails.Capabilities.maxImageExtent.height);
            return actualExtent;
        }
    }
}