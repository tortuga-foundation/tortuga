#ifdef VK_USE_PLATFORM_WAYLAND_KHR
#ifndef _WAYLAND_SURFACE
#define _WAYLAND_SURFACE

#include <vulkan/vulkan.h>
#include <vulkan/vulkan_wayland.h>

#include <wayland-client.h>
#include <cstring>

#include "../../Core/Console.hpp"
#include "./SurfaceInterface.hpp"

namespace Tortuga
{
namespace Graphics
{
namespace Surface
{
class WaylandSurface : public SurfaceInterface
{
public:
  WaylandSurface();
  ~WaylandSurface();

  VkSurfaceKHR CreateSurface(VkInstance instance);
  void Dispatch();
};
} // namespace Surface
} // namespace Graphics
} // namespace Tortuga

#endif
#endif