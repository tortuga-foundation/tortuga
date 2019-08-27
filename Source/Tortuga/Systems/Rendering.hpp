#ifndef _RENDERING_SYSTEM
#define _RENDERING_SYSTEM

#include <future>
#include <thread>
#include <vulkan/vulkan.h>

#include "../Graphics/Vulkan/DescriptorLayout.hpp"
#include "../Graphics/Vulkan/DescriptorPool.hpp"
#include "../Graphics/Vulkan/DescriptorSet.hpp"
#include "../Graphics/Vulkan/RenderPass.hpp"
#include "../Graphics/Vulkan/Pipeline.hpp"
#include "../Graphics/Vulkan/Framebuffer.hpp"
#include "../Graphics/Vulkan/Buffer.hpp"
#include "../Graphics/Vulkan/CommandPool.hpp"
#include "../Graphics/Vulkan/Command.hpp"
#include "../Graphics/Vulkan/Instance.hpp"
#include "../Graphics/Vulkan/Window.hpp"
#include "../Graphics/Vulkan/Semaphore.hpp"
#include "../Graphics/Vulkan/Fence.hpp"

#include "../Core/System.hpp"
#include "../Core/Screen.hpp"
#include "../Core/Entity.hpp"
#include "../Console.hpp"

#include "../Components/Transform.hpp"
#include "../Components/Mesh.hpp"

namespace Tortuga
{
namespace Systems
{
class Rendering : public Core::System
{
private:
  //general
  Graphics::Vulkan::CommandPool::CommandPool TransferCommandPool;
  Graphics::Vulkan::CommandPool::CommandPool ComputeCommandPool;
  Graphics::Vulkan::CommandPool::CommandPool GraphicsCommandPool;
  
  //graphics pipeline
  Graphics::Vulkan::Shader::Shader VertexShader;
  Graphics::Vulkan::Shader::Shader FragmentShader;
  Graphics::Vulkan::RenderPass::RenderPass RenderPass;
  Graphics::Vulkan::Pipeline::Pipeline Pipeline;
  std::vector<Graphics::Vulkan::Framebuffer::Framebuffer> Framebuffers;
  Graphics::Vulkan::Command::Command Renderer;
  Graphics::Vulkan::Command::Command Transfer;
  Graphics::Vulkan::Fence::Fence RenderFence;
  Graphics::Vulkan::Semaphore::Semaphore RenderSemaphore;
  Graphics::Vulkan::Semaphore::Semaphore PresentSemaphore;

public:
  void Update();
  Rendering();
  ~Rendering();
};
} // namespace Systems
} // namespace Tortuga

#endif