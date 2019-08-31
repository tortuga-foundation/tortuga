#include "./DescriptorLayout.hpp"

/*
struct Descriptor { type specific data }

struct DescriptorBinding {   
  int binding;
  DescriptorType type;
  Descriptor descriptors[]
};

struct DescriptorSet {
    DescriptorBinding bindings[];
};

struct PipelineLayout {
  DescriptorSet sets[]
}
*/

namespace Tortuga
{
namespace Graphics
{
namespace Vulkan
{
namespace DescriptorLayout
{
DescriptorLayout Create(Device::Device device, uint32_t bindingsAmount, VkShaderStageFlags shaderStage, VkDescriptorType type)
{
  DescriptorLayout data = {};
  data.Device = device.Device;
  data.BindingsAmount = bindingsAmount;
  data.Type = type;

  std::vector<VkDescriptorSetLayoutBinding> pBindings(bindingsAmount);
  for (uint32_t i = 0; i < bindingsAmount; i++)
  {
    //bindings
    pBindings[i].binding = i;
    pBindings[i].descriptorType = type;
    pBindings[i].descriptorCount = 1;
    pBindings[i].stageFlags = shaderStage;
    pBindings[i].pImmutableSamplers = VK_NULL_HANDLE;
  }
  VkDescriptorSetLayoutCreateInfo createInfo = {};
  {
    createInfo.sType = VK_STRUCTURE_TYPE_DESCRIPTOR_SET_LAYOUT_CREATE_INFO;
    createInfo.bindingCount = pBindings.size();
    createInfo.pBindings = pBindings.data();
  }
  ErrorCheck::Callback(vkCreateDescriptorSetLayout(device.Device, &createInfo, nullptr, &data.Layouts));
  return data;
}
void Destroy(DescriptorLayout data)
{
  if (data.Layouts == VK_NULL_HANDLE)
    return;

  vkDestroyDescriptorSetLayout(data.Device, data.Layouts, nullptr);
}
} // namespace DescriptorLayout
} // namespace Vulkan
} // namespace Graphics
} // namespace Tortuga