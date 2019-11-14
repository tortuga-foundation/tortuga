#ifndef _VULKAN_DESCRIPTOR_TYPE
#define _VULKAN_DESCRIPTOR_TYPE

#include <vulkan/vulkan.h>

#include "./ErrorCheck.hpp"
#include "./Device.hpp"

namespace Tortuga
{
namespace Graphics
{
namespace Vulkan
{
namespace DescriptorLayout
{
struct DescriptorLayout
{
  VkDevice Device = VK_NULL_HANDLE;
  VkDescriptorSetLayout Layouts = VK_NULL_HANDLE;
  uint32_t BindingsAmount = 0;
  std::vector<VkDescriptorType> Types = {};
};

DescriptorLayout Create(Device::Device device, std::vector<VkShaderStageFlags> shaderStages, std::vector<VkDescriptorType> types);
DescriptorLayout Create(Device::Device device, VkShaderStageFlags shaderStage, VkDescriptorType type, uint32_t amount);
void Destroy(DescriptorLayout data);
} // namespace DescriptorLayout
} // namespace Vulkan
} // namespace Graphics
} // namespace Tortuga

#endif