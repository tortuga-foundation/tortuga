#include "./Image.hpp"

namespace Tortuga
{
namespace Graphics
{
namespace Vulkan
{
namespace Image
{
VkFormat FindSupportedFormat(VkPhysicalDevice physicalDevice, const std::vector<VkFormat> &candidates, VkImageTiling tiling, VkFormatFeatureFlags features)
{
  for (VkFormat format : candidates)
  {
    VkFormatProperties props;
    vkGetPhysicalDeviceFormatProperties(physicalDevice, format, &props);

    if (tiling == VK_IMAGE_TILING_LINEAR && (props.linearTilingFeatures & features) == features)
    {
      return format;
    }
    else if (tiling == VK_IMAGE_TILING_OPTIMAL && (props.optimalTilingFeatures & features) == features)
    {
      return format;
    }
  }
  Console::Fatal("failed to find supported format!");
  return VK_FORMAT_UNDEFINED;
}
VkFormat FindDepthFormat(VkPhysicalDevice device)
{
  return FindSupportedFormat(
      device,
      {VK_FORMAT_D32_SFLOAT, VK_FORMAT_D32_SFLOAT_S8_UINT, VK_FORMAT_D24_UNORM_S8_UINT},
      VK_IMAGE_TILING_OPTIMAL,
      VK_FORMAT_FEATURE_DEPTH_STENCIL_ATTACHMENT_BIT);
}

bool HasStencilComponent(VkFormat format)
{
  return format == VK_FORMAT_D32_SFLOAT_S8_UINT || format == VK_FORMAT_D24_UNORM_S8_UINT;
}

Image Create(Device::Device device, uint32_t width, uint32_t height, VkFormat imageFormat, VkImageUsageFlags usageFlags, uint32_t mipMapLevels)
{
  Image data = {};
  data.Device = device.Device;
  data.Width = width;
  data.Height = height;
  data.Format = imageFormat;
  data.MipMapLevel = mipMapLevels;

  //Image handle
  VkImageCreateInfo imageInfo = {};
  {
    imageInfo.sType = VK_STRUCTURE_TYPE_IMAGE_CREATE_INFO;
    imageInfo.pNext = 0;
    imageInfo.flags = 0;
    imageInfo.imageType = VK_IMAGE_TYPE_2D;
    imageInfo.format = imageFormat;
    imageInfo.extent = {width, height, 1};
    imageInfo.mipLevels = mipMapLevels;
    imageInfo.arrayLayers = 1;
    imageInfo.samples = VK_SAMPLE_COUNT_1_BIT;
    imageInfo.tiling = VK_IMAGE_TILING_OPTIMAL;
    imageInfo.usage = usageFlags;
    imageInfo.sharingMode = VK_SHARING_MODE_EXCLUSIVE;
    imageInfo.initialLayout = VK_IMAGE_LAYOUT_UNDEFINED;
  }
  ErrorCheck::Callback(vkCreateImage(device.Device, &imageInfo, nullptr, &data.Image));

  //Memory
  VkMemoryRequirements memoryRequirements = {};
  vkGetImageMemoryRequirements(data.Device, data.Image, &memoryRequirements);

  VkMemoryAllocateInfo allocInfo = {};
  {
    allocInfo.sType = VK_STRUCTURE_TYPE_MEMORY_ALLOCATE_INFO;
    allocInfo.pNext = 0;
    allocInfo.memoryTypeIndex = Device::FindMemoryType(device, memoryRequirements.memoryTypeBits, VK_MEMORY_PROPERTY_DEVICE_LOCAL_BIT);
    allocInfo.allocationSize = memoryRequirements.size;
  }
  ErrorCheck::Callback(vkAllocateMemory(data.Device, &allocInfo, nullptr, &data.Memory));
  ErrorCheck::Callback(vkBindImageMemory(device.Device, data.Image, data.Memory, 0));

  return data;
}
void Destroy(Image data)
{
  if (data.Image == VK_NULL_HANDLE)
    return;

  vkDestroyImage(data.Device, data.Image, nullptr);
  vkFreeMemory(data.Device, data.Memory, nullptr);
}
} // namespace Image
} // namespace Vulkan
} // namespace Graphics
} // namespace Tortuga