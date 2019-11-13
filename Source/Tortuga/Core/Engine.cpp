#include "./Engine.hpp"

#include <unordered_map>
#include "./Console.hpp"

namespace Tortuga
{
namespace Core
{
namespace Engine
{
struct Engine
{
  std::unordered_map<std::type_index, ECS::System *> Systems;
  std::vector<ECS::Entity *> entities;
  std::unordered_map<std::type_index, std::vector<ECS::Component *>> Components;
  Graphics::Vulkan::Instance::Instance VulkanInstance;
  uint32_t RenderingDevice;
  std::vector<Graphics::Vulkan::DescriptorLayout::DescriptorLayout> DescriptorLayouts;

  Engine() {}
  ~Engine()
  {
    for (const auto device : this->VulkanInstance.Devices)
      Graphics::Vulkan::Device::WaitForDevice(device);

    for (const auto entity : entities)
      delete entity;

    for (auto i = Systems.begin(); i != Systems.end(); ++i)
      delete i->second;

    //destroy all descriptor layouts
    for (const auto layout : this->DescriptorLayouts)
      Graphics::Vulkan::DescriptorLayout::Destroy(layout);
    //descriptor vulkan instance
    Graphics::Vulkan::Instance::Destroy(this->VulkanInstance);
  }
};
Engine *engine = nullptr;
void Create()
{
  engine = new Engine();
  //start up vulkan
  engine->VulkanInstance = Graphics::Vulkan::Instance::Create();
  //select a gpu
  engine->RenderingDevice = 0;
  //setup descriptor layouts
  const auto device = GetPrimaryVulkanDevice();
  //model, view, projection, matrix
  engine->DescriptorLayouts.push_back(Graphics::Vulkan::DescriptorLayout::Create(device, {VK_SHADER_STAGE_VERTEX_BIT, VK_SHADER_STAGE_VERTEX_BIT, VK_SHADER_STAGE_VERTEX_BIT}, {VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER, VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER, VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER}));
  //light infos
  engine->DescriptorLayouts.push_back(Graphics::Vulkan::DescriptorLayout::Create(device, {VK_SHADER_STAGE_VERTEX_BIT | VK_SHADER_STAGE_FRAGMENT_BIT}, {VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER}));
  //material albedo, normal, detail1 texture
  engine->DescriptorLayouts.push_back(Graphics::Vulkan::DescriptorLayout::Create(device, {VK_SHADER_STAGE_FRAGMENT_BIT, VK_SHADER_STAGE_FRAGMENT_BIT, VK_SHADER_STAGE_FRAGMENT_BIT}, {VK_DESCRIPTOR_TYPE_SAMPLED_IMAGE, VK_DESCRIPTOR_TYPE_SAMPLED_IMAGE, VK_DESCRIPTOR_TYPE_SAMPLED_IMAGE}));
}
void Destroy()
{
  delete engine;
}
uint32_t GetPrimaryVulkanDeviceIndex()
{
  return engine->RenderingDevice;
}
Graphics::Vulkan::Device::Device GetPrimaryVulkanDevice()
{
  return engine->VulkanInstance.Devices[engine->RenderingDevice];
}
Graphics::Vulkan::Instance::Instance GetVulkanInstance()
{
  return engine->VulkanInstance;
}
std::vector<Graphics::Vulkan::DescriptorLayout::DescriptorLayout> GetVulkanDescriptorLayouts()
{
  return engine->DescriptorLayouts;
}
//systems
void AddSystem(std::type_index type, ECS::System *data)
{
  if (engine == nullptr)
  {
    Console::Error("You need to create an engine first");
    return;
  }

  if (engine->Systems.find(type) != engine->Systems.end())
    return;

  engine->Systems.insert(std::pair(type, data));
}
void RemoveSystem(std::type_index type)
{
  if (engine == nullptr)
  {
    Console::Error("You need to create an engine first");
    return;
  }

  if (engine->Systems.find(type) == engine->Systems.end())
    return;
  delete engine->Systems[type];
  engine->Systems.erase(type);
}
ECS::System *GetSystem(std::type_index type)
{
  if (engine == nullptr)
  {
    Console::Error("You need to create an engine first");
    return nullptr;
  }

  if (engine->Systems.find(type) == engine->Systems.end())
    return nullptr;
  return engine->Systems[type];
}
void IterateSystems()
{
  if (engine == nullptr)
  {
    Console::Error("You need to create an engine first");
    return;
  }

  for (auto i = engine->Systems.begin(); i != engine->Systems.end(); ++i)
    i->second->Update();
}
//entity
ECS::Entity *CreateEntity()
{
  if (engine == nullptr)
  {
    Console::Error("You need to create an engine first");
    return nullptr;
  }

  auto data = new ECS::Entity();
  engine->entities.push_back(data);
  return data;
}
void DestroyEntity(ECS::Entity *entity)
{
  if (engine == nullptr)
  {
    Console::Error("You need to create an engine first");
    return;
  }

  for (auto i = engine->entities.begin(); i != engine->entities.end(); ++i)
  {
    if ((*i) == entity)
    {
      for (auto j = entity->Components.begin(); j != entity->Components.end(); ++j)
      {
        auto comp = engine->Components[(*j).first];
        for (auto k = comp.begin(); k != comp.end(); ++k)
        {
          if ((*k)->Root == entity)
          {
            comp.erase(k);
            engine->Components[(*j).first] = comp;
            break;
          }
        }
      }
      engine->entities.erase(i);
      delete entity;
      break;
    }
  }
}
void AddComponent(ECS::Entity *entity, std::type_index type, ECS::Component *data)
{
  if (engine == nullptr)
  {
    Console::Error("You need to create an engine first");
    return;
  }
  if (entity->Components.find(type) != entity->Components.end())
    return;

  data->Root = entity;
  entity->Components.insert(std::pair(type, data));
  if (engine->Components.find(type) == engine->Components.end())
  {
    std::vector<ECS::Component *> dataList = {data};
    engine->Components.insert(std::pair(type, dataList));
  }
  else
  {
    engine->Components[type].push_back(data);
  }
  entity->Components[type]->OnCreate();
}
void RemoveComponent(ECS::Entity *entity, std::type_index type)
{
  if (engine == nullptr)
  {
    Console::Error("You need to create an engine first");
    return;
  }

  if (entity->Components.find(type) == entity->Components.end())
    return;

  entity->Components[type]->OnDestroy();
  delete entity->Components[type];
  entity->Components.erase(type);

  if (engine->Components.find(type) == engine->Components.end())
    return;

  auto comp = engine->Components[type];
  for (auto k = comp.begin(); k != comp.end(); ++k)
  {
    if ((*k)->Root == entity)
    {
      comp.erase(k);
      engine->Components[type] = comp;
      break;
    }
  }
}
ECS::Component *GetComponent(ECS::Entity *entity, std::type_index type)
{
  if (engine == nullptr)
  {
    Console::Error("You need to create an engine first");
    return nullptr;
  }
  if (entity->Components.find(type) == entity->Components.end())
    return nullptr;

  return entity->Components[type];
}
void SetComponent(ECS::Entity *entity, std::type_index type, ECS::Component *data)
{
  if (engine == nullptr)
  {
    Console::Error("You need to create an engine first");
    return;
  }
  if (entity->Components.find(type) == entity->Components.end())
    return;

  entity->Components[type] = data;
}
std::vector<ECS::Component *> GetComponents(std::type_index type)
{
  if (engine == nullptr)
  {
    Console::Error("You need to create an engine first");
    return {};
  }

  return engine->Components[type];
}
} // namespace Engine
} // namespace Core
} // namespace Tortuga