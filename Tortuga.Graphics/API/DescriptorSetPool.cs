using System;
using System.Collections.Generic;
using Vulkan;
using Tortuga.Utils;
using static Vulkan.VulkanNative;

namespace Tortuga.Graphics.API
{
    internal class DescriptorSetPool
    {
        public Device DeviceUsed => _device;
        private VkDescriptorPool _descriptorSetPool;
        private uint _totalSets;
        private DescriptorSetLayout _descriptorSetLayout;
        private Device _device;

        public unsafe DescriptorSetPool(DescriptorSetLayout layout)
        {
            _device = layout.DeviceUsed;
            _descriptorSetLayout = layout;
            _totalSets = 0;
            var poolSizes = new List<VkDescriptorPoolSize>();
            foreach (var info in layout.CreateInfoUsed)
            {
                var poolSizeIndex = poolSizes.FindIndex(p => p.type == info.type);
                if (poolSizeIndex != -1)
                {
                    var currentCount = poolSizes[poolSizeIndex].descriptorCount;
                    poolSizes[poolSizeIndex] = new VkDescriptorPoolSize
                    {
                        type = info.type,
                        descriptorCount = currentCount + 1
                    };
                    _totalSets++;
                }
                else
                {
                    poolSizes.Add(new VkDescriptorPoolSize
                    {
                        type = info.type,
                        descriptorCount = 1
                    });
                    _totalSets++;
                }
            }

            var pPoolSizes = new NativeList<VkDescriptorPoolSize>();
            foreach (var p in poolSizes)
                pPoolSizes.Add(p);

            var descriptorPoolCreateInfo = VkDescriptorPoolCreateInfo.New();
            descriptorPoolCreateInfo.maxSets = _totalSets;
            descriptorPoolCreateInfo.poolSizeCount = Convert.ToUInt32(poolSizes.Count);
            descriptorPoolCreateInfo.pPoolSizes = (VkDescriptorPoolSize*)pPoolSizes.Data.ToPointer();

            VkDescriptorPool descriptorSetPool;
            if (vkCreateDescriptorPool(
                _device.LogicalDevice,
                &descriptorPoolCreateInfo,
                null,
                &descriptorSetPool
            ) != VkResult.Success)
                throw new Exception("failed to create descriptor set pool");
            _descriptorSetPool = descriptorSetPool;
        }

        unsafe ~DescriptorSetPool()
        {
            vkDestroyDescriptorPool(
                _device.LogicalDevice,
                _descriptorSetPool,
                null
            );
        }

        public unsafe DescriptorSet AllocateDescriptorSet(uint setArrayCount = 1)
        {
            var layouts = new NativeList<VkDescriptorSetLayout>(setArrayCount);
            layouts.Count = setArrayCount;
            for (uint i = 0; i < setArrayCount; i++)
                layouts[i] = _descriptorSetLayout.Handle;

            var allocateInfo = VkDescriptorSetAllocateInfo.New();
            allocateInfo.descriptorPool = _descriptorSetPool;
            allocateInfo.descriptorSetCount = setArrayCount;
            allocateInfo.pSetLayouts = (VkDescriptorSetLayout*)layouts.Data.ToPointer();

            VkDescriptorSet descriptorSet;
            if (vkAllocateDescriptorSets(
                _device.LogicalDevice,
                &allocateInfo,
                &descriptorSet
            ) != VkResult.Success)
                throw new Exception("failed to allocate descriptor set");

            return new DescriptorSet(descriptorSet, this, setArrayCount);
        }

        internal class DescriptorSet
        {
            public VkDescriptorSet Handle => _descriptorSet;
            public Device DeviceUsed => _device;

            private VkDescriptorSet _descriptorSet;
            private DescriptorSetPool _pool;
            private uint _arrayCount;
            private Device _device;

            internal DescriptorSet(VkDescriptorSet descriptorSet, DescriptorSetPool pool, uint arrayCount)
            {
                _device = pool.DeviceUsed;
                _descriptorSet = descriptorSet;
                _pool = pool;
                _arrayCount = arrayCount;
            }

            public unsafe void BuffersUpdate(Buffer[] buffers, uint arrayIndex = 0)
            {
                if (buffers.Length != _pool._descriptorSetLayout.CreateInfoUsed.Length)
                    throw new Exception("provided incorrect number of buffers");

                var bufferInfo = new NativeList<VkDescriptorBufferInfo>();
                var writeInfos = new NativeList<VkWriteDescriptorSet>();
                for (uint i = 0; i < _pool._descriptorSetLayout.CreateInfoUsed.Length; i++)
                {
                    bufferInfo.Add(new VkDescriptorBufferInfo
                    {
                        buffer = buffers[i].Handle,
                        offset = 0,
                        range = buffers[i].Size
                    });

                    var bindings = _pool._descriptorSetLayout.CreateInfoUsed[i];
                    fixed (VkDescriptorBufferInfo* buff = &bufferInfo[i])
                    {
                        var info = VkWriteDescriptorSet.New();
                        info.dstSet = _descriptorSet;
                        info.dstBinding = i;
                        info.dstArrayElement = arrayIndex;
                        info.descriptorCount = _arrayCount;
                        info.descriptorType = bindings.type;
                        info.pBufferInfo = buff;
                        writeInfos.Add(info);
                    }
                }
                vkUpdateDescriptorSets(
                    _device.LogicalDevice,
                    writeInfos.Count,
                    (VkWriteDescriptorSet*)writeInfos.Data.ToPointer(),
                    0,
                    null
                );
            }

            public unsafe void BuffersUpdate(Buffer buffers, uint binding = 0, uint arrayIndex = 0)
            {
                var bufferInfo = new VkDescriptorBufferInfo()
                {
                    buffer = buffers.Handle,
                    offset = 0,
                    range = buffers.Size
                };
                var writeInfos = VkWriteDescriptorSet.New();


                var bindings = _pool._descriptorSetLayout.CreateInfoUsed[binding];
                writeInfos.dstSet = _descriptorSet;
                writeInfos.dstBinding = binding;
                writeInfos.dstArrayElement = arrayIndex;
                writeInfos.descriptorCount = _arrayCount;
                writeInfos.descriptorType = bindings.type;
                writeInfos.pBufferInfo = &bufferInfo;
                vkUpdateDescriptorSets(
                    _device.LogicalDevice,
                    1,
                    &writeInfos,
                    0,
                    null
                );
            }

            public unsafe void SampledImageUpdate(ImageView[] view, Sampler[] sampler, uint arrayIndex = 0)
            {
                if (view.Length != _pool._descriptorSetLayout.CreateInfoUsed.Length)
                    throw new Exception("provided incorrect number of image views");
                if (sampler.Length != _pool._descriptorSetLayout.CreateInfoUsed.Length)
                    throw new Exception("provided incorrect number of samplers");

                var imageInfo = new NativeList<VkDescriptorImageInfo>();
                var writeInfos = new NativeList<VkWriteDescriptorSet>();
                for (uint i = 0; i < _pool._descriptorSetLayout.CreateInfoUsed.Length; i++)
                {
                    imageInfo.Add(new VkDescriptorImageInfo
                    {
                        imageLayout = VkImageLayout.ShaderReadOnlyOptimal,
                        imageView = view[i].Handle,
                        sampler = sampler[i].Handle
                    });

                    var bindings = _pool._descriptorSetLayout.CreateInfoUsed[i];
                    fixed (VkDescriptorImageInfo* img = &imageInfo[i])
                    {
                        var info = VkWriteDescriptorSet.New();
                        info.dstSet = _descriptorSet;
                        info.dstBinding = i;
                        info.dstArrayElement = arrayIndex;
                        info.descriptorCount = _arrayCount;
                        info.descriptorType = bindings.type;
                        info.pImageInfo = img;
                        writeInfos.Add(info);
                    }
                }
                vkUpdateDescriptorSets(
                    _device.LogicalDevice,
                    writeInfos.Count,
                    (VkWriteDescriptorSet*)writeInfos.Data.ToPointer(),
                    0,
                    null
                );
            }

            public unsafe void SampledImageUpdate(VkImageLayout layout, ImageView view, Sampler sampler, uint binding = 0, uint arrayIndex = 0)
            {
                var imageInfo = new VkDescriptorImageInfo
                {
                    imageLayout = layout,
                    imageView = view.Handle,
                    sampler = sampler.Handle
                };
                var writeInfos = VkWriteDescriptorSet.New();

                var bindings = _pool._descriptorSetLayout.CreateInfoUsed[binding];
                writeInfos.dstSet = _descriptorSet;
                writeInfos.dstBinding = binding;
                writeInfos.dstArrayElement = arrayIndex;
                writeInfos.descriptorCount = _arrayCount;
                writeInfos.descriptorType = bindings.type;
                writeInfos.pImageInfo = &imageInfo;
            

                vkUpdateDescriptorSets(
                    _device.LogicalDevice,
                    1,
                    &writeInfos,
                    0,
                    null
                );
            }
        }
    }
}