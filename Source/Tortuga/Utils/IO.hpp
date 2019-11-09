#ifndef _UTILS_INPUT_OUTPUT
#define _UTILS_INPUT_OUTPUT

#include <SDL2/SDL.h>
#include <SDL2/SDL_image.h>
#include <fstream>
#include <string>
#include <vector>
#include <glm/glm.hpp>
#include <cstring>

#include "../Core/Console.hpp"

namespace Tortuga
{
namespace Utils
{
namespace IO
{
struct OBJ
{
  struct Index
  {
    uint32_t Position;
    uint32_t Texture;
    uint32_t Normal;
  };

  std::vector<glm::vec3> Positions;
  std::vector<glm::vec2> Textures;
  std::vector<glm::vec3> Normals;
  std::vector<Index> Indices;
};

struct ImageFile
{
  uint32_t Width;
  uint32_t Height;
  uint32_t BytesPerPixel;
  uint32_t TotalByteSize;
  uint32_t Pitch;
  std::vector<uint8_t> Pixels;
};

OBJ LoadObjFile(std::string filePath);
ImageFile LoadImageFile(std::string filePath);
std::string GetFileContents(std::string filePath);
void SetFileContents(std::string filePath, std::string data);
void SetFileContents(std::string filePath, const char *data, uint32_t size);
} // namespace IO
} // namespace Utils
} // namespace Tortuga

#endif