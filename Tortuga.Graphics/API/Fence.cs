#pragma warning disable CS1591
using System;
using System.Threading.Tasks;
using Vulkan;

namespace Tortuga.Graphics.API
{
    public class Fence
    {
        public VkFence Handle => _handle;
        private VkFence _handle;
        private Device _device;

        public unsafe Fence(Device device, bool isSignaled = false)
        {
            _device = device;
            var createInfo = new VkFenceCreateInfo
            {
                sType = VkStructureType.FenceCreateInfo,
                flags = (
                    isSignaled ?
                    VkFenceCreateFlags.Signaled :
                    VkFenceCreateFlags.None
                )
            };

            VkFence fence;
            if (VulkanNative.vkCreateFence(
                device.Handle,
                &createInfo,
                null,
                &fence
            ) != VkResult.Success)
                throw new Exception("failed to create fence");
            _handle = fence;
        }
        unsafe ~Fence()
        {
            if (_handle != VkFence.Null)
            {
                VulkanNative.vkDestroyFence(
                    _device.Handle,
                    _handle,
                    null
                );
                _handle = VkFence.Null;
            }
        }

        public unsafe bool IsSignaled()
        => VulkanNative.vkGetFenceStatus(
            _device.Handle,
            _handle
        ) == VkResult.Success;

        public unsafe void Wait(ulong timeout = ulong.MaxValue)
        {
            var fence = _handle;
            if (VulkanNative.vkWaitForFences(
                _device.Handle,
                1,
                &fence,
                true,
                timeout
            ) != VkResult.Success)
                throw new Exception("failed to wait on a fence");
        }

        public unsafe Task WaitAsync(ulong timeout = ulong.MaxValue)
        => Task.Run(() =>
        {
            var fence = _handle;
            if (VulkanNative.vkWaitForFences(
                _device.Handle,
                1,
                &fence,
                true,
                timeout
            ) != VkResult.Success)
                throw new Exception("failed to wait on a fence");
        });

        public unsafe void Reset()
        {
            var fence = _handle;
            if (VulkanNative.vkResetFences(
                _device.Handle,
                1,
                &fence
            ) != VkResult.Success)
                throw new Exception("failed to reset fence");
        }
    }
}