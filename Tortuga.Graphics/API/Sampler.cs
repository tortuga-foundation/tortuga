#pragma warning disable CS1591
using System;
using Vulkan;

namespace Tortuga.Graphics.API
{
    public class Sampler
    {
        public Device Device => _device;
        public VkSampler Handle => _handle;

        private Device _device;
        private VkSampler _handle;

        public unsafe Sampler(Image image)
        {
            _device = image.Device;
            var createInfo = new VkSamplerCreateInfo
            {
                sType = VkStructureType.SamplerCreateInfo,
                magFilter = VkFilter.Linear,
                minFilter = VkFilter.Linear,
                addressModeU = VkSamplerAddressMode.Repeat,
                addressModeV = VkSamplerAddressMode.Repeat,
                addressModeW = VkSamplerAddressMode.Repeat,
                anisotropyEnable = true,
                maxAnisotropy = 16,
                borderColor = VkBorderColor.IntOpaqueBlack,
                unnormalizedCoordinates = false,
                compareEnable = false,
                compareOp = VkCompareOp.Always,
                mipmapMode = VkSamplerMipmapMode.Linear,
                mipLodBias = 0.0f,
                minLod = 0.0f,
                maxLod = image.MipLevel
            };

            VkSampler sampler;
            if (VulkanNative.vkCreateSampler(
                _device.Handle,
                &createInfo,
                null,
                &sampler
            ) != VkResult.Success)
                throw new Exception("failed to create sampler");
            _handle = sampler;
        }

        unsafe Sampler()
        {
            if (_handle != VkSampler.Null)
            {
                VulkanNative.vkDestroySampler(
                    _device.Handle,
                    _handle,
                    null
                );
                _handle = VkSampler.Null;
            }
        }
    }
}