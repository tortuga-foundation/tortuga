using System;
using Vulkan;
using static Vulkan.VulkanNative;

namespace Tortuga.Graphics.API
{
    internal class Fence
    {
        public VkFence Handle => _fence;
        public Device DeviceUsed => _device;

        private VkFence _fence;
        private Device _device;

        public unsafe Fence(Device device, bool signaled = false)
        {
            _device = device;
            var fenceInfo = VkFenceCreateInfo.New();
            if (signaled)
                fenceInfo.flags = VkFenceCreateFlags.Signaled;

            VkFence fence;
            if (vkCreateFence(
                _device.LogicalDevice,
                &fenceInfo,
                null,
                &fence
            ) != VkResult.Success)
                throw new Exception("failed to create semaphore");
            _fence = fence;
        }

        unsafe ~Fence()
        {
            vkDestroyFence(
                _device.LogicalDevice,
                _fence,
                null
            );
        }

        public unsafe bool IsSignaled()
        {
            return (
                vkGetFenceStatus(
                    _device.LogicalDevice,
                    _fence
                ) == VkResult.Success
            );
        }

        public unsafe void Wait()
        {
            VkFence fence = _fence;
            if (vkWaitForFences(
                _device.LogicalDevice,
                1,
                &fence,
                true,
                ulong.MaxValue
            ) != VkResult.Success)
                throw new Exception("failed to wait on a fence");
        }

        public unsafe void Reset()
        {
            VkFence fence = _fence;
            if (vkResetFences(
                _device.LogicalDevice,
                1,
                &fence
            ) != VkResult.Success)
                throw new Exception("failed to reset fence");
        }
    }
}