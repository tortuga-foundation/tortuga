using System;
using System.Collections.Generic;
using Vulkan;
using Tortuga.Utils;
using static Vulkan.VulkanNative;

namespace Tortuga.Graphics.API
{
    internal class Swapchain
    {
        public Device.QueueFamily DevicePresentQueueFamily => _presentQueueFamily;
        public VkSwapchainKHR Handle => _swapchain;
        public NativeList<VkImage> Images => _images;
        public VkFormat ImagesFormat => _format.format;
        public VkExtent2D Extent => _extent;
        private Device DeviceUsed => _device;

        private Device.QueueFamily _presentQueueFamily;
        private bool[] _queuesSupportingPresentation;
        private VkSurfaceCapabilitiesKHR _surfaceCapabilities;
        private NativeList<VkSurfaceFormatKHR> _surfaceSupportedFormats;
        private NativeList<VkPresentModeKHR> _surfaceSupportedPresentModes;
        private VkSurfaceFormatKHR _format;
        private VkPresentModeKHR _presentMode;
        private VkExtent2D _extent;
        private VkSwapchainKHR _swapchain;
        private uint _imagesCount;
        private NativeList<VkImage> _images;
        private List<ImageView> _imageViews;
        private Image _depthImage;
        private ImageView _depthImageView;
        private Window _window;
        private Device _device;

        public unsafe Swapchain(Device device, Window window)
        {
            _device = device;
            _window = window;
            var windowSize = window.Size;
            //get device presentation queue
            _queuesSupportingPresentation = new bool[_device.QueueFamilyProperties.Count];
            for (int i = 0; i < _device.QueueFamilyProperties.Count; i++)
            {
                VkBool32 isSupported = false;
                if (vkGetPhysicalDeviceSurfaceSupportKHR(
                    _device.PhysicalDevice,
                    0,
                    window.Surface,
                    &isSupported) != VkResult.Success
                )
                    throw new Exception("failed to check if device supports presentation");
                _queuesSupportingPresentation[i] = isSupported;
            }

            var familySupportingPresentation = Array.FindIndex(
                _queuesSupportingPresentation,
                0,
                _queuesSupportingPresentation.Length,
                b => b
            );
            if (familySupportingPresentation == -1)
                throw new NotSupportedException("device does not support presentation");
            _presentQueueFamily = _device.QueueFamilyProperties[familySupportingPresentation];

            //get surface capabilities
            VkSurfaceCapabilitiesKHR surfaceCapabilities;
            if (vkGetPhysicalDeviceSurfaceCapabilitiesKHR(
                _device.PhysicalDevice,
                window.Surface,
                out surfaceCapabilities) != VkResult.Success
            )
                throw new Exception("failed to get device surface capabilities");
            _surfaceCapabilities = surfaceCapabilities;

            //get surface format support
            uint surfaceFormatCount;
            if (vkGetPhysicalDeviceSurfaceFormatsKHR(
                _device.PhysicalDevice,
                window.Surface,
                &surfaceFormatCount,
                null) != VkResult.Success
            )
                throw new Exception("failed to get device supported formats");
            var surfaceSupportedFormats = new NativeList<VkSurfaceFormatKHR>(surfaceFormatCount);
            surfaceSupportedFormats.Count = surfaceFormatCount;
            if (vkGetPhysicalDeviceSurfaceFormatsKHR(
                _device.PhysicalDevice,
                window.Surface,
                &surfaceFormatCount,
                (VkSurfaceFormatKHR*)surfaceSupportedFormats.Data.ToPointer()) != VkResult.Success
            )
                throw new Exception("failed to get device supported formats");
            _surfaceSupportedFormats = surfaceSupportedFormats;

            //get present mode support
            uint presentModeCount;
            if (vkGetPhysicalDeviceSurfacePresentModesKHR(
                _device.PhysicalDevice,
                window.Surface,
                &presentModeCount,
                null
            ) != VkResult.Success)
                throw new Exception("failed to get device supported present modes");
            var surfaceSupportedPresentModes = new NativeList<VkPresentModeKHR>(presentModeCount);
            surfaceSupportedPresentModes.Count = presentModeCount;
            if (vkGetPhysicalDeviceSurfacePresentModesKHR(
                _device.PhysicalDevice,
                window.Surface,
                &presentModeCount,
                (VkPresentModeKHR*)surfaceSupportedPresentModes.Data.ToPointer()
            ) != VkResult.Success)
                throw new Exception("failed to get device supported present modes");
            _surfaceSupportedPresentModes = surfaceSupportedPresentModes;

            //choose best surface format
            if (surfaceSupportedFormats.Count == 1 && surfaceSupportedFormats[0].format == VkFormat.Undefined)
                _format = new VkSurfaceFormatKHR
                {
                    colorSpace = VkColorSpaceKHR.SrgbNonlinearKHR,
                    format = VkFormat.R8g8b8Unorm
                };
            else
            {
                bool choosenFormat = false;
                foreach (var format in surfaceSupportedFormats)
                {
                    if (format.format == VkFormat.R8g8b8Unorm &&
                        format.colorSpace == VkColorSpaceKHR.SrgbNonlinearKHR)
                    {
                        _format = format;
                        choosenFormat = true;
                        break;
                    }
                }

                if (!choosenFormat)
                    _format = surfaceSupportedFormats[0];
            }

            //choose best present mode
            _presentMode = VkPresentModeKHR.FifoKHR;
            foreach (var presentMode in surfaceSupportedPresentModes)
            {
                if (presentMode == VkPresentModeKHR.MailboxKHR)
                {
                    _presentMode = presentMode;
                    break;
                }
                else if (presentMode == VkPresentModeKHR.ImmediateKHR)
                    _presentMode = presentMode;
            }

            //choose extent
            if (surfaceCapabilities.currentExtent.width != uint.MaxValue)
                _extent = surfaceCapabilities.currentExtent;
            else
            {
                VkExtent2D actualExtent = new VkExtent2D
                {
                    width = Convert.ToUInt32(windowSize.X),
                    height = Convert.ToUInt32(windowSize.Y)
                };
                actualExtent.width = Clamp(
                    actualExtent.width,
                    surfaceCapabilities.minImageExtent.width,
                    surfaceCapabilities.maxImageExtent.width
                );
                actualExtent.height = Clamp(
                    actualExtent.height,
                    surfaceCapabilities.minImageExtent.height,
                    surfaceCapabilities.maxImageExtent.height
                );
                _extent = actualExtent;
            }

            _imagesCount = surfaceCapabilities.minImageCount + 1;
            if (surfaceCapabilities.maxImageCount > 0)
                if (_imagesCount > surfaceCapabilities.maxImageCount)
                    _imagesCount = Math.Min(surfaceCapabilities.maxImageCount, 2);

            var swapchainInfo = VkSwapchainCreateInfoKHR.New();
            swapchainInfo.compositeAlpha = VkCompositeAlphaFlagsKHR.OpaqueKHR;
            swapchainInfo.surface = window.Surface;
            swapchainInfo.minImageCount = _imagesCount;
            swapchainInfo.imageFormat = _format.format;
            swapchainInfo.imageColorSpace = _format.colorSpace;
            swapchainInfo.imageExtent = _extent;
            swapchainInfo.imageArrayLayers = 1;
            swapchainInfo.imageUsage = VkImageUsageFlags.ColorAttachment | VkImageUsageFlags.TransferDst;
            swapchainInfo.imageSharingMode = VkSharingMode.Exclusive;
            swapchainInfo.preTransform = surfaceCapabilities.currentTransform;
            swapchainInfo.presentMode = _presentMode;
            swapchainInfo.clipped = true;

            VkSwapchainKHR swapchain;
            if (vkCreateSwapchainKHR(_device.LogicalDevice, &swapchainInfo, null, &swapchain) != VkResult.Success)
                throw new Exception("failed to create swapchain");
            _swapchain = swapchain;

            SetupSwapchainImages(
                Convert.ToInt32(windowSize.X), 
                Convert.ToInt32(windowSize.Y)
            );
        }

        unsafe ~Swapchain()
        {
            vkDestroySwapchainKHR(_device.LogicalDevice, _swapchain, null);
        }

        private unsafe void SetupSwapchainImages(int width, int height)
        {
            //get swapchain images
            uint imagesCount = 0;
            if (vkGetSwapchainImagesKHR(_device.LogicalDevice, _swapchain, &imagesCount, null) != VkResult.Success)
                throw new Exception("failed to get swapchain images");
            var images = new NativeList<VkImage>(imagesCount);
            images.Count = imagesCount;
            if (vkGetSwapchainImagesKHR(_device.LogicalDevice, _swapchain, &imagesCount, (VkImage*)images.Data.ToPointer()) != VkResult.Success)
                throw new Exception("failed to get swapchain images");
            _images = new NativeList<VkImage>();
            foreach (var image in images)
                _images.Add(image);

            //get swapchain image views
            _imageViews = new List<ImageView>(Convert.ToInt32(_images.Count));
            for (int i = 0; i < _imageViews.Count; i++)
                _imageViews[i] = new ImageView(_device, _images[i], _format.format, VkImageAspectFlags.Color);

            //get swapchaing depth image & depth image view
            var depthFormat = _device.FindDepthFormat;
            _depthImage = new Image(_device, _extent.width, _extent.height, depthFormat, VkImageUsageFlags.DepthStencilAttachment);
            _depthImageView = new ImageView(_depthImage, VkImageAspectFlags.Depth);

            //initialize depth image
            var creationFence = new Fence(_device);
            var commandPool = new CommandPool(_device, _device.GraphicsQueueFamily);
            var command = commandPool.AllocateCommands()[0];
            command.Begin(VkCommandBufferUsageFlags.OneTimeSubmit);
            command.TransferImageLayout(_depthImage, VkImageLayout.Undefined, VkImageLayout.DepthStencilAttachmentOptimal);
            foreach (var image in _images)
                command.TransferImageLayout(image, _format.format, VkImageLayout.Undefined, VkImageLayout.PresentSrcKHR);
            command.End();
            CommandPool.Command.Submit(
                _device.GraphicsQueueFamily.Queues[0],
                new CommandPool.Command[]{
                command
                },
                new Semaphore[0],
                new Semaphore[0],
                creationFence
            );
            creationFence.Wait();
        }

        public T Clamp<T>(T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }

        public unsafe void Resize()
        {
            var windowSize = _window.Size;
            //get surface capabilities
            VkSurfaceCapabilitiesKHR surfaceCapabilities;
            if (vkGetPhysicalDeviceSurfaceCapabilitiesKHR(
                _device.PhysicalDevice,
                _window.Surface,
                out surfaceCapabilities) != VkResult.Success
            )
                throw new Exception("failed to get device surface capabilities");
            _surfaceCapabilities = surfaceCapabilities;

            //choose extent
            if (_surfaceCapabilities.currentExtent.width != uint.MaxValue)
                _extent = _surfaceCapabilities.currentExtent;
            else
            {
                VkExtent2D actualExtent = new VkExtent2D
                {
                    width = Convert.ToUInt32(windowSize.X),
                    height = Convert.ToUInt32(windowSize.Y)
                };
                actualExtent.width = Clamp(
                    actualExtent.width,
                    _surfaceCapabilities.minImageExtent.width,
                    _surfaceCapabilities.maxImageExtent.width
                );
                actualExtent.height = Clamp(
                    actualExtent.height,
                    _surfaceCapabilities.minImageExtent.height,
                    _surfaceCapabilities.maxImageExtent.height
                );
                _extent = actualExtent;
            }

            var swapchainInfo = VkSwapchainCreateInfoKHR.New();
            swapchainInfo.compositeAlpha = VkCompositeAlphaFlagsKHR.OpaqueKHR;
            swapchainInfo.surface = _window.Surface;
            swapchainInfo.minImageCount = _imagesCount;
            swapchainInfo.imageFormat = _format.format;
            swapchainInfo.imageColorSpace = _format.colorSpace;
            swapchainInfo.imageExtent = _extent;
            swapchainInfo.imageArrayLayers = 1;
            swapchainInfo.imageUsage = VkImageUsageFlags.ColorAttachment | VkImageUsageFlags.TransferDst;
            swapchainInfo.imageSharingMode = VkSharingMode.Exclusive;
            swapchainInfo.preTransform = _surfaceCapabilities.currentTransform;
            swapchainInfo.presentMode = _presentMode;
            swapchainInfo.clipped = true;
            swapchainInfo.oldSwapchain = _swapchain;

            VkSwapchainKHR swapchain;
            if (vkCreateSwapchainKHR(_device.LogicalDevice, &swapchainInfo, null, &swapchain) != VkResult.Success)
                throw new Exception("failed to create swapchain");
            _swapchain = swapchain;

            SetupSwapchainImages(
                Convert.ToInt32(windowSize.X), 
                Convert.ToInt32(windowSize.Y)
            );
        }
    }
}