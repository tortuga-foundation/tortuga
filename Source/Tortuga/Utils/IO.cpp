#include "./IO.hpp"

namespace Tortuga
{
namespace Utils
{
namespace IO
{
OBJ LoadObjFile(std::string filePath)
{
  auto data = OBJ();
  FILE *file = fopen(filePath.c_str(), "r");
  if (file == NULL)
  {
    printf("Impossible to open the file !\n");
    return data;
  }
  while (true)
  {
    char lineHeader[128];
    // read the first word of the line
    int res = fscanf(file, "%s", lineHeader);
    if (res == EOF)
      break; // EOF = End Of File. Quit the loop.

    // else : parse lineHeader
    if (strcmp(lineHeader, "v") == 0)
    {
      glm::vec3 vertex;
      fscanf(file, "%f %f %f\n", &vertex.x, &vertex.y, &vertex.z);
      data.Positions.push_back(vertex);
    }
    else if (strcmp(lineHeader, "vt") == 0)
    {
      glm::vec2 uv;
      fscanf(file, "%f %f\n", &uv.x, &uv.y);
      data.Textures.push_back(uv);
    }
    else if (strcmp(lineHeader, "vn") == 0)
    {
      glm::vec3 normal;
      fscanf(file, "%f %f %f\n", &normal.x, &normal.y, &normal.z);
      data.Normals.push_back(normal);
    }
    else if (strcmp(lineHeader, "f") == 0)
    {
      std::string vertex1, vertex2, vertex3;
      unsigned int vertexIndex[3], uvIndex[3], normalIndex[3];
      int matches = fscanf(file, "%d/%d/%d %d/%d/%d %d/%d/%d\n", &vertexIndex[0], &uvIndex[0], &normalIndex[0], &vertexIndex[1], &uvIndex[1], &normalIndex[1], &vertexIndex[2], &uvIndex[2], &normalIndex[2]);
      if (matches != 9)
      {
        printf("File can't be read by our simple parser : ( Try exporting with other options\n");
        return data;
      }

      for (uint32_t i = 0; i < 3; i++)
      {
        auto index = OBJ::Index();
        index.Position = vertexIndex[i] - 1;
        index.Texture = uvIndex[i] - 1;
        index.Normal = normalIndex[i] - 1;
        data.Indices.push_back(index);
      }
    }
  }
  return data;
}
ImageFile LoadImageFile(std::string filePath)
{
  ImageFile data = {};
  SDL_Surface *surface = IMG_Load_RW(SDL_RWFromFile(filePath.c_str(), "rb"), 1);
  if (surface == nullptr)
  {
    Console::Error("Failed to load image file {0}", filePath);
    return data;
  }

  data.Width = surface->w;
  data.Height = surface->h;
  data.BytesPerPixel = surface->format->BytesPerPixel;
  data.TotalByteSize = data.BytesPerPixel * data.Width * data.Height;
  data.Pitch = surface->pitch;
  data.Pixels.resize(data.TotalByteSize);
  memcpy(data.Pixels.data(), surface->pixels, data.TotalByteSize);
  SDL_FreeSurface(surface);
  return data;
}
std::string GetFileContents(std::string filePath)
{
  std::ifstream file(filePath, std::ios::ate | std::ios::binary);
  if (!file.is_open())
  {
    Console::Warning("Failed to open file: {0}", Console::Arguments() << filePath);
    return {};
  }

  size_t fileSize = (size_t)file.tellg();
  std::string buffer;
  buffer.resize(fileSize);
  file.seekg(0);
  file.read(buffer.data(), fileSize);
  file.close();
  return buffer;
}
void SetFileContents(std::string filePath, std::string data)
{
  SetFileContents(filePath, data.data(), data.size());
}
void SetFileContents(std::string filePath, const char *data, uint32_t size)
{
  std::ofstream file(filePath, std::ios::ate | std::ios::binary);
  if (!file.is_open())
  {
    Console::Warning("Failed to open file: {0}", Console::Arguments() << filePath);
  }
  file.write(data, size);
  file.close();
}
} // namespace IO
} // namespace Utils
} // namespace Tortuga