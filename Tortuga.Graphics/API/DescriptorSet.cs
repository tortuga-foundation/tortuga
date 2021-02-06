#pragma warning disable CS1591
using System;
using System.Collections.Generic;
using Tortuga.Utils;
using Vulkan;

namespace Tortuga.Graphics.API
{
    public class DescriptorSet
    {
        public DescriptorPool DescriptorPool => _descriptorPool;
        public Device Device => _device;
        public VkDescriptorSet Handle => _handle;

        private DescriptorPool _descriptorPool;
        private Device _device;
        private VkDescriptorSet _handle;

        public unsafe DescriptorSet(DescriptorPool descriptorPool, uint setCount = 1)
        {
            _device = descriptorPool.Device;
            _descriptorPool = descriptorPool;

            var layouts = new NativeList<VkDescriptorSetLayout>(setCount);
            for (int i = 0; i < setCount; i++)
                layouts.Add(descriptorPool.Layout.Handle);

            var allocateInfo = new VkDescriptorSetAllocateInfo
            {
                sType = VkStructureType.DescriptorSetAllocateInfo,
                descriptorPool = descriptorPool.Handle,
                descriptorSetCount = setCount,
                pSetLayouts = (VkDescriptorSetLayout*)layouts.Data.ToPointer()
            };

            VkDescriptorSet descriptorSet;
            if (VulkanNative.vkAllocateDescriptorSets(
                _device.Handle,
                &allocateInfo,
                &descriptorSet
            ) != VkResult.Success)
                throw new Exception("failed to allocate descriptor sets");
            _handle = descriptorSet;
        }

        public unsafe void UpdateBuffers(List<Buffer> buffers)
        {
            if (buffers.Count != _descriptorPool.Layout.Bindings.Count)
                throw new InvalidOperationException("buffers length should match descriptor layout bindings");

            var descriptorSetBufferInfos = new NativeList<VkDescriptorBufferInfo>();
            var writeDescriptorSets = new NativeList<VkWriteDescriptorSet>();

            for (int i = 0; i < _descriptorPool.Layout.Bindings.Count; i++)
            {
                var binding = _descriptorPool.Layout.Bindings[i];
                var descriptorSetBufferInfo = new VkDescriptorBufferInfo
                {
                    buffer = buffers[i].Handle,
                    offset = 0,
                    range = buffers[i].Size
                };

                var writeDescriptorSet = new VkWriteDescriptorSet
                {
                    sType = VkStructureType.WriteDescriptorSet,
                    dstSet = _handle,
                    dstBinding = binding.Index,
                    dstArrayElement = 0,
                    descriptorCount = binding.DescriptorCounts,
                    descriptorType = binding.DescriptorType,
                    pBufferInfo = &descriptorSetBufferInfo,
                };
                descriptorSetBufferInfos.Add(descriptorSetBufferInfo);
                writeDescriptorSets.Add(writeDescriptorSet);
            }
            VulkanNative.vkUpdateDescriptorSets(
                _device.Handle,
                writeDescriptorSets.Count,
                (VkWriteDescriptorSet*)writeDescriptorSets.Data.ToPointer(),
                0,
                null
            );
        }

        public unsafe void UpdateBuffer(Buffer buffer, int binding)
        {
            var vulkanBinding = _descriptorPool.Layout.Bindings[binding];
            var descriptorSetBufferInfo = new VkDescriptorBufferInfo
            {
                buffer = buffer.Handle,
                offset = 0,
                range = buffer.Size
            };

            var writeDescriptorSet = new VkWriteDescriptorSet
            {
                sType = VkStructureType.WriteDescriptorSet,
                dstSet = _handle,
                dstBinding = vulkanBinding.Index,
                dstArrayElement = 0,
                descriptorCount = vulkanBinding.DescriptorCounts,
                descriptorType = vulkanBinding.DescriptorType,
                pBufferInfo = &descriptorSetBufferInfo,
            };
            VulkanNative.vkUpdateDescriptorSets(
                _device.Handle,
                1,
                &writeDescriptorSet,
                0,
                null
            );
        }

        public unsafe void UpdateSampledImages(List<ImageView> views, List<Sampler> samplers)
        {
            if (views.Count != _descriptorPool.Layout.Bindings.Count)
                throw new InvalidOperationException("views length should match descriptor layout bindings");
            if (samplers.Count != _descriptorPool.Layout.Bindings.Count)
                throw new InvalidOperationException("samplers length should match descriptor layout bindings");

            var descriptorImageInfos = new NativeList<VkDescriptorImageInfo>();
            var writeDescriptorSets = new NativeList<VkWriteDescriptorSet>();

            for (int i = 0; i < _descriptorPool.Layout.Bindings.Count; i++)
            {
                var binding = _descriptorPool.Layout.Bindings[i];
                var descriptorImageInfo = new VkDescriptorImageInfo
                {
                    imageLayout = views[i].Image.Layout,
                    imageView = views[i].Handle,
                    sampler = samplers[i].Handle
                };

                var writeDescriptorSet = new VkWriteDescriptorSet
                {
                    sType = VkStructureType.WriteDescriptorSet,
                    dstSet = _handle,
                    dstBinding = binding.Index,
                    dstArrayElement = 0,
                    descriptorCount = binding.DescriptorCounts,
                    descriptorType = binding.DescriptorType,
                    pImageInfo = &descriptorImageInfo
                };
                descriptorImageInfos.Add(descriptorImageInfo);
                writeDescriptorSets.Add(writeDescriptorSet);
            }
            VulkanNative.vkUpdateDescriptorSets(
                _device.Handle,
                writeDescriptorSets.Count,
                (VkWriteDescriptorSet*)writeDescriptorSets.Data.ToPointer(),
                0,
                null
            );
        }

        public unsafe void UpdateSampledImage(ImageView view, Sampler sampler, int binding)
        {
            var vulkanBinding = _descriptorPool.Layout.Bindings[binding];
            var descriptorImageInfo = new VkDescriptorImageInfo
            {
                imageLayout = view.Image.Layout,
                imageView = view.Handle,
                sampler = sampler.Handle
            };

            var writeDescriptorSet = new VkWriteDescriptorSet
            {
                sType = VkStructureType.WriteDescriptorSet,
                dstSet = _handle,
                dstBinding = vulkanBinding.Index,
                dstArrayElement = 0,
                descriptorCount = vulkanBinding.DescriptorCounts,
                descriptorType = vulkanBinding.DescriptorType,
                pImageInfo = &descriptorImageInfo
            };
            VulkanNative.vkUpdateDescriptorSets(
                _device.Handle,
                1,
                &writeDescriptorSet,
                0,
                null
            );
        }
    }
}