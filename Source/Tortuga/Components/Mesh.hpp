#ifndef _MESH_COMPONENT
#define _MESH_COMPONENT

#include <vector>
#include <cstring>
#include <glm/glm.hpp>

#include "../Core/Engine.hpp"
#include "../Graphics/Vertex.hpp"
#include "../Utils/IO.hpp"

#include "../Graphics/Vulkan/Buffer.hpp"
#include "../Graphics/Vulkan/CommandPool.hpp"
#include "../Graphics/Vulkan/Command.hpp"

namespace Tortuga
{
namespace Components
{
struct Mesh : public Core::ECS::Component
{
private:
  std::vector<Graphics::Vertex> Vertices;
  std::vector<uint8_t> Indices;
  int8_t ShouldReCreateBuffers = -1;

  //vulkan buffers
  Graphics::Vulkan::Buffer::Buffer VertexStagingBuffer;
  Graphics::Vulkan::Buffer::Buffer VertexBuffer;
  Graphics::Vulkan::Buffer::Buffer IndexStagingBuffer;
  Graphics::Vulkan::Buffer::Buffer IndexBuffer;
  //vulkan transfer
  Graphics::Vulkan::CommandPool::CommandPool TransferCommandPool;
  Graphics::Vulkan::Command::Command TransferVertexCommand;
  Graphics::Vulkan::Command::Command TransferIndexCommand;

public:
  enum BufferCreationType
  {
    BUFFER_CREATION_ALL = 0,
    BUFFER_CREATION_VERTEX = 1,
    BUFFER_CREATION_INDEX = 2
  };

  void OnCreate();
  void OnDestroy();
  void _ReCreateBuffers(); //un-safe internal function
  void UpdateBuffers(BufferCreationType type);

  std::vector<Graphics::Vertex> GetVertices();
  std::vector<uint8_t> GetIndices();
  void SetVertices(std::vector<Graphics::Vertex> vertices);
  void SetIndices(std::vector<uint8_t> indices);

  Mesh();
  Mesh(Utils::IO::OBJ obj);
};
} // namespace Components
} // namespace Tortuga

#endif