#ifndef _WAYLAND_MEMORY_POOL
#define _WAYLAND_MEMORY_POOL

#include "./Display.hpp"

namespace Tortuga
{
namespace Graphics
{
namespace DisplayServer
{
namespace Wayland
{
struct MemoryPool
{
  std::string FilePath;
  int32_t fd;
  uint8_t *Memory;
  uint32_t Size;
  wl_shm_pool *Pool;
};
MemoryPool CreatePool(Display wayland, uint32_t maxWidth, uint32_t maxHeight);
void DestroyPool(MemoryPool data);
} // namespace Wayland
} // namespace DisplayServer
} // namespace Graphics
} // namespace Tortuga

#endif