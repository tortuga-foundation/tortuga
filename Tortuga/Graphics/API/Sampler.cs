using Vulkan;
using static Vulkan.VulkanNative;

namespace Tortuga.Graphics.API
{
    public class Sampler
    {
        public VkSampler Handle => _sampler;

        private VkSampler _sampler;

        public unsafe Sampler()
        {
            var samplerInfo = VkSamplerCreateInfo.New();
            samplerInfo.magFilter = VkFilter.Linear;
            samplerInfo.minFilter = VkFilter.Linear;
            samplerInfo.addressModeU = VkSamplerAddressMode.Repeat;
            samplerInfo.addressModeV = VkSamplerAddressMode.Repeat;
            samplerInfo.addressModeW = VkSamplerAddressMode.Repeat;
            samplerInfo.anisotropyEnable = true;
            samplerInfo.maxAnisotropy = 16;
            samplerInfo.borderColor = VkBorderColor.IntOpaqueBlack;
            samplerInfo.unnormalizedCoordinates = false;
            samplerInfo.compareEnable = false;
            samplerInfo.compareOp = VkCompareOp.Always;
            samplerInfo.mipmapMode = VkSamplerMipmapMode.Linear;
            samplerInfo.mipLodBias = 0.0f;
            samplerInfo.minLod = 0.0f;
            samplerInfo.maxLod = 0.0f;

            VkSampler sampler;
            if (vkCreateSampler(
                Engine.Instance.MainDevice.LogicalDevice,
                &samplerInfo,
                null,
                &sampler
            ) != VkResult.Success)
                throw new System.Exception("failed to create sampler");
            _sampler = sampler;
        }
        unsafe ~Sampler()
        {
            vkDestroySampler(
                Engine.Instance.MainDevice.LogicalDevice,
                _sampler,
                null
            );
        }
    }
}