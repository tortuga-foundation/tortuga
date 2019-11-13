#include "./Rendering.hpp"

namespace Tortuga
{
namespace Systems
{
uint32_t Rendering::LightsPerMesh = 3;
Rendering *Rendering::Instance = nullptr;
Rendering::Rendering()
{
  Rendering::Instance = this;
  //vulkan instance & display surface
  const auto device = Core::Engine::GetPrimaryVulkanDevice();

  //command pools
  TransferCommandPool = Graphics::Vulkan::CommandPool::Create(device, device.QueueFamilies.Transfer.Index);
  ComputeCommandPool = Graphics::Vulkan::CommandPool::Create(device, device.QueueFamilies.Compute.Index);
  GraphicsCommandPool = Graphics::Vulkan::CommandPool::Create(device, device.QueueFamilies.Graphics.Index);
  RenderCommand = Graphics::Vulkan::Command::Create(device, GraphicsCommandPool, Graphics::Vulkan::Command::Type::PRIMARY);
}
Rendering::~Rendering()
{
  Graphics::Vulkan::Device::WaitForDevice(Core::Engine::GetPrimaryVulkanDevice());
  Graphics::Vulkan::CommandPool::Destroy(TransferCommandPool);
  Graphics::Vulkan::CommandPool::Destroy(ComputeCommandPool);
  Graphics::Vulkan::CommandPool::Destroy(GraphicsCommandPool);
}
void Rendering::Update()
{
  const auto device = Core::Engine::GetPrimaryVulkanDevice();

}
} // namespace Systems
} // namespace Tortuga