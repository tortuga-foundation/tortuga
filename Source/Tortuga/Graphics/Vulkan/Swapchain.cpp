#include "./Swapchain.hpp"

namespace Tortuga
{
namespace Graphics
{
namespace Vulkan
{
namespace Swapchain
{
SwapChainSupportDetails GetSupportDetails(Device::Device device, VkSurfaceKHR surface)
{
  SwapChainSupportDetails data = {};

  ErrorCheck::Callback(vkGetPhysicalDeviceSurfaceCapabilitiesKHR(device.PhysicalDevice, surface, &data.Capabilities));

  uint32_t formatCount;
  ErrorCheck::Callback(vkGetPhysicalDeviceSurfaceFormatsKHR(device.PhysicalDevice, surface, &formatCount, nullptr));
  data.Formats.resize(formatCount);
  ErrorCheck::Callback(vkGetPhysicalDeviceSurfaceFormatsKHR(device.PhysicalDevice, surface, &formatCount, data.Formats.data()));

  uint32_t presentModeCount;
  ErrorCheck::Callback(vkGetPhysicalDeviceSurfacePresentModesKHR(device.PhysicalDevice, surface, &presentModeCount, nullptr));
  data.PresentModes.resize(presentModeCount);
  ErrorCheck::Callback(vkGetPhysicalDeviceSurfacePresentModesKHR(device.PhysicalDevice, surface, &presentModeCount, data.PresentModes.data()));

  return data;
}

VkSurfaceFormatKHR ChooseSurfaceFormat(std::vector<VkSurfaceFormatKHR> formats)
{
  if (formats.size() == 1 && formats[0].format == VK_FORMAT_UNDEFINED)
    return {VK_FORMAT_R8G8B8_UNORM, VK_COLOR_SPACE_SRGB_NONLINEAR_KHR};

  for (uint32_t i = 0; i < formats.size(); i++)
  {
    if (formats[i].format == VK_FORMAT_R8G8B8_UNORM)
      if (formats[i].colorSpace == VK_COLOR_SPACE_SRGB_NONLINEAR_KHR)
        return formats[i];
  }

  return formats[0];
}

VkPresentModeKHR ChoosePresentMode(std::vector<VkPresentModeKHR> presentModes)
{
  VkPresentModeKHR bestMode = VK_PRESENT_MODE_FIFO_KHR;
  for (uint32_t i = 0; i < presentModes.size(); i++)
  {
    if (presentModes[i] == VK_PRESENT_MODE_MAILBOX_KHR)
      return presentModes[i];

    if (presentModes[i] == VK_PRESENT_MODE_IMMEDIATE_KHR)
      bestMode = presentModes[i];
  }
  return bestMode;
}

VkExtent2D ChooseExtent(VkSurfaceCapabilitiesKHR capabilities, uint32_t width, uint32_t height)
{
  if (capabilities.currentExtent.width != std::numeric_limits<uint32_t>::max())
    return capabilities.currentExtent;

  VkExtent2D actualExtent = {width, height};

  actualExtent.width = std::max(capabilities.minImageExtent.width, std::min(capabilities.maxImageExtent.width, actualExtent.width));
  actualExtent.height = std::max(capabilities.minImageExtent.height, std::min(capabilities.maxImageExtent.height, actualExtent.height));

  return actualExtent;
}

Swapchain Create(Device::Device device, uint32_t width, uint32_t height, VkSurfaceKHR surface, VkSwapchainKHR oldSwapchain)
{
  Swapchain data = {};
  data.Device = device.Device;
  data.SupportDetails = GetSupportDetails(device, surface);
  data.SurfaceFormat = ChooseSurfaceFormat(data.SupportDetails.Formats);
  data.PresentMode = ChoosePresentMode(data.SupportDetails.PresentModes);
  data.Extent = ChooseExtent(data.SupportDetails.Capabilities, width, height);

  if (!device.CanPresent) {
    Console::Error("this device does not have present support");
    return data;
  }

  VkBool32 isTransferSupported = false;
  VkBool32 isComputeSupported = false;
  VkBool32 isGraphicsSupported = false;
  ErrorCheck::Callback(vkGetPhysicalDeviceSurfaceSupportKHR(device.PhysicalDevice, device.QueueFamilies.Transfer.Index, surface, &isTransferSupported));
  ErrorCheck::Callback(vkGetPhysicalDeviceSurfaceSupportKHR(device.PhysicalDevice, device.QueueFamilies.Compute.Index, surface, &isComputeSupported));
  ErrorCheck::Callback(vkGetPhysicalDeviceSurfaceSupportKHR(device.PhysicalDevice, device.QueueFamilies.Graphics.Index, surface, &isGraphicsSupported));
  if (!isComputeSupported && !isGraphicsSupported && !isTransferSupported)
    Console::Fatal("This device does not have any present queues");

  data.ImageCount = data.SupportDetails.Capabilities.minImageCount + 1;
  if (data.SupportDetails.Capabilities.maxImageCount > 0)
    if (data.ImageCount > data.SupportDetails.Capabilities.maxImageCount)
      data.ImageCount = data.SupportDetails.Capabilities.maxImageCount;

  VkSwapchainCreateInfoKHR swapchainInfo = {};
  {
    swapchainInfo.sType = VK_STRUCTURE_TYPE_SWAPCHAIN_CREATE_INFO_KHR;
    swapchainInfo.compositeAlpha = VK_COMPOSITE_ALPHA_OPAQUE_BIT_KHR;
    swapchainInfo.surface = surface;
    swapchainInfo.minImageCount = data.ImageCount;
    swapchainInfo.imageFormat = data.SurfaceFormat.format;
    swapchainInfo.imageColorSpace = data.SurfaceFormat.colorSpace;
    swapchainInfo.imageExtent = data.Extent;
    swapchainInfo.imageArrayLayers = 1;
    swapchainInfo.imageUsage = VK_IMAGE_USAGE_COLOR_ATTACHMENT_BIT | VK_IMAGE_USAGE_TRANSFER_DST_BIT;
    swapchainInfo.imageSharingMode = VK_SHARING_MODE_EXCLUSIVE;
    swapchainInfo.preTransform = data.SupportDetails.Capabilities.currentTransform;
    swapchainInfo.presentMode = data.PresentMode;
    swapchainInfo.clipped = true;
    swapchainInfo.oldSwapchain = oldSwapchain;
  }

  ErrorCheck::Callback(vkCreateSwapchainKHR(device.Device, &swapchainInfo, nullptr, &data.Swapchain));

  ErrorCheck::Callback(vkGetSwapchainImagesKHR(device.Device, data.Swapchain, &data.ImageCount, nullptr));
  data.Images.resize(data.ImageCount);
  ErrorCheck::Callback(vkGetSwapchainImagesKHR(device.Device, data.Swapchain, &data.ImageCount, data.Images.data()));

  data.Views.resize(data.ImageCount);
  for (uint32_t i = 0; i < data.ImageCount; i++)
    data.Views[i] = ImageView::Create(data.Device, data.Images[i], data.SurfaceFormat.format, VK_IMAGE_ASPECT_COLOR_BIT);

  //initialize depth buffer
  {
    auto depthFormat = Image::FindDepthFormat(device.PhysicalDevice);
    data.DepthImage = Image::Create(device, data.Extent.width, data.Extent.height, depthFormat, VK_IMAGE_USAGE_DEPTH_STENCIL_ATTACHMENT_BIT);
    data.DepthImageView = ImageView::Create(device, data.DepthImage, VK_IMAGE_ASPECT_DEPTH_BIT);

    auto depthCommandPool = CommandPool::Create(device, device.QueueFamilies.Graphics.Index);
    auto depthCommand = Command::Create(device, depthCommandPool, Command::PRIMARY);
    Command::Begin(depthCommand, VK_COMMAND_BUFFER_USAGE_ONE_TIME_SUBMIT_BIT);
    Command::TransferImageLayout(depthCommand, data.DepthImage, VK_IMAGE_LAYOUT_UNDEFINED, VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL);
    Command::End(depthCommand);
    Command::Submit({depthCommand}, device.Queues.Graphics[0]);
    Device::WaitForQueue(device.Queues.Graphics[0]);
    CommandPool::Destroy(depthCommandPool);
  }

  VkFenceCreateInfo fenceInfo = {};
  {
    fenceInfo.sType = VK_STRUCTURE_TYPE_FENCE_CREATE_INFO;
  }
  ErrorCheck::Callback(vkCreateFence(data.Device, &fenceInfo, nullptr, &data.Fence));

  return data;
}
void Destroy(Swapchain data)
{
  if (data.Swapchain == VK_NULL_HANDLE)
    return;

  vkDestroyFence(data.Device, data.Fence, nullptr);
  vkDestroySwapchainKHR(data.Device, data.Swapchain, nullptr);
  for (uint32_t i = 0; i < data.ImageCount; i++)
    ImageView::Destroy(data.Views[i]);

  ImageView::Destroy(data.DepthImageView);
  Image::Destroy(data.DepthImage);
}
uint32_t AquireNextImage(Swapchain data)
{
  vkResetFences(data.Device, 1, &data.Fence);
  uint32_t imageIndex;
  ErrorCheck::Callback(vkAcquireNextImageKHR(data.Device, data.Swapchain, std::numeric_limits<uint64_t>::max(), VK_NULL_HANDLE, data.Fence, &imageIndex));
  ErrorCheck::Callback(vkWaitForFences(data.Device, 1, &data.Fence, VK_TRUE, std::numeric_limits<uint64_t>::max()));
  return imageIndex;
}
Image::Image GetImage(Swapchain data, uint32_t index)
{
  Image::Image image = {};
  image.Device = data.Device;
  image.Width = data.Extent.width;
  image.Height = data.Extent.height;
  image.Image = data.Images[index];
  image.Memory = VK_NULL_HANDLE;
  image.MipMapLevel = 1;
  return image;
}
void PresentImage(Swapchain data, uint32_t imageIndex, VkQueue Queue, std::vector<Semaphore::Semaphore> waitSemaphores)
{
  std::vector<VkSemaphore> semaphores(waitSemaphores.size());
  for (uint32_t i = 0; i < waitSemaphores.size(); i++)
    semaphores[i] = waitSemaphores[i].Semaphore;

  std::vector<VkSwapchainKHR> swapChains = {data.Swapchain};
  auto presentInfo = VkPresentInfoKHR();
  {
    presentInfo.sType = VK_STRUCTURE_TYPE_PRESENT_INFO_KHR;
    presentInfo.waitSemaphoreCount = semaphores.size();
    presentInfo.pWaitSemaphores = semaphores.data();
    presentInfo.swapchainCount = swapChains.size();
    presentInfo.pSwapchains = swapChains.data();
    presentInfo.pImageIndices = &imageIndex;
  }
  ErrorCheck::Callback(vkQueuePresentKHR(Queue, &presentInfo));
}
} // namespace Swapchain
} // namespace Vulkan
} // namespace Graphics
} // namespace Tortuga