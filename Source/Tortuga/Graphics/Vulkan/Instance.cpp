#include "./Instance.hpp"

#include "../DisplaySurface.hpp"

namespace Tortuga
{
namespace Graphics
{
namespace Vulkan
{
namespace Instance
{
Instance Create()
{
  Instance data = {};
  if (SDL_Init(SDL_INIT_VIDEO) != 0)
  {
    Console::Fatal("SDL: failed to init {0}", SDL_GetError());
  }

  //create helper window
  const auto helperWindow = SDL_CreateWindow("tortuga helper", SDL_WINDOWPOS_CENTERED, SDL_WINDOWPOS_CENTERED, 800, 600, SDL_WINDOW_VULKAN);
  uint32_t windowExtensionsCount = 0;
  if (!SDL_Vulkan_GetInstanceExtensions(helperWindow, &windowExtensionsCount, nullptr))
    Console::Fatal("SDL: failed to get window extensions");
  std::vector<const char *> windowExtensions(windowExtensionsCount);
  if (!SDL_Vulkan_GetInstanceExtensions(helperWindow, &windowExtensionsCount, windowExtensions.data()))
    Console::Fatal("SDL: failed to get window extensions");

  std::vector<const char *> extensions = windowExtensions;
  std::vector<const char *> validationLayers = {"VK_LAYER_LUNARG_standard_validation"};
  extensions.push_back(VK_EXT_DEBUG_REPORT_EXTENSION_NAME);
  extensions.push_back(VK_EXT_DEBUG_UTILS_EXTENSION_NAME);

  VkApplicationInfo appInfo = {};
  {
    appInfo.sType = VK_STRUCTURE_TYPE_APPLICATION_INFO;
    appInfo.pNext = nullptr, appInfo.pEngineName = "Tortuga";
    appInfo.engineVersion = VK_MAKE_VERSION(0, 0, 1);
    appInfo.pApplicationName = "Tortuga Application";
    appInfo.applicationVersion = VK_MAKE_VERSION(0, 0, 1);
    appInfo.apiVersion = VK_API_VERSION_1_1;
  }

  VkInstanceCreateInfo createInfo = {};
  {
    createInfo.sType = VK_STRUCTURE_TYPE_INSTANCE_CREATE_INFO;
    createInfo.pApplicationInfo = &appInfo;
    createInfo.enabledLayerCount = validationLayers.size();
    createInfo.ppEnabledLayerNames = validationLayers.data();
    createInfo.enabledExtensionCount = extensions.size();
    createInfo.ppEnabledExtensionNames = extensions.data();
  }
  ErrorCheck::Callback(vkCreateInstance(&createInfo, nullptr, &data.Instance));

  uint32_t deviceCount = 0;
  ErrorCheck::Callback(vkEnumeratePhysicalDevices(data.Instance, &deviceCount, nullptr));
  std::vector<VkPhysicalDevice> physicalDevices(deviceCount);
  ErrorCheck::Callback(vkEnumeratePhysicalDevices(data.Instance, &deviceCount, physicalDevices.data()));
  if (deviceCount <= 0)
    Console::Fatal("Failed to locate any graphics device");

  auto debugReportInfo = VkDebugReportCallbackCreateInfoEXT();
  {
    debugReportInfo.sType = VK_STRUCTURE_TYPE_DEBUG_REPORT_CALLBACK_CREATE_INFO_EXT;
#ifdef DEBUG_MODE
    debugReportInfo.flags = VK_DEBUG_REPORT_INFORMATION_BIT_EXT | VK_DEBUG_REPORT_DEBUG_BIT_EXT | VK_DEBUG_REPORT_PERFORMANCE_WARNING_BIT_EXT | VK_DEBUG_REPORT_ERROR_BIT_EXT | VK_DEBUG_REPORT_WARNING_BIT_EXT;
#else
    debugReportInfo.flags = VK_DEBUG_REPORT_PERFORMANCE_WARNING_BIT_EXT | VK_DEBUG_REPORT_ERROR_BIT_EXT | VK_DEBUG_REPORT_WARNING_BIT_EXT;
#endif
    debugReportInfo.pfnCallback = ErrorCheck::DebugReportCallback;
  }
  ErrorCheck::CreateDebugReportCallback(data.Instance, &debugReportInfo, nullptr, &data.DebugCallbackReport);

  auto debugMessageInfo = VkDebugUtilsMessengerCreateInfoEXT();
  {
    debugMessageInfo.sType = VK_STRUCTURE_TYPE_DEBUG_UTILS_MESSENGER_CREATE_INFO_EXT;
#ifdef DEBUG_MODE
    debugMessageInfo.messageType = VK_DEBUG_UTILS_MESSAGE_TYPE_VALIDATION_BIT_EXT | VK_DEBUG_UTILS_MESSAGE_TYPE_PERFORMANCE_BIT_EXT | VK_DEBUG_UTILS_MESSAGE_TYPE_GENERAL_BIT_EXT;
    debugMessageInfo.messageSeverity = VK_DEBUG_UTILS_MESSAGE_SEVERITY_ERROR_BIT_EXT | VK_DEBUG_UTILS_MESSAGE_SEVERITY_WARNING_BIT_EXT | VK_DEBUG_UTILS_MESSAGE_SEVERITY_VERBOSE_BIT_EXT | VK_DEBUG_UTILS_MESSAGE_SEVERITY_INFO_BIT_EXT;
#else
    debugMessageInfo.messageType = VK_DEBUG_UTILS_MESSAGE_TYPE_VALIDATION_BIT_EXT | VK_DEBUG_UTILS_MESSAGE_TYPE_PERFORMANCE_BIT_EXT;
    debugMessageInfo.messageSeverity = VK_DEBUG_UTILS_MESSAGE_SEVERITY_ERROR_BIT_EXT | VK_DEBUG_UTILS_MESSAGE_SEVERITY_WARNING_BIT_EXT;
#endif
    debugMessageInfo.pfnUserCallback = ErrorCheck::DebugUtilCallback;
  }
  ErrorCheck::CreateDebugUtilsMessengerEXT(data.Instance, &debugMessageInfo, nullptr, &data.DebugUtilsMessenger);

  //create helper window surface
  VkSurfaceKHR helperSurface = VK_NULL_HANDLE;
  if (!SDL_Vulkan_CreateSurface(helperWindow, data.Instance, &helperSurface))
    Console::Fatal("SDL: failed to setup surface, it is required to initialize devices");

  for (uint32_t i = 0; i < deviceCount; i++)
  {
    auto device = Device::Create(helperSurface, physicalDevices[i]);
    if (device.IsDeviceCompatible)
      data.Devices.push_back(device);
  }

  vkDestroySurfaceKHR(data.Instance, helperSurface, nullptr);
  SDL_DestroyWindow(helperWindow);

  return data;
}
void Destroy(Instance data)
{
  if (data.Instance == VK_NULL_HANDLE)
    return;

  for (uint32_t i = 0; i < data.Devices.size(); i++)
    Device::Destroy(data.Devices[i]);

  ErrorCheck::DestroyDebugReportCallbackEXT(data.Instance, data.DebugCallbackReport, nullptr);
  ErrorCheck::DestroyDebugUtilsMessengerEXT(data.Instance, data.DebugUtilsMessenger, nullptr);

  vkDestroyInstance(data.Instance, nullptr);
  SDL_Quit();
}
} // namespace Instance
} // namespace Vulkan
} // namespace Graphics
} // namespace Tortuga