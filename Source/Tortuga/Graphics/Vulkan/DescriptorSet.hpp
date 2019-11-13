#ifndef _VULKAN_DESCRIPTOR_SET
#define _VULKAN_DESCRIPTOR_SET

#include <vulkan/vulkan.h>

#include "./ErrorCheck.hpp"
#include "./Device.hpp"
#include "./DescriptorLayout.hpp"
#include "./DescriptorPool.hpp"
#include "./Buffer.hpp"
#include "./Image.hpp"
#include "./ImageView.hpp"
#include "./Sampler.hpp"

namespace Tortuga
{
namespace Graphics
{
namespace Vulkan
{
namespace DescriptorSet
{
struct DescriptorSet
{
  uint32_t SetArrayCount;
  VkDevice Device = VK_NULL_HANDLE;
  VkDescriptorSet set = VK_NULL_HANDLE;
  DescriptorPool::DescriptorPool Pool;
  DescriptorLayout::DescriptorLayout Layout;
};

DescriptorSet Create(Device::Device device, DescriptorPool::DescriptorPool pool, DescriptorLayout::DescriptorLayout layout, uint32_t setArrayCount = 1);
void UpdateDescriptorSet(DescriptorSet data, std::vector<Buffer::Buffer> content, uint32_t setArrayIndex = 0);
void UpdateDescriptorSet(DescriptorSet data, std::vector<ImageView::ImageView> content, Sampler::Sampler sampler, uint32_t setArrayIndex = 0);
} // namespace DescriptorSet
} // namespace Vulkan
} // namespace Graphics
} // namespace Tortuga

#endif