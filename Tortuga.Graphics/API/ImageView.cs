#pragma warning disable CS1591
using System;
using Vulkan;

namespace Tortuga.Graphics.API
{
    public class ImageView
    {
        public Device Device => _device;
        public VkImageView Handle => _handle;
        public Image Image => _image;

        private Device _device;
        private VkImageView _handle;
        private Image _image;

        public unsafe ImageView(
            Image image,
            VkImageAspectFlags aspectMask
        )
        {
            _device = image.Device;
            _image = image;
            var imageViewInfo = new VkImageViewCreateInfo
            {
                sType = VkStructureType.ImageViewCreateInfo,
                image = image.Handle,
                viewType = VkImageViewType.Image2D,
                format = image.Format,
                components = new VkComponentMapping
                {
                    r = VkComponentSwizzle.Identity,
                    g = VkComponentSwizzle.Identity,
                    b = VkComponentSwizzle.Identity,
                    a = VkComponentSwizzle.Identity
                },
                subresourceRange = new VkImageSubresourceRange
                {
                    aspectMask = aspectMask,
                    baseMipLevel = 0,
                    levelCount = image.MipLevel,
                    baseArrayLayer = 0,
                    layerCount = 1
                }
            };

            VkImageView imageView;
            if (VulkanNative.vkCreateImageView(
                _device.Handle,
                &imageViewInfo,
                null,
                &imageView
            ) != VkResult.Success)
                throw new Exception("failed to create image view");
            _handle = imageView;
        }

        unsafe ~ImageView()
        {
            if (_handle != VkImageView.Null)
            {
                VulkanNative.vkDestroyImageView(
                    _device.Handle,
                    _handle,
                    null
                );
                _handle = VkImageView.Null;
            }
        }
    }
}