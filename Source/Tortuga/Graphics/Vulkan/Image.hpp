#ifndef _VULKAN_IMAGE
#define _VULKAN_IMAGE

#include <vulkan/vulkan.h>

#include "./ErrorCheck.hpp"
#include "./Device.hpp"

namespace Tortuga
{
namespace Graphics
{
namespace Vulkan
{
namespace Image
{
struct Image
{
  VkDevice Device = VK_NULL_HANDLE;
  VkImage Image = VK_NULL_HANDLE;
  VkDeviceMemory Memory = VK_NULL_HANDLE;
  VkFormat Format;
  uint32_t Width;
  uint32_t Height;
  uint32_t MipMapLevel;
};

//depth buffer helpers
VkFormat FindSupportedFormat(VkPhysicalDevice physicalDevice, const std::vector<VkFormat> &candidates, VkImageTiling tiling, VkFormatFeatureFlags features);
VkFormat FindDepthFormat(VkPhysicalDevice device);
bool HasStencilComponent(VkFormat format);

Image Create(Device::Device device, uint32_t width, uint32_t height, VkFormat imageFormat, VkImageUsageFlags usageFlags, uint32_t mipMapLevels = 1);
void Destroy(Image data);
} // namespace Image
} // namespace Vulkan
} // namespace Graphics
} // namespace Tortuga

#endif