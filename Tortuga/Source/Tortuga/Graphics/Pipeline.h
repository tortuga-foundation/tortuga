#ifndef _PIPELINE
#define _PIPELINE

#include "VulkanAPI/DataStructures.h"
#include "VulkanAPI/RenderPass.h"
#include "VulkanAPI/Pipeline.h"

#include "HardwareController.h"
#include "RenderPass.h"
#include "Shader.h"
#include "DescriptorLayout.h"

namespace Tortuga
{
namespace Graphics
{
struct Pipeline
{
  DescriptorLayout Layout;
  std::vector<VulkanAPI::PipelineData> VulkanPipeline;
};

Pipeline CreatePipeline(HardwareController hardware, RenderPass renderPass, std::vector<Shader> shaders);
void DestroyPipeline(Pipeline data);

}; // namespace Graphics
}; // namespace Tortuga

#endif