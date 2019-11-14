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
DescriptorLayout Create(Device::Device device, std::vector<VkShaderStageFlags> shaderStages, std::vector<VkDescriptorType> types)
{
  DescriptorLayout data = {};
  if (shaderStages.size() != types.size())
  {
    Console::Error("shader stages and types must be the same length");
    return data;
  }

  data.Device = device.Device;
  data.BindingsAmount = types.size();
  data.Types = types;

  std::vector<VkDescriptorSetLayoutBinding> pBindings(types.size());
  for (uint32_t i = 0; i < types.size(); i++)
  {
    //bindings
    pBindings[i].binding = i;
    pBindings[i].descriptorType = types[i];
    pBindings[i].descriptorCount = 1;
    pBindings[i].stageFlags = shaderStages[i];
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
DescriptorLayout Create(Device::Device device, VkShaderStageFlags shaderStage, VkDescriptorType type, uint32_t amount)
{
  std::vector<VkShaderStageFlags> stages(amount);
  std::vector<VkDescriptorType> types(amount);
  for (uint32_t i = 0; i < amount; i++)
  {
    stages[i] = shaderStage;
    types[i] = type;
  }
  return Create(device, stages, types);
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