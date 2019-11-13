#ifndef _RENDERING_SYSTEM
#define _RENDERING_SYSTEM

#define MAXIMUM_NUM_OF_LIGHTS 10

#include <future>
#include <thread>
#include <bits/stdc++.h>

#include "../../Core/ECS/Entity.hpp"
#include "../../Core/ECS/System.hpp"
#include "../../Graphics/Vulkan/Instance.hpp"
#include "../../Graphics/DisplaySurface.hpp"
#include "../../Graphics/Vertex.hpp"
#include "../../Core/Engine.hpp"

#include "../../Components/Mesh.hpp"
#include "../../Components/Transform.hpp"
#include "../../Components/Camera.hpp"
#include "../../Components/Light.hpp"
#include "../../Components/Material.hpp"

namespace Tortuga
{
namespace Systems
{
class Rendering : public Core::ECS::System
{
private:
  //general command pools
  Graphics::Vulkan::CommandPool::CommandPool TransferCommandPool;
  Graphics::Vulkan::CommandPool::CommandPool ComputeCommandPool;
  Graphics::Vulkan::CommandPool::CommandPool GraphicsCommandPool;

  //rendering
  Graphics::Vulkan::Command::Command RenderCommand;

public:
  static Rendering *Instance;
  static uint32_t LightsPerMesh;

  Rendering();
  ~Rendering();
  void Update();
};
} // namespace Systems
} // namespace Tortuga

#endif