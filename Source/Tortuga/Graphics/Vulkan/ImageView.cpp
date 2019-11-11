#include "./ImageView.hpp"

namespace Tortuga
{
namespace Graphics
{
namespace Vulkan
{
namespace ImageView
{
ImageView Create(Device::Device device, Image::Image image, VkImageAspectFlags aspectFlags)
{
  ImageView data = {};
  data.Device = device.Device;
  data.Image = image.Image;

  VkImageViewCreateInfo createInfo = {};
  {
    createInfo.sType = VK_STRUCTURE_TYPE_IMAGE_VIEW_CREATE_INFO;
    createInfo.image = image.Image;
    createInfo.viewType = VK_IMAGE_VIEW_TYPE_2D;
    createInfo.format = image.Format;
    //components
    createInfo.components.r = VK_COMPONENT_SWIZZLE_IDENTITY;
    createInfo.components.g = VK_COMPONENT_SWIZZLE_IDENTITY;
    createInfo.components.b = VK_COMPONENT_SWIZZLE_IDENTITY;
    createInfo.components.a = VK_COMPONENT_SWIZZLE_IDENTITY;
    //subresource
    createInfo.subresourceRange.aspectMask = aspectFlags;
    createInfo.subresourceRange.baseMipLevel = 0;
    createInfo.subresourceRange.levelCount = image.MipMapLevel;
    createInfo.subresourceRange.baseArrayLayer = 0;
    createInfo.subresourceRange.layerCount = 1;
  }

  ErrorCheck::Callback(vkCreateImageView(device.Device, &createInfo, nullptr, &data.View));
  return data;
}
ImageView Create(VkDevice device, VkImage image, VkFormat format, VkImageAspectFlags aspectFlags)
{
  ImageView data = {};
  data.Device = device;
  data.Image = image;

  VkImageViewCreateInfo createInfo = {};
  {
    createInfo.sType = VK_STRUCTURE_TYPE_IMAGE_VIEW_CREATE_INFO;
    createInfo.image = image;
    createInfo.viewType = VK_IMAGE_VIEW_TYPE_2D;
    createInfo.format = format;
    //components
    createInfo.components.r = VK_COMPONENT_SWIZZLE_IDENTITY;
    createInfo.components.g = VK_COMPONENT_SWIZZLE_IDENTITY;
    createInfo.components.b = VK_COMPONENT_SWIZZLE_IDENTITY;
    createInfo.components.a = VK_COMPONENT_SWIZZLE_IDENTITY;
    //subresource
    createInfo.subresourceRange.aspectMask = aspectFlags;
    createInfo.subresourceRange.baseMipLevel = 0;
    createInfo.subresourceRange.levelCount = 1;
    createInfo.subresourceRange.baseArrayLayer = 0;
    createInfo.subresourceRange.layerCount = 1;
  }

  ErrorCheck::Callback(vkCreateImageView(device, &createInfo, nullptr, &data.View));
  return data;
}
void Destroy(ImageView data)
{
  if (data.View == VK_NULL_HANDLE)
    return;

  vkDestroyImageView(data.Device, data.View, nullptr);
}
} // namespace ImageView
} // namespace Vulkan
} // namespace Graphics
} // namespace Tortuga