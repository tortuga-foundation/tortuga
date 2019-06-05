#ifndef _VULKAN_SWAPCHAIN
#define _VULKAN_SWAPCHAIN

#include "./VulkanDevice.h"
#include "./Window.h"

#include <vulkan/vulkan.h>

namespace Tortuga {
namespace Graphics {
struct SwapChainSupportDetails {
  VkSurfaceCapabilitiesKHR Capabilities;
  std::vector<VkSurfaceFormatKHR> Formats;
  std::vector<VkPresentModeKHR> PresentModes;
};
struct VulkanSwapchain {
  VkSwapchainKHR Swapchain;
  std::vector<VkImage> Images;
  std::vector<VkImageView> ImageViews;
  uint32_t ImageCount;
  VkSurfaceFormatKHR SurfaceFormat;
  VkExtent2D Extent;
  VkPresentModeKHR PresentMode;
  SwapChainSupportDetails SupportDetails;
  VkInstance VulkanInstance;
  VkDevice VirtualDevice;
};

VulkanSwapchain CreateVulkanSwapchain(VulkanDevice device, Window window);
void DestroyVulkanSwapchain(VulkanSwapchain swapchain);
} // namespace Graphics
} // namespace Tortuga

#endif