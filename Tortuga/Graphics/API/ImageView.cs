using System;
using Vulkan;
using static Vulkan.VulkanNative;

namespace Tortuga.Graphics.API
{
    internal class ImageView
    {
        public VkImageView Handle => _imageViewHandle;

        private VkImageView _imageViewHandle;

        public unsafe ImageView(VkImage image, VkFormat format, VkImageAspectFlags aspectFlags, uint mipLevel = 1)
        {
            var imageViewInfo = VkImageViewCreateInfo.New();
            imageViewInfo.image = image;
            imageViewInfo.viewType = VkImageViewType.Image2D;
            imageViewInfo.format = format;
            //components
            imageViewInfo.components = new VkComponentMapping
            {
                r = VkComponentSwizzle.Identity,
                g = VkComponentSwizzle.Identity,
                b = VkComponentSwizzle.Identity,
                a = VkComponentSwizzle.Identity
            };
            //subresource
            imageViewInfo.subresourceRange = new VkImageSubresourceRange
            {
                aspectMask = aspectFlags,
                baseMipLevel = 0,
                levelCount = mipLevel,
                baseArrayLayer = 0,
                layerCount = 1
            };

            VkImageView imageView;
            if (vkCreateImageView(Engine.Instance.MainDevice.LogicalDevice, &imageViewInfo, null, &imageView) != VkResult.Success)
                throw new Exception("failed to create image view");
            _imageViewHandle = imageView;
        }

        public unsafe ImageView(Image image, VkImageAspectFlags aspectFlags) : this(image.ImageHandle, image.Format, aspectFlags, image.MipLevel)
        {
        }

        unsafe ~ImageView()
        {
            vkDestroyImageView(Engine.Instance.MainDevice.LogicalDevice, _imageViewHandle, null);
        }
    }
}