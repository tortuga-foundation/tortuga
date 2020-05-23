using System;
using Vulkan;
using static Vulkan.VulkanNative;

namespace Tortuga.Graphics.API
{
    internal class Semaphore
    {
        public VkSemaphore Handle => _semaphore;
        public Device DeviceUsed => _device;

        private VkSemaphore _semaphore;
        private Device _device;

        public unsafe Semaphore(Device device)
        {
            _device = device;
            var semaphoreInfo = VkSemaphoreCreateInfo.New();

            VkSemaphore semaphore;
            if (vkCreateSemaphore(
                _device.LogicalDevice,
                &semaphoreInfo,
                null,
                &semaphore
            ) != VkResult.Success)
                throw new Exception("failed to create semaphore");
            _semaphore = semaphore;
        }

        unsafe ~Semaphore()
        {
            vkDestroySemaphore(
                _device.LogicalDevice,
                _semaphore,
                null
            );
        }
    }
}