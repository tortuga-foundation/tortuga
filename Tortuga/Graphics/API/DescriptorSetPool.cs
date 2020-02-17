using System;
using System.Collections.Generic;
using Vulkan;
using static Vulkan.VulkanNative;

namespace Tortuga.Graphics.API
{
    internal class DescriptorSetPool
    {
        private VkDescriptorPool _descriptorSetPool;
        private uint _totalSets;
        private DescriptorSetLayout _descriptorSetLayout;

        public unsafe DescriptorSetPool(DescriptorSetLayout layout)
        {
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
                Engine.Instance.MainDevice.LogicalDevice,
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
                Engine.Instance.MainDevice.LogicalDevice,
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
                Engine.Instance.MainDevice.LogicalDevice,
                &allocateInfo,
                &descriptorSet
            ) != VkResult.Success)
                throw new Exception("failed to allocate descriptor set");

            return new DescriptorSet(descriptorSet, this, setArrayCount);
        }

        internal class DescriptorSet
        {
            public VkDescriptorSet Handle => _descriptorSet;

            private VkDescriptorSet _descriptorSet;
            private DescriptorSetPool _pool;
            private uint _arrayCount;

            internal DescriptorSet(VkDescriptorSet descriptorSet, DescriptorSetPool pool, uint arrayCount)
            {
                _descriptorSet = descriptorSet;
                _pool = pool;
                _arrayCount = arrayCount;
            }

            public unsafe void BuffersUpdate(Buffer[] buffers, uint arrayIndex = 0)
            {
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
                        writeInfos.Add(new VkWriteDescriptorSet
                        {
                            dstSet = _descriptorSet,
                            dstBinding = i,
                            dstArrayElement = arrayIndex,
                            descriptorCount = _arrayCount,
                            descriptorType = bindings.type,
                            pBufferInfo = buff
                        });
                    }
                }
                vkUpdateDescriptorSets(
                    Engine.Instance.MainDevice.LogicalDevice,
                    writeInfos.Count,
                    (VkWriteDescriptorSet*)writeInfos.Data.ToPointer(),
                    0,
                    null
                );
            }
        }
    }
}