#ifndef _CORE_ENGINE
#define _CORE_ENGINE

#include <typeindex>
#include <vector>
#include "./ECS/System.hpp"
#include "./ECS/Entity.hpp"
#include "../Graphics/Vulkan/Device.hpp"
#include "../Graphics/Vulkan/Instance.hpp"
#include "../Graphics/Vulkan/RenderPass.hpp"

namespace Tortuga
{
namespace Core
{
namespace Engine
{
void Create();
void Destroy();
uint32_t GetPrimaryVulkanDeviceIndex();
Graphics::Vulkan::Device::Device GetPrimaryVulkanDevice();
Graphics::Vulkan::Instance::Instance GetVulkanInstance();
Graphics::Vulkan::RenderPass::RenderPass GetVulkanRenderPass();
//systems
void AddSystem(std::type_index type, ECS::System *data);
void RemoveSystem(std::type_index type);
ECS::System *GetSystem(std::type_index type);
void IterateSystems();
//entity
ECS::Entity *CreateEntity();
void DestroyEntity(ECS::Entity *entity);
void AddComponent(ECS::Entity *entity, std::type_index type, ECS::Component *data);
void RemoveComponent(ECS::Entity *entity, std::type_index type);
ECS::Component *GetComponent(ECS::Entity *entity, std::type_index type);
void SetComponent(ECS::Entity *entity, std::type_index type, ECS::Component *data);
std::vector<ECS::Component *> GetComponents(std::type_index type);
//templates
template <typename T>
void AddSystem()
{
  auto type = std::type_index(typeid(T));
  auto data = new T();
  AddSystem(type, data);
}
template <typename T>
void RemoveSystem()
{
  auto type = std::type_index(typeid(T));
  RemoveSystem(type);
}
template <typename T>
T *GetSystem()
{
  auto type = std::type_index(typeid(T));
  const auto data = GetSystem(type);
  return static_cast<T *>(data);
}
template <typename T>
void AddComponent(ECS::Entity *entity, T data = T())
{
  auto type = std::type_index(typeid(T));
  auto temp = new T(data);
  AddComponent(entity, type, temp);
}
template <typename T>
void RemoveComponent(ECS::Entity *entity)
{
  auto type = std::type_index(typeid(T));
  RemoveComponent(entity, type);
}
template <typename T>
T *GetComponent(ECS::Entity *entity)
{
  auto type = std::type_index(typeid(T));
  const auto data = GetComponent(entity, type);
  return static_cast<T *>(data);
}
template <typename T>
void SetComponent(ECS::Entity *entity, T data)
{
  auto type = std::type_index(typeid(T));
  T *temp = new T(data);
  SetComponent(entity, type, temp);
}
template <typename T>
std::vector<T *> GetComponents()
{
  auto type = std::type_index(typeid(T));
  const auto temp = GetComponents(type);
  std::vector<T *> data(temp.size());
  for (uint32_t i = 0; i < temp.size(); i++)
    data[i] = static_cast<T *>(temp[i]);

  return data;
}
} // namespace Engine
} // namespace Core
} // namespace Tortuga

#endif