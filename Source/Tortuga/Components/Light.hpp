#ifndef _LIGHT_COMPONENT
#define _LIGHT_COMPONENT

#include <glm/glm.hpp>

#include "../Core/Engine.hpp"
#include "../Core/ECS/Entity.hpp"
#include "../Graphics/Vulkan/Buffer.hpp"
#include "../Graphics/Vulkan/CommandPool.hpp"
#include "../Graphics/Vulkan/Command.hpp"

namespace Tortuga
{
namespace Components
{
enum LightType
{
  POINT = 0,
  DIRECTIONAL = 1
};
struct Light : Core::ECS::Component
{
private:
  LightType Type = LightType::POINT;
  glm::vec4 Color = glm::vec4(1, 1, 1, 1);
  float Intensity = 1.0f;
  float Range = 10.0f;

  //vulkan buffers
  Graphics::Vulkan::Buffer::Buffer LightStagingBuffer;
  Graphics::Vulkan::Buffer::Buffer LightBuffer;
  
  //transfer
  Graphics::Vulkan::CommandPool::CommandPool TransferCommandPool;
  Graphics::Vulkan::Command::Command TransferCommand;

public:
  Light();

  void OnCreate();
  void OnDestroy();

  LightType GetType();
  void SetType(LightType type);
  glm::vec4 GetColor();
  void SetColor(glm::vec4 color);
  float GetIntensity();
  void SetIntensity(float intensity);
  float GetRange();
  void SetRange(float range);
};
} // namespace Components
} // namespace Tortuga

#endif