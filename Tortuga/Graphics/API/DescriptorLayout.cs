using System;
using Vulkan;
using Tortuga.Utils;
using static Vulkan.VulkanNative;

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
    internal struct DescriptorSetCreateInfo
    {
        public VkDescriptorType type;
        public VkShaderStageFlags stage;
    };

    internal class DescriptorSetLayout
    {
        public VkDescriptorSetLayout Handle => _layout;
        public DescriptorSetCreateInfo[] CreateInfoUsed => _createInfo;

        private VkDescriptorSetLayout _layout;
        private DescriptorSetCreateInfo[] _createInfo;


        private unsafe void SetupDescriptorSetLayout(NativeList<VkDescriptorSetLayoutBinding> bindings)
        {
            var descriptorSetLayoutInfo = VkDescriptorSetLayoutCreateInfo.New();
            descriptorSetLayoutInfo.bindingCount = bindings.Count;
            descriptorSetLayoutInfo.pBindings = (VkDescriptorSetLayoutBinding*)bindings.Data.ToPointer();

            VkDescriptorSetLayout layout;
            if (vkCreateDescriptorSetLayout(
              Engine.Instance.MainDevice.LogicalDevice,
              &descriptorSetLayoutInfo,
              null,
              &layout
            ) != VkResult.Success)
                throw new Exception("failed to create descriptor set layout");
            _layout = layout;
        }

        public DescriptorSetLayout(DescriptorSetCreateInfo[] createInfo)
        {
            this._createInfo = createInfo;
            var bindings = new NativeList<VkDescriptorSetLayoutBinding>();
            for (uint i = 0; i < createInfo.Length; i++)
            {
                var info = createInfo[i];
                bindings.Add(new VkDescriptorSetLayoutBinding
                {
                    binding = i,
                    descriptorType = info.type,
                    descriptorCount = 1,
                    stageFlags = info.stage
                });
            }
            this.SetupDescriptorSetLayout(bindings);
        }

        public DescriptorSetLayout(VkDescriptorType type, VkShaderStageFlags stage, int amount)
        {
            var bindings = new NativeList<VkDescriptorSetLayoutBinding>();
            for (uint i = 0; i < amount; i++)
            {
                bindings.Add(new VkDescriptorSetLayoutBinding
                {
                    binding = i,
                    descriptorType = type,
                    descriptorCount = 1,
                    stageFlags = stage
                });
            }
            this.SetupDescriptorSetLayout(bindings);
        }

        unsafe ~DescriptorSetLayout()
        {
            vkDestroyDescriptorSetLayout(
              Engine.Instance.MainDevice.LogicalDevice,
              _layout,
              null
            );
        }
    }
}