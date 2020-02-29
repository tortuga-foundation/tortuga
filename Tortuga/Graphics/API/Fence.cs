using System;
using Vulkan;
using static Vulkan.VulkanNative;

namespace Tortuga.Graphics.API
{
    internal class Fence
    {
        public VkFence Handle => _fence;

        private VkFence _fence;

        public unsafe Fence(bool signaled = false)
        {
            var fenceInfo = VkFenceCreateInfo.New();
            if (signaled)
                fenceInfo.flags = VkFenceCreateFlags.Signaled;

            VkFence fence;
            if (vkCreateFence(
                Engine.Instance.MainDevice.LogicalDevice,
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
                Engine.Instance.MainDevice.LogicalDevice,
                _fence,
                null
            );
        }

        public unsafe bool IsSignaled()
        {
            return (
                vkGetFenceStatus(
                    Engine.Instance.MainDevice.LogicalDevice,
                    _fence
                ) == VkResult.Success
            );
        }

        public unsafe void Wait()
        {
            VkFence fence = _fence;
            if (vkWaitForFences(
                Engine.Instance.MainDevice.LogicalDevice,
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
                Engine.Instance.MainDevice.LogicalDevice,
                1,
                &fence
            ) != VkResult.Success)
                throw new Exception("failed to reset fence");
        }
    }
}