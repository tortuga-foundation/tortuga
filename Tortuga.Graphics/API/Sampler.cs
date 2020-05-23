using Vulkan;
using static Vulkan.VulkanNative;

namespace Tortuga.Graphics.API
{
    internal class Sampler
    {
        public VkSampler Handle => _sampler;
        public Device DeviceUsed => _device;

        private VkSampler _sampler;
        private Device _device;

        public unsafe Sampler(Device device)
        {
            _device = device;
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
                _device.LogicalDevice,
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
                _device.LogicalDevice,
                _sampler,
                null
            );
        }
    }
}