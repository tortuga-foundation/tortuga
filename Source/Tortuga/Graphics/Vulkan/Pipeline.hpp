#ifndef _VULKAN_PIPELINE
#define _VULKAN_PIPELINE

#include <vulkan/vulkan.h>

#include "./ErrorCheck.hpp"
#include "./Device.hpp"
#include "./DescriptorLayout.hpp"
#include "./Shader.hpp"
#include "./RenderPass.hpp"

namespace Tortuga
{
namespace Graphics
{
namespace Vulkan
{
namespace Pipeline
{
struct Pipeline
{
  VkDevice Device = VK_NULL_HANDLE;
  VkPipelineLayout Layout = VK_NULL_HANDLE;
  VkPipeline Pipeline = VK_NULL_HANDLE;
  VkRenderPass RenderPass = VK_NULL_HANDLE;
  VkPipelineCache Cache = VK_NULL_HANDLE;
};
Pipeline Create(
  Device::Device device,
  RenderPass::RenderPass renderPass,
  std::vector<Shader::Shader> shaders,
  std::vector<VkVertexInputBindingDescription> vertexBindings,
  std::vector<VkVertexInputAttributeDescription> vertexAttributes,
  std::vector<DescriptorLayout::DescriptorLayout> descriptorLayouts
);
void Destroy(Pipeline data);
} // namespace Pipeline
} // namespace Vulkan
} // namespace Graphics
} // namespace Tortuga

#endif