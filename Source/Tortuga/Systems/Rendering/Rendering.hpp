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
#include "../../Graphics/RenderTarget.hpp"

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
  //vulkan instance and surface
  Graphics::Vulkan::Instance::Instance VulkanInstance;
  Graphics::DisplaySurface::DisplaySurface DisplaySurface;

  //general command pools
  Graphics::Vulkan::CommandPool::CommandPool TransferCommandPool;
  Graphics::Vulkan::CommandPool::CommandPool ComputeCommandPool;
  Graphics::Vulkan::CommandPool::CommandPool GraphicsCommandPool;

  //descriptor layouts
  std::vector<Graphics::Vulkan::DescriptorLayout::DescriptorLayout> DescriptorLayouts;

  //rendering
  std::vector<Graphics::Vulkan::Shader::Shader> Shaders;
  Graphics::Vulkan::RenderPass::RenderPass RenderPass;
  Graphics::Vulkan::Pipeline::Pipeline Pipeline;
  Graphics::Vulkan::Command::Command RenderCommand;

  //sync
  std::vector<Graphics::Vulkan::Semaphore::Semaphore> TransferSemaphore;
  std::vector<Graphics::Vulkan::Semaphore::Semaphore> TransferGraphicsSemaphore;
  std::vector<Graphics::Vulkan::Semaphore::Semaphore> RenderSemaphore;
  std::vector<Graphics::Vulkan::Fence::Fence> RenderFence;

  struct LightInfoStruct
  {
    glm::vec4 Position;
    glm::vec4 Forward;
    glm::vec4 Color;
    int32_t Type;
    float Intensity;
    float Range;
  };

  struct MeshView : public Core::ECS::Component
  {
    bool IsStatic;
    bool IsTransformDirty;
    bool IsMeshDirty;
    bool IsMaterialDirty;
    uint32_t IndexCount;
    //general
    Graphics::Vulkan::Device::Device DeviceInUse;
    Graphics::Vulkan::CommandPool::CommandPool GraphicsCommandPool;
    Graphics::Vulkan::CommandPool::CommandPool TransferCommandPool;
    Graphics::Vulkan::DescriptorPool::DescriptorPool DescriptorPool;
    //mesh
    Graphics::Vulkan::Buffer::Buffer StagingVertexBuffer;
    Graphics::Vulkan::Buffer::Buffer VertexBuffer;
    Graphics::Vulkan::Buffer::Buffer StagingIndexBuffer;
    Graphics::Vulkan::Buffer::Buffer IndexBuffer;
    Graphics::Vulkan::Command::Command RenderCommand;
    Graphics::Vulkan::Command::Command MeshTransferCommand;
    //transform
    Graphics::Vulkan::Buffer::Buffer StagingTransformBuffer;
    Graphics::Vulkan::Buffer::Buffer TransformBuffer;
    Graphics::Vulkan::Command::Command TransformTransferCommand;
    Graphics::Vulkan::DescriptorSet::DescriptorSet TransformDescriptorSet;
    //lights
    Graphics::Vulkan::Buffer::Buffer StagingLightsBuffer;
    Graphics::Vulkan::Buffer::Buffer LightsBuffer;
    Graphics::Vulkan::DescriptorSet::DescriptorSet LightsDescriptorSet;
    Graphics::Vulkan::Command::Command LightTransferCommand;
    //material general
    Graphics::Vulkan::Buffer::Buffer StagingMaterialBuffer;
    Graphics::Vulkan::Buffer::Buffer MaterialBuffer;
    Graphics::Vulkan::DescriptorSet::DescriptorSet MaterialDescriptorSet;
    Graphics::Vulkan::Command::Command MaterialTransferCommand;
    //material albedo texture
    Graphics::Vulkan::Buffer::Buffer StagingAlbedoImage;
    Graphics::Vulkan::Image::Image AlbedoImage;
    Graphics::Vulkan::ImageView::ImageView AlbedoImageView;
    Graphics::Vulkan::Sampler::Sampler AlbedoImageSampler;
    Graphics::Vulkan::DescriptorSet::DescriptorSet AlbedoDescriptorSet;
    Graphics::Vulkan::Command::Command AlbedoTransferCommand;

    void Setup(Graphics::Vulkan::Device::Device device, std::vector<Graphics::Vulkan::DescriptorLayout::DescriptorLayout> layouts);
    void OnDestroy();
    std::vector<Components::Light *> GetClosestLights();
    void UpdateLightsBuffer(std::vector<Components::Light *> lights);
  };

  struct CameraView : public Core::ECS::Component
  {
    Graphics::CameraRender::CameraRender Render;
    Graphics::Vulkan::DescriptorPool::DescriptorPool DescriptorPool;
    Graphics::Vulkan::DescriptorSet::DescriptorSet DescriptorSet;
    Graphics::Vulkan::Buffer::Buffer StagingCameraMatrix;
    Graphics::Vulkan::Buffer::Buffer CameraMatrix;
    Graphics::Vulkan::CommandPool::CommandPool TransferCommandPool;
    Graphics::Vulkan::Command::Command TransferCommand;

    void Setup(Graphics::Vulkan::Device::Device device, Graphics::Vulkan::RenderPass::RenderPass renderPass, std::vector<Graphics::Vulkan::DescriptorLayout::DescriptorLayout> descriptorLayouts);
    void OnDestroy();
  };


  struct LightView : public Core::ECS::Component
  {
    Graphics::CameraRender::CameraRender Render;
    
  };

  void SetupSemaphores(uint32_t size);

public:
  static uint32_t LightsPerMesh;

  Rendering();
  ~Rendering();

  void Update();
  void WaitForDevice();
};
} // namespace Systems
} // namespace Tortuga

#endif