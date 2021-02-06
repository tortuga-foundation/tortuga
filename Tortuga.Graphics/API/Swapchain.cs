#pragma warning disable CS1591
using System;
using System.Collections.Generic;
using System.Linq;
using Tortuga.Utils;
using Vulkan;

namespace Tortuga.Graphics.API
{
    public class Swapchain
    {
        private Device _device;
        private NativeWindow _window;
        private QueueFamily _presentQueueFamily;
        private VkSurfaceCapabilitiesKHR _surfaceCapabilities;
        private List<VkSurfaceFormatKHR> _supportedSurfaceFormats;
        private List<VkPresentModeKHR> _supportedPresentModes;
        private VkSurfaceFormatKHR _surfaceFormat;
        private VkPresentModeKHR _surfacePresentMode;
        private VkExtent2D _surfaceExtent;
        private VkSwapchainKHR _handle;
        private List<Image> _images;
        private List<ImageView> _imageViews;
        private Image _depthImage;
        private ImageView _depthImageView;

        public unsafe Swapchain(Device device, NativeWindow window)
        {
            _device = device;
            _window = window;

            //get present queue
            _presentQueueFamily = GetQueueFamilyWithPresentationSupport(
                device,
                window
            );

            //get surface capabilities
            _surfaceCapabilities = GetSurfaceCapabilities(
                device,
                window
            );

            //get surface format support
            _supportedSurfaceFormats = GetSupportedSurfaceFormats(
                device,
                window
            );

            //get present mode support
            _supportedPresentModes = GetSupportedPresentModes(
                device,
                window
            );

            //choose best surface format
            #region Surface Format

            if (
                _supportedSurfaceFormats.Count == 1 &&
                _supportedSurfaceFormats[0].format == VkFormat.Undefined
            )
            {
                _surfaceFormat = new VkSurfaceFormatKHR
                {
                    colorSpace = VkColorSpaceKHR.SrgbNonlinearKHR,
                    format = VkFormat.R8g8b8Unorm
                };
            }
            else
            {
                bool choosenFormat = false;
                foreach (var format in _supportedSurfaceFormats)
                {
                    if (
                        format.format == VkFormat.R8g8b8Unorm &&
                        format.colorSpace == VkColorSpaceKHR.SrgbNonlinearKHR
                    )
                    {
                        _surfaceFormat = format;
                        choosenFormat = true;
                        break;
                    }
                }
                if (choosenFormat == false)
                    _surfaceFormat = _supportedSurfaceFormats[0];
            }

            #endregion

            #region Surface Present Mode

            _surfacePresentMode = VkPresentModeKHR.FifoKHR;
            foreach (var presentMode in _supportedPresentModes)
            {
                if (presentMode == VkPresentModeKHR.MailboxKHR)
                {
                    _surfacePresentMode = presentMode;
                    break;
                }
                else if (presentMode == VkPresentModeKHR.ImmediateKHR)
                    _surfacePresentMode = presentMode;
            }

            #endregion

            #region Surface Extent

            if (_surfaceCapabilities.currentExtent.width != uint.MaxValue)
                _surfaceExtent = _surfaceCapabilities.currentExtent;
            else
            {
                _surfaceExtent = new VkExtent2D
                {
                    width = Math.Clamp(
                        Convert.ToUInt32(window.Width),
                        _surfaceCapabilities.minImageExtent.width,
                        _surfaceCapabilities.maxImageExtent.width
                    ),
                    height = Math.Clamp(
                        Convert.ToUInt32(window.Height),
                        _surfaceCapabilities.minImageExtent.height,
                        _surfaceCapabilities.maxImageExtent.height
                    )
                };
            }

            #endregion

            #region Images Count

            var imagesCount = _surfaceCapabilities.minImageCount + 1;
            if (_surfaceCapabilities.maxImageCount > 0)
                if (imagesCount > _surfaceCapabilities.maxImageCount)
                    imagesCount = Math.Min(_surfaceCapabilities.maxImageCount, 2);

            #endregion

            var swapchainInfo = new VkSwapchainCreateInfoKHR
            {
                sType = VkStructureType.SwapchainCreateInfoKHR,
                compositeAlpha = VkCompositeAlphaFlagsKHR.OpaqueKHR,
                minImageCount = imagesCount,
                imageFormat = _surfaceFormat.format,
                imageColorSpace = _surfaceFormat.colorSpace,
                imageExtent = _surfaceExtent,
                imageArrayLayers = 1,
                imageUsage = (
                    VkImageUsageFlags.ColorAttachment |
                    VkImageUsageFlags.TransferDst
                ),
                imageSharingMode = VkSharingMode.Exclusive,
                preTransform = _surfaceCapabilities.currentTransform,
                presentMode = _surfacePresentMode,
                surface = window.Surface,
                clipped = true
            };

            VkSwapchainKHR swapchain;
            if (VulkanNative.vkCreateSwapchainKHR(
                device.Handle,
                &swapchainInfo,
                null,
                &swapchain
            ) != VkResult.Success)
                throw new Exception("failed to create swapchain");
            _handle = swapchain;

            SetupSwapchainImages();
        }

        private unsafe void SetupSwapchainImages()
        {
            #region get swapchain images

            uint imagesCount = 0;
            if (VulkanNative.vkGetSwapchainImagesKHR(
                _device.Handle,
                _handle,
                &imagesCount,
                null
            ) != VkResult.Success)
                throw new Exception("failed to get swapchain images");
            var swapchainNativeImages = new NativeList<VkImage>(imagesCount);
            swapchainNativeImages.Count = imagesCount;
            if (VulkanNative.vkGetSwapchainImagesKHR(
                _device.Handle,
                _handle,
                &imagesCount,
                (VkImage*)swapchainNativeImages.Data.ToPointer()
            ) != VkResult.Success)
                throw new Exception("failed to get swapchain images");

            #endregion

            #region setup images and image views

            _images = swapchainNativeImages.Select(
                image => Image.CreateImageObject(
                    _device,
                    _surfaceExtent.width,
                    _surfaceExtent.height,
                    image,
                    _surfaceFormat.format,
                    VkImageLayout.Undefined,
                    null
                )
            ).ToList();
            _imageViews = new List<ImageView>();
            foreach (var image in _images)
                _imageViews.Add(new ImageView(image, VkImageAspectFlags.Color));

            #endregion

            #region setup depth image and depth image view

            _depthImage = new Image(
                _device,
                _surfaceExtent.width,
                _surfaceExtent.height,
                _device.FindDepthFormat,
                VkImageUsageFlags.DepthStencilAttachment
            );
            _depthImageView = new ImageView(
                _depthImage,
                VkImageAspectFlags.Depth
            );

            #endregion

            #region transfer images to correct layout

            var module = Engine.Instance.GetModule<GraphicsModule>();
            var graphicsQueue = _device.GraphicsQueueFamily;
            var fence = new Fence(_device);
            var command = module.CommandBufferService.GetNewCommand(
                QueueFamilyType.Graphics,
                CommandType.Primary
            );
            command.Begin(VkCommandBufferUsageFlags.OneTimeSubmit);
            //transfer images to correct layout
            foreach (var image in _images)
            {
                command.TransferImageLayout(
                    image,
                    VkImageLayout.PresentSrcKHR
                );
            }
            //transfer depth image to correct layout
            command.TransferImageLayout(
                _depthImage,
                VkImageLayout.DepthStencilAttachmentOptimal
            );
            command.End();
            module.CommandBufferService.Submit(
                command,
                null, null,
                fence
            );
            fence.Wait();

            #endregion
        }

        private unsafe QueueFamily GetQueueFamilyWithPresentationSupport(
            Device device,
            NativeWindow window
        )
        {
            var queueFamiliesSupportingPresentation = new List<bool>();
            foreach (var queueFamily in device.QueueFamilies)
            {
                var isSupported = VkBool32.False;
                if (VulkanNative.vkGetPhysicalDeviceSurfaceSupportKHR(
                    _device.PhysicalDevice,
                    0,
                    window.Surface,
                    &isSupported
                ) != VkResult.Success)
                    throw new Exception("failed to check if device supports presentation");
                queueFamiliesSupportingPresentation.Add(isSupported);
            }

            var familySupportingPresentation = queueFamiliesSupportingPresentation.FindIndex(
                0, queueFamiliesSupportingPresentation.Count,
                q => q
            );
            if (familySupportingPresentation == -1)
                throw new NotSupportedException("device does not support presentation");
            return device.QueueFamilies[familySupportingPresentation];
        }

        private unsafe List<VkSurfaceFormatKHR> GetSupportedSurfaceFormats(
            Device device,
            NativeWindow window
        )
        {
            uint surfaceSupportedFormatsCount;
            if (VulkanNative.vkGetPhysicalDeviceSurfaceFormatsKHR(
                device.PhysicalDevice,
                window.Surface,
                &surfaceSupportedFormatsCount,
                null
            ) != VkResult.Success)
                throw new Exception("failed to get device support formats");
            var surfaceSupportFormats = new NativeList<VkSurfaceFormatKHR>(surfaceSupportedFormatsCount);
            surfaceSupportFormats.Count = surfaceSupportedFormatsCount;
            if (VulkanNative.vkGetPhysicalDeviceSurfaceFormatsKHR(
                device.PhysicalDevice,
                window.Surface,
                &surfaceSupportedFormatsCount,
                (VkSurfaceFormatKHR*)surfaceSupportFormats.Data.ToPointer()
            ) != VkResult.Success)
                throw new Exception("failed to get device support formats");

            return surfaceSupportFormats.ToList();
        }

        private unsafe VkSurfaceCapabilitiesKHR GetSurfaceCapabilities(
            Device device,
            NativeWindow window
        )
        {
            VkSurfaceCapabilitiesKHR surfaceCapabilities;
            if (VulkanNative.vkGetPhysicalDeviceSurfaceCapabilitiesKHR(
                device.PhysicalDevice,
                window.Surface,
                out surfaceCapabilities
            ) != VkResult.Success)
                throw new Exception("failed to get device surface presentation");
            return surfaceCapabilities;
        }

        private unsafe List<VkPresentModeKHR> GetSupportedPresentModes(
            Device device,
            NativeWindow window
        )
        {
            uint presentModeCount;
            if (VulkanNative.vkGetPhysicalDeviceSurfacePresentModesKHR(
                device.PhysicalDevice,
                window.Surface,
                &presentModeCount,
                null
            ) != VkResult.Success)
                throw new Exception("failed to get device supported present modes");
            var supporedPresentModes = new NativeList<VkPresentModeKHR>(presentModeCount);
            supporedPresentModes.Count = presentModeCount;
            if (VulkanNative.vkGetPhysicalDeviceSurfacePresentModesKHR(
                device.PhysicalDevice,
                window.Surface,
                &presentModeCount,
                (VkPresentModeKHR*)supporedPresentModes.Data.ToPointer()
            ) != VkResult.Success)
                throw new Exception("failed to get device supported present modes");
            return supporedPresentModes.ToList();
        }
    }
}