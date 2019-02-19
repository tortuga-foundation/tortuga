#include "FrameBuffers.h"

namespace Tortuga
{
namespace Graphics
{
namespace VulkanAPI
{
FrameBufferData CreateFrameBuffers(DeviceData device, RenderPassData renderPass, std::vector<VkImageView> imageViews, uint32_t width, uint32_t height)
{
  auto data = FrameBufferData();
  data.Device = device.Device;

  auto frameBufferInfo = VkFramebufferCreateInfo();
  {
    frameBufferInfo.sType = VK_STRUCTURE_TYPE_FRAMEBUFFER_CREATE_INFO;
    frameBufferInfo.renderPass = renderPass.RenderPass;
    frameBufferInfo.attachmentCount = imageViews.size();
    frameBufferInfo.pAttachments = imageViews.data();
    frameBufferInfo.width = width;
    frameBufferInfo.height = height;
    frameBufferInfo.layers = 1;
  }
  if (vkCreateFramebuffer(device.Device, &frameBufferInfo, nullptr, &data.FrameBuffers) != VK_SUCCESS)
  {
    Console::Fatal("Failed to create framebuffer on device: {0}", Console::Arguments() << device.Properties.deviceName);
  }

  return data;
}
void DestroyFrameBuffers(FrameBufferData data)
{
  vkDestroyFramebuffer(data.Device, data.FrameBuffers, nullptr);
}
}; // namespace VulkanAPI
}; // namespace Graphics
}; // namespace Tortuga