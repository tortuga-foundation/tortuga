#pragma warning disable 1591
#pragma warning disable 0649

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

    /// <summary>
    /// Different type of descriptor set types, for more information please look at VkDescriptorType
    /// </summary>
    public enum DescriptorType
    {
        Sampler = VkDescriptorType.Sampler,
        CombinedImageSampler = VkDescriptorType.CombinedImageSampler,
        SampledImage = VkDescriptorType.SampledImage,
        StorageImage = VkDescriptorType.StorageImage,
        UniformTexelBuffer = VkDescriptorType.UniformTexelBuffer,
        StorageTexelBuffer = VkDescriptorType.StorageTexelBuffer,
        UniformBuffer = VkDescriptorType.UniformBuffer,
        StorageBuffer = VkDescriptorType.StorageBuffer,
        UniformBufferDynamic = VkDescriptorType.UniformBufferDynamic,
        StorageBufferDynamic = VkDescriptorType.StorageBufferDynamic,
        InputAttachment = VkDescriptorType.InputAttachment
    }

    /// <summary>
    /// Different type of shader stages, for more information please look at VkShaderStageFlags
    /// </summary>
    public enum ShaderStageType
    {
        None = VkShaderStageFlags.None,
        Vertex = VkShaderStageFlags.Vertex,
        TessellationControl = VkShaderStageFlags.TessellationControl,
        TessellationEvaluation = VkShaderStageFlags.TessellationEvaluation,
        Geometry = VkShaderStageFlags.Geometry,
        Fragment = VkShaderStageFlags.Fragment,
        AllGraphics = VkShaderStageFlags.AllGraphics,
        Compute = VkShaderStageFlags.Compute,
        All = VkShaderStageFlags.All
    }

    public struct DescriptorSetCreateInfo
    {
        public DescriptorType type;
        public ShaderStageType stage;
    };

    public class DescriptorSetLayout
    {
        internal VkDescriptorSetLayout Handle => _layout;
        public DescriptorSetCreateInfo[] CreateInfoUsed => _createInfo;
        internal Device DeviceUsed => _device;

        private VkDescriptorSetLayout _layout;
        private DescriptorSetCreateInfo[] _createInfo;
        private Device _device;


        private unsafe void SetupDescriptorSetLayout(NativeList<VkDescriptorSetLayoutBinding> bindings)
        {
            var descriptorSetLayoutInfo = VkDescriptorSetLayoutCreateInfo.New();
            descriptorSetLayoutInfo.bindingCount = bindings.Count;
            descriptorSetLayoutInfo.pBindings = (VkDescriptorSetLayoutBinding*)bindings.Data.ToPointer();

            VkDescriptorSetLayout layout;
            if (vkCreateDescriptorSetLayout(
              _device.LogicalDevice,
              &descriptorSetLayoutInfo,
              null,
              &layout
            ) != VkResult.Success)
                throw new Exception("failed to create descriptor set layout");
            _layout = layout;
        }

        public DescriptorSetLayout(Device device, DescriptorSetCreateInfo[] createInfo)
        {
            _device = device;
            this._createInfo = createInfo;
            var bindings = new NativeList<VkDescriptorSetLayoutBinding>();
            for (uint i = 0; i < createInfo.Length; i++)
            {
                var info = createInfo[i];
                bindings.Add(new VkDescriptorSetLayoutBinding
                {
                    binding = i,
                    descriptorType = (VkDescriptorType)info.type,
                    descriptorCount = 1,
                    stageFlags = (VkShaderStageFlags)info.stage
                });
            }
            this.SetupDescriptorSetLayout(bindings);
        }

        unsafe ~DescriptorSetLayout()
        {
            vkDestroyDescriptorSetLayout(
              _device.LogicalDevice,
              _layout,
              null
            );
        }
    }
}