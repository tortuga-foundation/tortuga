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
        public int Width => _width;
        public int Height => _height;
        internal uint ReservedDescriptorSet;

        private VkImage _imageHandle;
        private VkDeviceMemory _deviceMemory;
        private VkFormat _format;
        private uint _mipLevel;
        private int _width;
        private int _height;

        private Image() { }
        public unsafe Image(uint width, uint height, VkFormat format, VkImageUsageFlags usageFlags, uint mipMapLevel = 1)
        {
            this._width = Convert.ToInt32(width);
            this._height = Convert.ToInt32(height);
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
            if (vkCreateImage(Engine.Instance.MainDevice.LogicalDevice, &imageInfo, null, &image) != VkResult.Success)
                throw new Exception("failed to create image");
            _imageHandle = image;

            //memory
            VkMemoryRequirements memoryRequirements;
            vkGetImageMemoryRequirements(Engine.Instance.MainDevice.LogicalDevice, _imageHandle, out memoryRequirements);

            var allocateInfo = VkMemoryAllocateInfo.New();
            allocateInfo.memoryTypeIndex = Engine.Instance.MainDevice.FindMemoryType(memoryRequirements.memoryTypeBits, VkMemoryPropertyFlags.DeviceLocal);
            allocateInfo.allocationSize = memoryRequirements.size;

            VkDeviceMemory deviceMemory;
            if (vkAllocateMemory(Engine.Instance.MainDevice.LogicalDevice, &allocateInfo, null, &deviceMemory) != VkResult.Success)
                throw new Exception("failed to allocate image memory on device");
            _deviceMemory = deviceMemory;
            if (vkBindImageMemory(Engine.Instance.MainDevice.LogicalDevice, _imageHandle, _deviceMemory, 0) != VkResult.Success)
                throw new Exception("failed to bind image to device memory");
        }
        unsafe ~Image()
        {
            vkDestroyImage(Engine.Instance.MainDevice.LogicalDevice, _imageHandle, null);
            vkFreeMemory(Engine.Instance.MainDevice.LogicalDevice, _deviceMemory, null);
        }
        public static Image GetImageObject(VkImage image, VkFormat format, VkDeviceMemory memory, int width, int height, uint mipLevel = 1)
        {
            return new Image()
            {
                _width = width,
                _height = height,
                _deviceMemory = memory,
                _format = format,
                _imageHandle = image,
                _mipLevel = mipLevel
            };
        }

        public bool HasStencilComponent => _format == VkFormat.D32SfloatS8Uint || _format == VkFormat.D24UnormS8Uint;
        public static bool HasStencil(VkFormat format) => format == VkFormat.D32SfloatS8Uint || format == VkFormat.D24UnormS8Uint;
    }
}