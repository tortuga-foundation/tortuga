using System;
using Vulkan;
using static Vulkan.VulkanNative;

namespace Tortuga.Graphics.API
{
    internal class Image
    {
        public VkImage ImageHandle => _imageHandle;
        public VkDeviceMemory Memory => _deviceMemory;
        public VkFormat Format => _format;
        public uint MipLevel => _mipLevel;
        public Device DeviceUsed => _device;

        private Device _device;
        private VkImage _imageHandle;
        private VkDeviceMemory _deviceMemory;
        private VkFormat _format;
        private uint _mipLevel;

        public unsafe Image(Device device, uint width, uint height, VkFormat format, VkImageUsageFlags usageFlags, uint mipMapLevel = 1)
        {
            this._device = device;
            this._format = format;
            this._mipLevel = mipMapLevel;

            var imageInfo = VkImageCreateInfo.New();
            imageInfo.imageType = VkImageType.Image2D;
            imageInfo.format = format;
            imageInfo.extent = new VkExtent3D
            {
                width = width,
                height = height,
                depth = 1
            };
            imageInfo.mipLevels = mipMapLevel;
            imageInfo.arrayLayers = 1;
            imageInfo.samples = VkSampleCountFlags.Count1;
            imageInfo.tiling = VkImageTiling.Optimal;
            imageInfo.usage = usageFlags;
            imageInfo.sharingMode = VkSharingMode.Exclusive;
            imageInfo.initialLayout = VkImageLayout.Undefined;

            VkImage image;
            if (vkCreateImage(device.LogicalDevice, &imageInfo, null, &image) != VkResult.Success)
                throw new Exception("failed to create image");
            _imageHandle = image;

            //memory
            VkMemoryRequirements memoryRequirements;
            vkGetImageMemoryRequirements(device.LogicalDevice, _imageHandle, out memoryRequirements);

            var allocateInfo = VkMemoryAllocateInfo.New();
            allocateInfo.memoryTypeIndex = device.FindMemoryType(memoryRequirements.memoryTypeBits, VkMemoryPropertyFlags.DeviceLocal);
            allocateInfo.allocationSize = memoryRequirements.size;

            VkDeviceMemory deviceMemory;
            if (vkAllocateMemory(device.LogicalDevice, &allocateInfo, null, &deviceMemory) != VkResult.Success)
                throw new Exception("failed to allocate image memory on device");
            _deviceMemory = deviceMemory;
            if (vkBindImageMemory(device.LogicalDevice, _imageHandle, _deviceMemory, 0) != VkResult.Success)
                throw new Exception("failed to bind image to device memory");
        }

        unsafe ~Image()
        {
            vkDestroyImage(_device.LogicalDevice, _imageHandle, null);
            vkFreeMemory(_device.LogicalDevice, _deviceMemory, null);
        }

        public bool HasStencilComponent => _format == VkFormat.D32SfloatS8Uint || Format == VkFormat.D24UnormS8Uint;
    }
}