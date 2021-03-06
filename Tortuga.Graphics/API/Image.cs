#pragma warning disable CS1591
using System;
using Tortuga.Utils;
using Vulkan;

namespace Tortuga.Graphics.API
{
    public class Image
    {
        public Device Device => _device;
        public uint Width => _width;
        public uint Height => _height;
        public uint MipLevel => _mipLevel;
        public VkFormat Format => _format;
        public VkImageLayout[] Layout
        {
            get => _layout;
            set => _layout = value;
        }
        public VkImage Handle => _handle;
        public VkDeviceMemory MemoryHandle => _memoryHandle;

        private Device _device;
        private uint _width;
        private uint _height;
        private VkFormat _format;
        private uint _mipLevel;
        private VkImageLayout[] _layout;
        private VkImage _handle;
        private VkDeviceMemory _memoryHandle;

        private Image() { }
        /// <summary>
        /// create a vulkan image
        /// </summary>
        /// <param name="device">the device to use, where is the image stored</param>
        /// <param name="width">width of the image</param>
        /// <param name="height">height of the image</param>
        /// <param name="format">format of the image</param>
        /// <param name="usageFlags">image usage flags, how the image will be used</param>
        /// <param name="mipLevel">mip map levels of the image, if 0 then it will be auto-generated</param>
        public unsafe Image(
            Device device,
            uint width, uint height,
            VkFormat format,
            VkImageUsageFlags usageFlags,
            uint mipLevel = 0
        )
        {
            // auto generate mip map levels
            if (mipLevel == 0)
            {
                mipLevel = Convert.ToUInt32(
                    MathF.Floor(
                        MathF.Log2(
                            MathF.Max(width, height)
                        )
                    ) + 1
                );
            }

            _device = device;
            _width = width;
            _height = height;
            _format = format;
            _mipLevel = mipLevel;
            _layout = new VkImageLayout[mipLevel];
            Array.Fill(_layout, VkImageLayout.Undefined);

            var queueFamilyIndices = new NativeList<uint>();
            foreach (var queueFamily in device.QueueFamilies)
                queueFamilyIndices.Add(queueFamily.Index);

            var imageInfo = new VkImageCreateInfo
            {
                sType = VkStructureType.ImageCreateInfo,
                imageType = VkImageType.Image2D,
                format = format,
                extent = new VkExtent3D
                {
                    width = width,
                    height = height,
                    depth = 1
                },
                mipLevels = mipLevel,
                arrayLayers = 1,
                samples = VkSampleCountFlags.Count1,
                tiling = VkImageTiling.Optimal,
                usage = usageFlags,
                sharingMode = VkSharingMode.Concurrent,
                initialLayout = VkImageLayout.Undefined,
                queueFamilyIndexCount = queueFamilyIndices.Count,
                pQueueFamilyIndices = (uint*)queueFamilyIndices.Data.ToPointer()
            };

            VkImage image;
            if (VulkanNative.vkCreateImage(
                device.Handle,
                &imageInfo,
                null,
                &image
            ) != VkResult.Success)
                throw new Exception("failed to create vulkan image");
            _handle = image;

            //memory 
            VkMemoryRequirements memoryRequirements;
            VulkanNative.vkGetImageMemoryRequirements(
                device.Handle,
                image,
                out memoryRequirements
            );

            var allocateInfo = new VkMemoryAllocateInfo
            {
                sType = VkStructureType.MemoryAllocateInfo,
                memoryTypeIndex = device.FindMemoryType(
                    memoryRequirements.memoryTypeBits,
                    VkMemoryPropertyFlags.DeviceLocal
                ),
                allocationSize = memoryRequirements.size
            };

            VkDeviceMemory deviceMemory;
            if (VulkanNative.vkAllocateMemory(
                device.Handle,
                &allocateInfo,
                null,
                &deviceMemory
            ) != VkResult.Success)
                throw new Exception("failed to allocate device memory");
            _memoryHandle = deviceMemory;

            //bind memory with image
            if (VulkanNative.vkBindImageMemory(
                device.Handle,
                image,
                deviceMemory,
                0
            ) != VkResult.Success)
                throw new Exception("failed to bind image to device memory");
        }

        unsafe ~Image()
        {
            if (_device == null)
                return;

            if (_handle != VkImage.Null)
            {
                VulkanNative.vkDestroyImage(_device.Handle, _handle, null);
                _handle = VkImage.Null;
            }
            if (_memoryHandle != VkDeviceMemory.Null)
            {
                VulkanNative.vkFreeMemory(_device.Handle, _memoryHandle, null);
                _memoryHandle = VkDeviceMemory.Null;
            }
        }

        public static Image CreateImageObject(
            Device device,
            uint width, uint height,
            VkImage image,
            VkFormat format,
            VkImageLayout layout,
            VkDeviceMemory? memory,
            uint mipLevel = 1
        ) => new Image
        {
            _width = width,
            _height = height,
            _handle = image,
            _format = format,
            _memoryHandle = (
                memory != null ?
                (VkDeviceMemory)memory :
                VkDeviceMemory.Null
            ),
            _layout = new VkImageLayout[] { VkImageLayout.Undefined },
            _mipLevel = mipLevel,
            _device = device
        };

        public bool HasStencilComponent => HasStencil(_format);
        public static bool HasStencil(VkFormat format) => (
            GraphicsApiConstants.DEPTH_FORMAT_CANDIDATES.IndexOf(
                format
            ) != -1
        );
    }
}