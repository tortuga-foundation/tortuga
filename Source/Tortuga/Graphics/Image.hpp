#ifndef _GRAPHICS_IMAGE
#define _GRAPHICS_IMAGE

#include <cstdint>
#include <vector>
#include <vulkan/vulkan.h>

#include "./Pixel.hpp"

namespace Tortuga
{
namespace Graphics
{
struct Image
{
  uint32_t Width;
  uint32_t Height;
  uint32_t Channels;
  uint32_t TotalByteSize;
  void *Pixels = nullptr;

  Image()
  {
  }
  Image(uint32_t width, uint32_t height)
  {
    Width = width;
    Height = height;
    Channels = 4;
    TotalByteSize = width * height * Channels;
    Pixels = malloc(sizeof(TotalByteSize));
  }
  ~Image()
  {
    free(Pixels);
  }
};
} // namespace Graphics
} // namespace Tortuga

#endif