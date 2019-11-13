#include "./Sampler.hpp"

namespace Tortuga
{
namespace Graphics
{
namespace Vulkan
{
namespace Sampler
{
Sampler Create(Device::Device device, uint32_t mipMapLevels)
{
  Sampler data = {};
  data.Device = device.Device;

  VkSamplerCreateInfo createInfo = {};
  {
    createInfo.sType = VK_STRUCTURE_TYPE_SAMPLER_CREATE_INFO;
    createInfo.magFilter = VK_FILTER_LINEAR;
    createInfo.minFilter = VK_FILTER_LINEAR;
    createInfo.addressModeU = VK_SAMPLER_ADDRESS_MODE_REPEAT;
    createInfo.addressModeV = VK_SAMPLER_ADDRESS_MODE_REPEAT;
    createInfo.addressModeW = VK_SAMPLER_ADDRESS_MODE_REPEAT;
    createInfo.anisotropyEnable = VK_TRUE;
    createInfo.maxAnisotropy = 4;
    createInfo.borderColor = VK_BORDER_COLOR_INT_OPAQUE_BLACK;
    createInfo.unnormalizedCoordinates = VK_FALSE;
    createInfo.compareEnable = VK_FALSE;
    createInfo.compareOp = VK_COMPARE_OP_ALWAYS;
    createInfo.mipmapMode = VK_SAMPLER_MIPMAP_MODE_LINEAR;
    createInfo.mipLodBias = 0.0f;
    createInfo.minLod = 0;
    createInfo.maxLod = mipMapLevels;
  }

  ErrorCheck::Callback(vkCreateSampler(device.Device, &createInfo, nullptr, &data.Sampler));
  return data;
}
void Destroy(Sampler data)
{
  if (data.Sampler == VK_NULL_HANDLE)
    return;

  vkDestroySampler(data.Device, data.Sampler, nullptr);
}
} // namespace Sampler
} // namespace Vulkan
} // namespace Graphics
} // namespace Tortuga