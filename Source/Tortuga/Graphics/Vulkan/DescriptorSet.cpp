#include "./DescriptorSet.hpp"

namespace Tortuga
{
namespace Graphics
{
namespace Vulkan
{
namespace DescriptorSet
{
DescriptorSet Create(Device::Device device, DescriptorPool::DescriptorPool pool, DescriptorLayout::DescriptorLayout layout, uint32_t setArrayCount)
{
  DescriptorSet data = {};
  data.Device = device.Device;
  data.Pool = pool;
  data.Layout = layout;
  data.SetArrayCount = setArrayCount;

  VkDescriptorSetAllocateInfo info = {};
  {
    info.sType = VK_STRUCTURE_TYPE_DESCRIPTOR_SET_ALLOCATE_INFO;
    info.descriptorPool = pool.Pool,
    info.descriptorSetCount = setArrayCount;
    info.pSetLayouts = &layout.Layouts;
  }
  ErrorCheck::Callback(vkAllocateDescriptorSets(device.Device, &info, &data.set));
  return data;
}
void UpdateDescriptorSet(DescriptorSet data, std::vector<Buffer::Buffer> content, uint32_t setArrayIndex)
{
  if (data.Layout.BindingsAmount != content.size())
  {
    Console::Error("provided Content does not match this descriptor set size");
    return;
  }

  std::vector<VkDescriptorBufferInfo> bufferInfos(content.size());
  std::vector<VkWriteDescriptorSet> writeInfos(data.Layout.BindingsAmount);
  for (uint32_t i = 0; i < data.Layout.BindingsAmount; i++)
  {
    {
      bufferInfos[i] = {};
      bufferInfos[i].buffer = content[i].Buffer;
      bufferInfos[i].offset = 0;
      bufferInfos[i].range = content[i].Size;
    }

    writeInfos[i].sType = VK_STRUCTURE_TYPE_WRITE_DESCRIPTOR_SET;
    writeInfos[i].dstSet = data.set;
    writeInfos[i].dstBinding = i;
    writeInfos[i].dstArrayElement = setArrayIndex;
    writeInfos[i].descriptorCount = data.SetArrayCount;
    writeInfos[i].descriptorType = data.Layout.Types[i];

    writeInfos[i].pBufferInfo = &(bufferInfos[i]);
    writeInfos[i].pImageInfo = VK_NULL_HANDLE;
    writeInfos[i].pTexelBufferView = VK_NULL_HANDLE;
  }
  vkUpdateDescriptorSets(data.Device, writeInfos.size(), writeInfos.data(), 0, 0);
}

void UpdateDescriptorSet(DescriptorSet data, std::vector<ImageView::ImageView> content, Sampler::Sampler sampler, uint32_t setArrayIndex)
{
  if (data.Layout.BindingsAmount != content.size())
  {
    Console::Error("provided Content does not match this descriptor set size");
    return;
  }

  std::vector<VkDescriptorImageInfo> imageInfo(content.size());
  std::vector<VkWriteDescriptorSet> writeInfos(data.Layout.BindingsAmount);
  for (uint32_t i = 0; i < data.Layout.BindingsAmount; i++)
  {
    {
      imageInfo[i] = {};
      imageInfo[i].imageLayout = VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL;
      imageInfo[i].imageView = content[i].View;
      imageInfo[i].sampler = sampler.Sampler;
    }

    writeInfos[i].sType = VK_STRUCTURE_TYPE_WRITE_DESCRIPTOR_SET;
    writeInfos[i].dstSet = data.set;
    writeInfos[i].dstBinding = i;
    writeInfos[i].dstArrayElement = setArrayIndex;
    writeInfos[i].descriptorCount = data.SetArrayCount;
    writeInfos[i].descriptorType = data.Layout.Types[i];

    writeInfos[i].pBufferInfo = VK_NULL_HANDLE;
    writeInfos[i].pImageInfo = &(imageInfo[i]);
    writeInfos[i].pTexelBufferView = VK_NULL_HANDLE;
  }
  vkUpdateDescriptorSets(data.Device, writeInfos.size(), writeInfos.data(), 0, 0);
}
} // namespace DescriptorSet
} // namespace Vulkan
} // namespace Graphics
} // namespace Tortuga