#pragma warning disable CS1591
using System;
using System.Collections.Generic;
using Tortuga.Utils;
using Vulkan;

/*
-- GPU DATA STRUCTURE --
struct Descriptor { type specific data }
struct DescriptorBinding {   
  int binding;
  DescriptorType type;
  Descriptor descriptors[]
};

struct DescriptorSet {
    DescriptorBinding bindings[];
};

struct PipelineLayout {
  DescriptorSet sets[]
}
*/

namespace Tortuga.Graphics.API
{
    public struct DescriptorBindingInfo
    {
        public uint Index;
        public VkDescriptorType DescriptorType;
        public uint DescriptorCounts;
        public VkShaderStageFlags ShaderStageFlags;

        public DescriptorBindingInfo(
            uint index,
            VkDescriptorType descriptorType,
            uint descriptorCounts,
            VkShaderStageFlags shaderStageFlags
        )
        {
            Index = index;
            DescriptorType = descriptorType;
            DescriptorCounts = descriptorCounts;
            ShaderStageFlags = shaderStageFlags;
        }
    }

    public class DescriptorLayout
    {
        public Device Device => _device;
        public VkDescriptorSetLayout Handle => _handle;
        public List<DescriptorBindingInfo> Bindings => _bindings;

        private Device _device;
        private VkDescriptorSetLayout _handle;
        private List<DescriptorBindingInfo> _bindings;

        public unsafe DescriptorLayout(Device device, List<DescriptorBindingInfo> bindings)
        {
            _device = device;
            _bindings = bindings;

            var vulkanBindings = new NativeList<VkDescriptorSetLayoutBinding>();
            foreach (var binding in bindings)
            {
                vulkanBindings.Add(new VkDescriptorSetLayoutBinding
                {
                    binding = binding.Index,
                    descriptorType = binding.DescriptorType,
                    descriptorCount = binding.DescriptorCounts,
                    stageFlags = binding.ShaderStageFlags
                });
            }

            var createInfo = new VkDescriptorSetLayoutCreateInfo
            {
                sType = VkStructureType.DescriptorSetLayoutCreateInfo,
                bindingCount = vulkanBindings.Count,
                pBindings = (VkDescriptorSetLayoutBinding*)vulkanBindings.Data.ToPointer()
            };

            VkDescriptorSetLayout layout;
            if (VulkanNative.vkCreateDescriptorSetLayout(
                device.Handle,
                &createInfo,
                null,
                &layout
            ) != VkResult.Success)
                throw new Exception("failed to create descriptor set layout");
            _handle = layout;
        }
        unsafe ~DescriptorLayout()
        {
            if (_handle != VkDescriptorSetLayout.Null)
            {
                VulkanNative.vkDestroyDescriptorSetLayout(
                    _device.Handle,
                    _handle,
                    null
                );
                _handle = VkDescriptorSetLayout.Null;
            }
        }
    }
}