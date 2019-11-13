#ifndef _VULKAN_SAMPLER
#define _VULKAN_SAMPLER

#include <vulkan/vulkan.h>

#include "./ErrorCheck.hpp"
#include "./Device.hpp"
#include "./Image.hpp"

namespace Tortuga
{
namespace Graphics
{
namespace Vulkan
{
namespace Sampler
{
struct Sampler
{
  VkDevice Device = VK_NULL_HANDLE;
  VkSampler Sampler = VK_NULL_HANDLE;
};
Sampler Create(Device::Device device, uint32_t mipMapLevels = 1);
void Destroy(Sampler data);
} // namespace Sampler
} // namespace Vulkan
} // namespace Graphics
} // namespace Tortuga

#endif