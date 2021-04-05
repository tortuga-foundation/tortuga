#pragma warning disable CS1591
using System;
using System.Collections.Generic;
using Tortuga.Utils;
using Vulkan;

namespace Tortuga.Graphics.API
{
    public class DescriptorPool
    {
        public uint MaxSets => _maxSets;
        public Device Device => _device;
        public DescriptorLayout Layout => _layout;
        public VkDescriptorPool Handle => _handle;

        private uint _maxSets;
        private Device _device;
        private DescriptorLayout _layout;
        private VkDescriptorPool _handle;

        public unsafe DescriptorPool(DescriptorLayout layout, uint maxSets)
        {
            _device = layout.Device;
            _layout = layout;
            _maxSets = maxSets;

            var poolSizes = new List<VkDescriptorPoolSize>();
            foreach (var binding in layout.Bindings)
            {
                var poolSizeIndex = poolSizes.FindIndex(
                    p => p.type == binding.DescriptorType
                );
                if (poolSizeIndex == -1)
                {
                    poolSizes.Add(new VkDescriptorPoolSize
                    {
                        type = binding.DescriptorType,
                        descriptorCount = binding.DescriptorCounts
                    });
                }
                else
                {
                    poolSizes[poolSizeIndex] = new VkDescriptorPoolSize
                    {
                        type = binding.DescriptorType,
                        descriptorCount = (
                            poolSizes[poolSizeIndex].descriptorCount +
                            binding.DescriptorCounts
                        )
                    };
                }
            }

            var vulkanPoolSizes = new NativeList<VkDescriptorPoolSize>();
            foreach (var p in poolSizes)
                vulkanPoolSizes.Add(p);

            var createInfo = new VkDescriptorPoolCreateInfo
            {
                sType = VkStructureType.DescriptorPoolCreateInfo,
                maxSets = maxSets,
                poolSizeCount = vulkanPoolSizes.Count,
                pPoolSizes = (VkDescriptorPoolSize*)vulkanPoolSizes.Data.ToPointer()
            };

            VkDescriptorPool descriptorPool;
            if (VulkanNative.vkCreateDescriptorPool(
                _device.Handle,
                &createInfo,
                null,
                &descriptorPool
            ) != VkResult.Success)
                throw new Exception("failed to create descriptor pool");
            _handle = descriptorPool;
        }

        unsafe ~DescriptorPool()
        {
            if (_handle != VkDescriptorPool.Null)
            {
                VulkanNative.vkDestroyDescriptorPool(
                    _device.Handle,
                    _handle,
                    null
                );
                _handle = VkDescriptorPool.Null;
            }
        }
    }
}