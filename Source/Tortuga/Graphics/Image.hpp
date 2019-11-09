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
struct Image
{
  uint32_t Width;
  uint32_t Height;
  uint32_t TotalByteSize;
  std::vector<glm::vec4> Pixels;

  Image()
  {
    this->Width = 1;
    this->Height = 1;
    this->TotalByteSize = this->Width * this->Height * sizeof(glm::vec4);
    this->Pixels.resize(this->Width * this->Height);
  }
  Image(uint32_t width, uint32_t height)
  {
    this->Width = width;
    this->Height = height;
    this->TotalByteSize = this->Width * this->Height * sizeof(glm::vec4);
    this->Pixels.resize(this->Width * this->Height);
  }
  Image(Utils::IO::ImageFile image)
  {
    this->Width = image.Width;
    this->Height = image.Height;
    this->TotalByteSize = this->Width * this->Height * sizeof(glm::vec4);
    this->Pixels.resize(this->Width * this->Height);
    const float MAX_COLOR = 256.0f;
    for (uint32_t x = 0; x < this->Width; x++)
    {
      for (uint32_t y = 0; y < this->Height; y++)
      {
        uint32_t i = y * this->Width + x;
        uint32_t j = y * image.Pitch + x * image.BytesPerPixel;
        this->Pixels[i].r = 0;
        this->Pixels[i].g = 0;
        this->Pixels[i].b = 0;
        this->Pixels[i].a = 1;
        if (image.BytesPerPixel == 1)
        {
          this->Pixels[i].r = (float)image.Pixels[j + 0] / MAX_COLOR;
        }
        else if (image.BytesPerPixel == 2)
        {
          this->Pixels[i].r = (float)image.Pixels[j + 0] / MAX_COLOR;
          this->Pixels[i].g = (float)image.Pixels[j + 1] / MAX_COLOR;
        }
        else if (image.BytesPerPixel == 3)
        {
          this->Pixels[i].r = (float)image.Pixels[j + 0] / MAX_COLOR;
          this->Pixels[i].g = (float)image.Pixels[j + 1] / MAX_COLOR;
          this->Pixels[i].b = (float)image.Pixels[j + 2] / MAX_COLOR;
        }
        else if (image.BytesPerPixel == 4)
        {
          this->Pixels[i].r = (float)image.Pixels[j + 0] / MAX_COLOR;
          this->Pixels[i].g = (float)image.Pixels[j + 1] / MAX_COLOR;
          this->Pixels[i].b = (float)image.Pixels[j + 2] / MAX_COLOR;
          this->Pixels[i].a = (float)image.Pixels[j + 1] / MAX_COLOR;
        }
      }
    }
  }
};
} // namespace Graphics
} // namespace Tortuga

#endif