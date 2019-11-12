#ifndef _GRAPHICS_IMAGE
#define _GRAPHICS_IMAGE

#include <cstdint>
#include <vector>
#include <vulkan/vulkan.h>
#include <glm/glm.hpp>

#include "../Utils/IO.hpp"

namespace Tortuga
{
namespace Graphics
{
struct Pixel
{
  uint8_t r;
  uint8_t g;
  uint8_t b;
  uint8_t a;

  Pixel() {}
  Pixel(uint8_t r, uint8_t g, uint8_t b, uint8_t a)
  {
    this->r = r;
    this->g = g;
    this->b = b;
    this->a = a;
  }
};
struct Image
{
  enum ChannelType
  {
    CHANNEL_R,
    CHANNEL_G,
    CHANNEL_B,
    CHANNEL_A
  };

  uint32_t Width;
  uint32_t Height;
  uint32_t TotalByteSize;
  std::vector<Pixel> Pixels;

  Image();
  Image(uint32_t width, uint32_t height);
  Image(Utils::IO::ImageFile image);

  static Image Blue();
  static Image White();

  void CopyChannel(Image sourceImage, ChannelType source, ChannelType destination);
};
} // namespace Graphics
} // namespace Tortuga

#endif