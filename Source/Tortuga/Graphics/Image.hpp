#ifndef _GRAPHICS_IMAGE
#define _GRAPHICS_IMAGE

#include <cstdint>
#include <vector>
#include <vulkan/vulkan.h>

#include "../Utils/IO.hpp"

namespace Tortuga
{
namespace Graphics
{
struct Pixel 
{
  uint r;
  uint g;
  uint b;
  uint a;
};
struct Image
{
  uint32_t Width;
  uint32_t Height;
  uint32_t TotalByteSize;
  std::vector<Pixel> Pixels;

  Image()
  {
    this->Width = 1;
    this->Height = 1;
    this->TotalByteSize = this->Width * this->Height * sizeof(Pixel);
    this->Pixels.resize(this->Width * this->Height);
  }
  Image(uint32_t width, uint32_t height)
  {
    this->Width = width;
    this->Height = height;
    this->TotalByteSize = this->Width * this->Height * sizeof(Pixel);
    this->Pixels.resize(this->Width * this->Height);
  }
  Image(Utils::IO::ImageFile image)
  {
    this->Width = image.Width;
    this->Height = image.Height;
    this->TotalByteSize = this->Width * this->Height * sizeof(Pixel);
    this->Pixels.resize(this->Width * this->Height);
    for (uint32_t i = 0; i < this->Pixels.size(); i++)
    {
      uint32_t j = i * image.Channels;
      if (image.Channels == 1)
      {
        this->Pixels[i].r = image.Pixels[i + 0];
      }
      else if (image.Channels == 2)
      {
        this->Pixels[i].r = image.Pixels[i + 0];
        this->Pixels[i].g = image.Pixels[i + 1];
      }
      else if (image.Channels == 3)
      {
        this->Pixels[i].r = image.Pixels[i + 0];
        this->Pixels[i].g = image.Pixels[i + 1];
        this->Pixels[i].b = image.Pixels[i + 2];
      }
      else if (image.Channels == 4)
      {
        this->Pixels[i].r = image.Pixels[i + 0];
        this->Pixels[i].g = image.Pixels[i + 1];
        this->Pixels[i].b = image.Pixels[i + 2];
        this->Pixels[i].a = image.Pixels[i + 1];
      }
    }
  }
};
} // namespace Graphics
} // namespace Tortuga

#endif