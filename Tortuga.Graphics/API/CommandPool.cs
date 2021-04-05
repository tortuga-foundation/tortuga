#pragma warning disable CS1591
using Vulkan;

namespace Tortuga.Graphics.API
{
    public class CommandPool
    {
        public QueueFamily QueueFamily => _queueFamily;
        public VkCommandPool Handle => _handle;
        public Device Device => _device;

        private QueueFamily _queueFamily;
        private Device _device;
        private VkCommandPool _handle;

        public unsafe CommandPool(QueueFamily queueFamily)
        {
            _queueFamily = queueFamily;
            _device = queueFamily.Device;
            var createInfo = new VkCommandPoolCreateInfo
            {
                sType = VkStructureType.CommandPoolCreateInfo,
                flags = VkCommandPoolCreateFlags.ResetCommandBuffer,
                queueFamilyIndex = queueFamily.Index
            };

            VkCommandPool commandPool;
            if (VulkanNative.vkCreateCommandPool(
                queueFamily.Device.Handle,
                &createInfo,
                null,
                &commandPool
            ) != VkResult.Success)
                throw new System.Exception("failed to create command pool on device");
            _handle = commandPool;
        }
        unsafe ~CommandPool()
        {
            if (_handle != VkCommandPool.Null)
            {
                VulkanNative.vkDestroyCommandPool(
                    _queueFamily.Device.Handle,
                    _handle,
                    null
                );
                _handle = VkCommandPool.Null;
            }
        }
    }
}