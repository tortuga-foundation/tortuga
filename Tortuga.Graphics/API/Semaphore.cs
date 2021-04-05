#pragma warning disable CS1591
using System;
using Vulkan;

namespace Tortuga.Graphics.API
{
    public class Semaphore
    {
        public Device Device => _device;
        public VkSemaphore Handle => _handle;

        private Device _device;
        private VkSemaphore _handle;

        public unsafe Semaphore(Device device)
        {
            _device = device;
            var createInfo = new VkSemaphoreCreateInfo
            {
                sType = VkStructureType.SemaphoreCreateInfo
            };

            VkSemaphore semaphore;
            if (VulkanNative.vkCreateSemaphore(
                _device.Handle,
                &createInfo,
                null,
                &semaphore
            ) != VkResult.Success)
                throw new Exception("failed to create semaphore");
            _handle = semaphore;
        }
        unsafe ~Semaphore()
        {
            if (_handle != VkSemaphore.Null)
            {
                VulkanNative.vkDestroySemaphore(
                    _device.Handle,
                    _handle,
                    null
                );
                _handle = VkSemaphore.Null;
            }
        }
    }
}