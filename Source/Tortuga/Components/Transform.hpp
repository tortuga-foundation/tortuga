#ifndef _TRANSFORM_COMPONENT
#define _TRANSFORM_COMPONENT

#include <glm/glm.hpp>
#include <glm/gtc/matrix_transform.hpp>

#include "../Core/Engine.hpp"
#include "../Graphics/Vulkan/Buffer.hpp"
#include "../Graphics/Vulkan/CommandPool.hpp"
#include "../Graphics/Vulkan/Command.hpp"

namespace Tortuga
{
namespace Components
{
struct Transform : public Core::ECS::Component
{
private:
  bool IsBuffersCreated = false;
  glm::vec3 Position = glm::vec3(0, 0, 0);
  glm::vec4 Rotation = glm::vec4(0, 0, 0, 1);
  glm::vec3 Scale = glm::vec3(1, 1, 1);

  //vulkan buffers
  Graphics::Vulkan::Buffer::Buffer TransferStagingBuffer;
  Graphics::Vulkan::Buffer::Buffer TransferBuffer;
  //vukan transfer
  Graphics::Vulkan::CommandPool::CommandPool TransferCommandPool;
  Graphics::Vulkan::Command::Command TransferCommand;

public:
  void OnCreate();
  void OnDestroy();
  void UpdateBuffers();

  glm::vec3 GetPosition();
  glm::vec4 GetRotation();
  glm::vec3 GetScale();
  void SetPosition(glm::vec3 pos);
  void SetRotation(glm::vec4 rot);
  void SetScale(glm::vec3 sca);
  glm::mat4 GetModelMatrix();
  glm::vec3 GetForward();
};
} // namespace Components
} // namespace Tortuga

#endif