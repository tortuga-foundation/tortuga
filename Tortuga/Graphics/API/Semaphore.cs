using System;
using Vulkan;
using static Vulkan.VulkanNative;

namespace Tortuga.Graphics.API
{
    internal class Semaphore
    {
        public VkSemaphore Handle => _semaphore;

        private VkSemaphore _semaphore;

        public unsafe Semaphore()
        {
            var semaphoreInfo = VkSemaphoreCreateInfo.New();

            VkSemaphore semaphore;
            if (vkCreateSemaphore(
                Engine.Instance.MainDevice.LogicalDevice,
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
                Engine.Instance.MainDevice.LogicalDevice,
                _semaphore,
                null
            );
        }
    }
}