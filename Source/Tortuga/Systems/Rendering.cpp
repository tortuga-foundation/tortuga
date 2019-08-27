#include "./Rendering.hpp"

namespace Tortuga
{
namespace Systems
{
void Rendering::Update()
{
  auto swapchain = Core::Screen::GetSwapchain();
  auto device = Core::Engine::GetMainDevice();

  //wait for previous render to finish before starting a new render process
  if (!Graphics::Vulkan::Fence::IsFenceSignaled(RenderingWaiter))
    return;

  Graphics::Vulkan::Fence::ResetFences({RenderingWaiter});
  //geometry processing
  std::vector<Graphics::Vulkan::Buffer::Buffer> lightBuffers;
  std::vector<Graphics::Vulkan::Buffer::Buffer> meshBuffers;
  std::vector<Graphics::Vulkan::Semaphore::Semaphore> meshSemaphore;
  std::vector<Graphics::Vulkan::Semaphore::Semaphore> lightSemaphore;
  {
    auto geometryPipeline = this->GeometryPipeline;
    auto entities = Core::Entity::GetAllEntities();
    std::vector<std::future<void>> commandThreads(0);
    std::vector<Graphics::Vulkan::Command::Command> meshCommands;
    std::vector<Graphics::Vulkan::Command::Command> lightCommands;
    for (auto entity : entities)
    {
      //process mesh info
      auto mesh = entity->GetComponent<Component::Mesh>();
      if (mesh != nullptr)
      {
        if (mesh->IsEnabled)
        {
          meshBuffers.push_back(mesh->Buffer);
          if (!mesh->IsStatic || !mesh->IsProcessedOnce)
          {
            auto transform = entity->GetComponent<Component::Transform>();
            meshCommands.push_back(mesh->Command);
            commandThreads.push_back(std::async(std::launch::async, [mesh, geometryPipeline, transform] {
              if (transform)
                mesh->ApplyTransformation(transform->Position, transform->Rotation, transform->Scale);
              Graphics::Vulkan::Buffer::SetData(mesh->Staging, &mesh->BufferData, Component::MESH_SIZE);
              Graphics::Vulkan::Command::Begin(mesh->Command, VK_COMMAND_BUFFER_USAGE_ONE_TIME_SUBMIT_BIT);
              Graphics::Vulkan::Command::CopyBuffer(mesh->Command, mesh->Staging, mesh->Buffer);
              Graphics::Vulkan::Command::BindPipeline(mesh->Command, VK_PIPELINE_BIND_POINT_COMPUTE, geometryPipeline, {mesh->DescriptorSets});
              uint32_t computeGroupSize = (mesh->BufferData.VerticesSize / 16) + 1;
              Graphics::Vulkan::Command::Compute(mesh->Command, computeGroupSize, computeGroupSize, 1);
              Graphics::Vulkan::Command::End(mesh->Command);
            }));
            mesh->IsProcessedOnce = true;
          }
        }
      }
      //process light info
      auto light = entity->GetComponent<Component::Light>();
      if (light != nullptr)
      {
        if (light->IsEnabled)
        {
          if (!light->IsStatic || !light->IsProcessed)
          {
            auto transform = entity->GetComponent<Component::Transform>();
            if (transform)
              light->UpdateTransform(glm::vec4(transform->Position.x, transform->Position.y, transform->Position.z, 1.0f), transform->Rotation);
            Graphics::Vulkan::Buffer::SetData(light->Staging, &light->Data, sizeof(Component::LightData));
            lightCommands.push_back(light->Command);
            lightBuffers.push_back(light->Buffer);
            light->IsProcessed = false;
          }
        }
      }
    }
    for (uint32_t i = 0; i < commandThreads.size(); i++)
      commandThreads[i].wait();

    if (meshCommands.size() > 0)
      meshSemaphore.push_back(GeometrySemaphore);
    if (lightCommands.size() > 0)
      lightSemaphore.push_back(LightSemaphore);

    Graphics::Vulkan::Command::Submit(meshCommands, device.Queues.Compute[0], {}, meshSemaphore);
    Graphics::Vulkan::Command::Submit(lightCommands, device.Queues.Transfer[0], {}, lightSemaphore);
  }

  //combine meshes into single buffer
  {
    auto totalSize = meshBuffers.size() * Component::MESH_SIZE;
    if (totalSize <= 0)
      totalSize = 1;
    if (MeshCombineBuffer.Buffer == VK_NULL_HANDLE || totalSize != MeshCombineBuffer.Size)
    {
      //buffer needs to be recreated
      if (MeshCombineBuffer.Buffer != VK_NULL_HANDLE)
        Graphics::Vulkan::Buffer::Destroy(MeshCombineBuffer);

      MeshCombineBuffer = Graphics::Vulkan::Buffer::CreateDeviceOnlyDest(device, totalSize);
      Graphics::Vulkan::DescriptorSet::UpdateDescriptorSets(MeshesDescriptorSet, {MeshCombineBuffer});
    }
    Graphics::Vulkan::Command::Begin(MeshCombineCommand, VK_COMMAND_BUFFER_USAGE_ONE_TIME_SUBMIT_BIT);
    uint32_t offset = 0;
    for (auto meshBuffer : meshBuffers)
    {
      Graphics::Vulkan::Command::CopyBuffer(MeshCombineCommand, meshBuffer, MeshCombineBuffer, 0, offset);
      offset += meshBuffer.Size;
    }
    Graphics::Vulkan::Command::End(MeshCombineCommand);
    Graphics::Vulkan::Command::Submit({MeshCombineCommand}, device.Queues.Transfer[0], meshSemaphore, {MeshCombineSemaphore});
  }

  //combine lights into a single buffer
  {
    auto totalSize = lightBuffers.size() * sizeof(Component::LightData);
    if (totalSize <= 0)
      totalSize = 1;
    
    if (LightCombineBuffer.Buffer == VK_NULL_HANDLE || totalSize != LightCombineBuffer.Size)
    {
      //buffer needs to be recreated
      if (LightCombineBuffer.Buffer != VK_NULL_HANDLE)
        Graphics::Vulkan::Buffer::Destroy(LightCombineBuffer);

      LightCombineBuffer = Graphics::Vulkan::Buffer::CreateHostDest(device, totalSize);
      Graphics::Vulkan::DescriptorSet::UpdateDescriptorSets(LightsDescriptorSet, {LightCombineBuffer});
    }
    Graphics::Vulkan::Command::Begin(LightCombineCommand, VK_COMMAND_BUFFER_USAGE_ONE_TIME_SUBMIT_BIT);
    uint32_t offset = 0;
    for (auto lightBuffer : lightBuffers)
    {
      Graphics::Vulkan::Command::CopyBuffer(LightCombineCommand, lightBuffer, LightCombineBuffer, 0, offset);
      offset += lightBuffer.Size;
    }
    Graphics::Vulkan::Command::End(LightCombineCommand);
    Graphics::Vulkan::Command::Submit({LightCombineCommand}, device.Queues.Transfer[0], lightSemaphore, {LightCombineSemaphore});
  }

  //rendering
  {
    //check if swapchain was resized
    if (SwapchainExtent.width != swapchain.Extent.width || SwapchainExtent.height != swapchain.Extent.height)
    {
      SwapchainExtent = swapchain.Extent;
      UpdateWindowSize(this);
    }

    //submit command
    auto windowWidth = (SwapchainExtent.width / 8) + 1;
    auto windowHeight = (SwapchainExtent.height / 8) + 1;
    Graphics::Vulkan::Command::Begin(RenderingCommand, VK_COMMAND_BUFFER_USAGE_ONE_TIME_SUBMIT_BIT);
    Graphics::Vulkan::Command::BindPipeline(RenderingCommand, VK_PIPELINE_BIND_POINT_COMPUTE, RenderingPipeline, {MeshesDescriptorSet, LightsDescriptorSet, OutRenderingDescriptorSet});
    Graphics::Vulkan::Command::Compute(RenderingCommand, windowWidth, windowHeight, 1);
    Graphics::Vulkan::Command::TransferImageLayout(RenderingCommand, RenderingImage, VK_IMAGE_LAYOUT_UNDEFINED, VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL);
    Graphics::Vulkan::Command::BufferToImage(RenderingCommand, RenderingBuffer, RenderingImage, {0, 0}, {SwapchainExtent.width, SwapchainExtent.height});
    Graphics::Vulkan::Command::End(RenderingCommand);
    Graphics::Vulkan::Command::Submit({RenderingCommand}, Core::Engine::GetMainDevice().Queues.Compute[0], {MeshCombineSemaphore, LightCombineSemaphore}, {RenderingSemaphore});
  }

  //present
  {
    auto index = Graphics::Vulkan::Swapchain::AquireNextImage(swapchain);
    auto presentImage = Graphics::Vulkan::Swapchain::GetImage(swapchain, index);

    {
      Graphics::Vulkan::Command::Begin(PresentCommand, VK_COMMAND_BUFFER_USAGE_ONE_TIME_SUBMIT_BIT);
      Graphics::Vulkan::Command::TransferImageLayout(PresentCommand, RenderingImage, VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL, VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL);
      Graphics::Vulkan::Command::TransferImageLayout(PresentCommand, presentImage, VK_IMAGE_LAYOUT_UNDEFINED, VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL);
      Graphics::Vulkan::Command::BlitImage(PresentCommand, RenderingImage, presentImage, {SwapchainExtent.width, SwapchainExtent.height}, {0, 0}, {0, 0});
      Graphics::Vulkan::Command::TransferImageLayout(PresentCommand, presentImage, VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL, VK_IMAGE_LAYOUT_PRESENT_SRC_KHR);
      Graphics::Vulkan::Command::End(PresentCommand);
    }
    Graphics::Vulkan::Command::Submit({PresentCommand}, device.Queues.Graphics[0], {RenderingSemaphore}, {PresentSemaphore}, RenderingWaiter);
    Graphics::Vulkan::Swapchain::PresentImage(swapchain, index, device.Queues.Present, {PresentSemaphore});
  }
}
Rendering::Rendering()
{
  auto vulkan = Core::Engine::GetVulkan();
  auto device = Core::Engine::GetMainDevice();

  //general
  {
    TransferCommandPool = Graphics::Vulkan::CommandPool::Create(device, device.QueueFamilies.Transfer.Index);
    ComputeCommandPool = Graphics::Vulkan::CommandPool::Create(device, device.QueueFamilies.Compute.Index);
    GraphicsCommandPool = Graphics::Vulkan::CommandPool::Create(device, device.QueueFamilies.Graphics.Index);
    RenderingWaiter = Graphics::Vulkan::Fence::Create(device, true);
  }

  //geometry pipeline
  {
    //shader
    auto geometryShaderCode = Utils::IO::GetFileContents("Shaders/Geometry.comp");
    auto compiledGeometryShaderCode = Graphics::Vulkan::Shader::CompileShader(vulkan, Graphics::Vulkan::Shader::COMPUTE, geometryShaderCode);
    GeometryShader = Graphics::Vulkan::Shader::Create(device, compiledGeometryShaderCode);

    //pipeline
    GeometryPipeline = Graphics::Vulkan::Pipeline::CreateComputePipeline(device, GeometryShader, {}, {Component::GetMeshDescriptorLayout()});
    GeometrySemaphore = Graphics::Vulkan::Semaphore::Create(device);
    LightSemaphore = Graphics::Vulkan::Semaphore::Create(device);
  }

  //combine meshes into single buffer
  {
    MeshCombineBuffer.Buffer = VK_NULL_HANDLE;
    MeshCombineCommand = Graphics::Vulkan::Command::Create(device, TransferCommandPool, Graphics::Vulkan::Command::PRIMARY);
    MeshCombineSemaphore = Graphics::Vulkan::Semaphore::Create(device);
  }

  //combine lights into single buffer
  {
    LightCombineBuffer.Buffer = VK_NULL_HANDLE;
    LightCombineCommand = Graphics::Vulkan::Command::Create(device, TransferCommandPool, Graphics::Vulkan::Command::PRIMARY);
    LightCombineSemaphore = Graphics::Vulkan::Semaphore::Create(device);
  }

  //rendering
  {
    auto swapchain = Core::Screen::GetSwapchain();
    SwapchainExtent = swapchain.Extent;
    RenderingInfoBufferStaging = Graphics::Vulkan::Buffer::CreateHostSrc(device, sizeof(RenderInfo));
    RenderingInfoBuffer = Graphics::Vulkan::Buffer::CreateDeviceOnlyDest(device, sizeof(RenderInfo));

    MeshesDescriptorLayout = Graphics::Vulkan::DescriptorLayout::Create(device, 1);
    LightsDescriptorLayout = Graphics::Vulkan::DescriptorLayout::Create(device, 1);
    OutRenderingDescriptorLayout = Graphics::Vulkan::DescriptorLayout::Create(device, 2);
    RenderingDescriptorPool = Graphics::Vulkan::DescriptorPool::Create(device, {MeshesDescriptorLayout, LightsDescriptorLayout, OutRenderingDescriptorLayout}, 3);
    MeshesDescriptorSet = Graphics::Vulkan::DescriptorSet::Create(device, RenderingDescriptorPool, MeshesDescriptorLayout);
    LightsDescriptorSet = Graphics::Vulkan::DescriptorSet::Create(device, RenderingDescriptorPool, LightsDescriptorLayout);
    OutRenderingDescriptorSet = Graphics::Vulkan::DescriptorSet::Create(device, RenderingDescriptorPool, OutRenderingDescriptorLayout);
    RenderingCommand = Graphics::Vulkan::Command::Create(device, ComputeCommandPool, Graphics::Vulkan::Command::PRIMARY);

    auto shaderCode = Utils::IO::GetFileContents("Shaders/Rendering.comp");
    auto compiledShader = Graphics::Vulkan::Shader::CompileShader(vulkan, Graphics::Vulkan::Shader::COMPUTE, shaderCode);
    RenderingShader = Graphics::Vulkan::Shader::Create(device, compiledShader);
    RenderingPipeline = Graphics::Vulkan::Pipeline::CreateComputePipeline(device, RenderingShader, {}, {MeshesDescriptorLayout, LightsDescriptorLayout, OutRenderingDescriptorLayout});
    RenderingSemaphore = Graphics::Vulkan::Semaphore::Create(device);

    RenderingBuffer.Buffer = VK_NULL_HANDLE;
    RenderingImage.Image = VK_NULL_HANDLE;
    UpdateWindowSize(this);
  }

  //present
  {
    PresentCommand = Graphics::Vulkan::Command::Create(device, GraphicsCommandPool, Graphics::Vulkan::Command::PRIMARY);
    PresentSemaphore = Graphics::Vulkan::Semaphore::Create(device);
  }
}
Rendering::~Rendering()
{
  auto device = Core::Engine::GetMainDevice();

  Graphics::Vulkan::Device::WaitForQueue(device.Queues.Compute[0]);
  Graphics::Vulkan::Device::WaitForQueue(device.Queues.Transfer[0]);
  Graphics::Vulkan::Device::WaitForQueue(device.Queues.Graphics[0]);
  Graphics::Vulkan::Device::WaitForDevice(device);

  //general
  {
    Graphics::Vulkan::CommandPool::Destroy(TransferCommandPool);
    Graphics::Vulkan::CommandPool::Destroy(ComputeCommandPool);
    Graphics::Vulkan::CommandPool::Destroy(GraphicsCommandPool);
    Graphics::Vulkan::Fence::Destroy(RenderingWaiter);
  }

  //geometry pipeline
  {
    Graphics::Vulkan::Pipeline::DestroyPipeline(GeometryPipeline);
    Graphics::Vulkan::Shader::Destroy(GeometryShader);
    Graphics::Vulkan::Semaphore::Destroy(GeometrySemaphore);
    Graphics::Vulkan::Semaphore::Destroy(LightSemaphore);
  }

  //combine meshes into single buffer
  {
    Graphics::Vulkan::Buffer::Destroy(MeshCombineBuffer);
    Graphics::Vulkan::Semaphore::Destroy(MeshCombineSemaphore);
  }

  //combine lights into single buffer
  {
    Graphics::Vulkan::Buffer::Destroy(LightCombineBuffer);
    Graphics::Vulkan::Semaphore::Destroy(LightCombineSemaphore);
  }

  //rendering
  {
    Graphics::Vulkan::Buffer::Destroy(RenderingBuffer);
    Graphics::Vulkan::Buffer::Destroy(RenderingInfoBuffer);
    Graphics::Vulkan::Buffer::Destroy(RenderingInfoBufferStaging);
    Graphics::Vulkan::DescriptorLayout::Destroy(MeshesDescriptorLayout);
    Graphics::Vulkan::DescriptorLayout::Destroy(OutRenderingDescriptorLayout);
    Graphics::Vulkan::DescriptorLayout::Destroy(LightsDescriptorLayout);
    Graphics::Vulkan::DescriptorPool::Destroy(RenderingDescriptorPool);
    Graphics::Vulkan::Shader::Destroy(RenderingShader);
    Graphics::Vulkan::Pipeline::DestroyPipeline(RenderingPipeline);
    Graphics::Vulkan::Semaphore::Destroy(RenderingSemaphore);
    Graphics::Vulkan::Image::Destroy(RenderingImage);
  }

  //present
  {
    Graphics::Vulkan::Semaphore::Destroy(PresentSemaphore);
  }
}

void Rendering::UpdateWindowSize(Rendering *render)
{
  auto device = Core::Engine::GetMainDevice();

  RenderInfo renderInfo = {};
  {
    renderInfo.WindowWidth = render->SwapchainExtent.width;
    renderInfo.WindowHeight = render->SwapchainExtent.height;
  }
  Graphics::Vulkan::Buffer::SetData(render->RenderingInfoBufferStaging, &renderInfo, sizeof(renderInfo));
  auto renderInfoTransfer = Graphics::Vulkan::Command::Create(device, render->TransferCommandPool, Graphics::Vulkan::Command::PRIMARY);
  Graphics::Vulkan::Command::Begin(renderInfoTransfer, VK_COMMAND_BUFFER_USAGE_ONE_TIME_SUBMIT_BIT);
  Graphics::Vulkan::Command::CopyBuffer(renderInfoTransfer, render->RenderingInfoBufferStaging, render->RenderingInfoBuffer);
  Graphics::Vulkan::Command::End(renderInfoTransfer);
  Graphics::Vulkan::Command::Submit({renderInfoTransfer}, device.Queues.Transfer[0]);
  Graphics::Vulkan::Device::WaitForQueue(device.Queues.Transfer[0]);

  if (render->RenderingBuffer.Buffer != VK_NULL_HANDLE)
    Graphics::Vulkan::Buffer::Destroy(render->RenderingBuffer);
  render->RenderingBuffer = Graphics::Vulkan::Buffer::CreateDeviceOnly(device, sizeof(glm::vec4) * render->SwapchainExtent.width * render->SwapchainExtent.height);

  Graphics::Vulkan::DescriptorSet::UpdateDescriptorSets(render->OutRenderingDescriptorSet, {render->RenderingInfoBuffer, render->RenderingBuffer});

  if (render->RenderingImage.Image != VK_NULL_HANDLE)
    Graphics::Vulkan::Image::Destroy(render->RenderingImage);
  render->RenderingImage = Graphics::Vulkan::Image::Create(device, render->SwapchainExtent.width, render->SwapchainExtent.height);
}
} // namespace Systems
} // namespace Tortuga