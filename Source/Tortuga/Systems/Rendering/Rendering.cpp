#include "./Rendering.hpp"

namespace Tortuga
{
namespace Systems
{
uint32_t Rendering::LightsPerMesh = 3;
Rendering::Rendering()
{
  //vulkan instance & display surface
  VulkanInstance = Graphics::Vulkan::Instance::Create();
  const auto device = VulkanInstance.Devices[0];
  DisplaySurface = Graphics::DisplaySurface::Create(VulkanInstance, VulkanInstance.Devices[0]);

  //command pools
  TransferCommandPool = Graphics::Vulkan::CommandPool::Create(device, device.QueueFamilies.Transfer.Index);
  ComputeCommandPool = Graphics::Vulkan::CommandPool::Create(device, device.QueueFamilies.Compute.Index);
  GraphicsCommandPool = Graphics::Vulkan::CommandPool::Create(device, device.QueueFamilies.Graphics.Index);

  //setup descriptor set layouts
  //view matrix & projection matrix
  DescriptorLayouts.push_back(Graphics::Vulkan::DescriptorLayout::Create(device, {VK_SHADER_STAGE_VERTEX_BIT}, {VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER}));
  //model matrix
  DescriptorLayouts.push_back(Graphics::Vulkan::DescriptorLayout::Create(device, {VK_SHADER_STAGE_VERTEX_BIT}, {VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER}));
  //light infos
  DescriptorLayouts.push_back(Graphics::Vulkan::DescriptorLayout::Create(device, {VK_SHADER_STAGE_VERTEX_BIT | VK_SHADER_STAGE_FRAGMENT_BIT}, {VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER}));
  //mesh material
  DescriptorLayouts.push_back(Graphics::Vulkan::DescriptorLayout::Create(device, {VK_SHADER_STAGE_FRAGMENT_BIT}, {VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER}));
  //material albedo texture
  DescriptorLayouts.push_back(Graphics::Vulkan::DescriptorLayout::Create(device, {VK_SHADER_STAGE_VERTEX_BIT}, {VK_DESCRIPTOR_TYPE_SAMPLED_IMAGE}));

  //rendering
  {
    //vertex shader
    const auto vertexShader = Graphics::Vulkan::Shader::GetFullShaderCode("Assets/Shaders/pbr.vert");
    const auto compiledVertex = Graphics::Vulkan::Shader::CompileShader(vertexShader.code, vertexShader.location, vertexShader.type);
    Shaders.push_back(Graphics::Vulkan::Shader::Create(device, compiledVertex, VK_SHADER_STAGE_VERTEX_BIT));

    //fragment shader
    const auto fragmentShader = Graphics::Vulkan::Shader::GetFullShaderCode("Assets/Shaders/pbr.frag");
    const auto compiledFragment = Graphics::Vulkan::Shader::CompileShader(fragmentShader.code, fragmentShader.location, fragmentShader.type);
    Shaders.push_back(Graphics::Vulkan::Shader::Create(device, compiledFragment, VK_SHADER_STAGE_FRAGMENT_BIT));
  }
  RenderPass = Graphics::Vulkan::RenderPass::Create(device);
  Pipeline = Graphics::Vulkan::Pipeline::CreateGraphicsPipeline(device, Shaders, RenderPass, Graphics::Vertex::GetBindingDescription(), Graphics::Vertex::GetAttributeDescriptions(), DescriptorLayouts);
  RenderCommand = Graphics::Vulkan::Command::Create(device, GraphicsCommandPool, Graphics::Vulkan::Command::Type::PRIMARY);
}
Rendering::~Rendering()
{
  Graphics::Vulkan::Device::WaitForDevice(VulkanInstance.Devices[0]);

  for (const auto fence : RenderFence)
    Graphics::Vulkan::Fence::Destroy(fence);

  for (const auto semaphore : RenderSemaphore)
    Graphics::Vulkan::Semaphore::Destroy(semaphore);

  for (const auto semaphore : TransferSemaphore)
    Graphics::Vulkan::Semaphore::Destroy(semaphore);

  Graphics::Vulkan::Pipeline::Destroy(Pipeline);
  Graphics::Vulkan::RenderPass::Destroy(RenderPass);

  for (const auto shader : Shaders)
    Graphics::Vulkan::Shader::Destroy(shader);

  for (const auto descriptorLayout : DescriptorLayouts)
    Graphics::Vulkan::DescriptorLayout::Destroy(descriptorLayout);

  Graphics::Vulkan::CommandPool::Destroy(TransferCommandPool);
  Graphics::Vulkan::CommandPool::Destroy(ComputeCommandPool);
  Graphics::Vulkan::CommandPool::Destroy(GraphicsCommandPool);
  Graphics::DisplaySurface::Destroy(DisplaySurface);
  Graphics::Vulkan::Instance::Destroy(VulkanInstance);
}
void Rendering::SetupSemaphores(uint32_t size)
{
  const auto device = VulkanInstance.Devices[0];

  if (size > RenderSemaphore.size())
    RenderSemaphore.resize(size);
  else if (size < RenderSemaphore.size())
  {
    for (uint32_t i = size; i < RenderSemaphore.size(); i++)
      Graphics::Vulkan::Semaphore::Destroy(RenderSemaphore[i]);
    RenderSemaphore.resize(size);
  }
  for (uint32_t i = 0; i < size; i++)
  {
    if (RenderSemaphore[i].Semaphore == VK_NULL_HANDLE)
      RenderSemaphore[i] = Graphics::Vulkan::Semaphore::Create(device);
  }

  if (size > TransferSemaphore.size())
    TransferSemaphore.resize(size);
  else if (size < TransferSemaphore.size())
  {
    for (uint32_t i = size; i < TransferSemaphore.size(); i++)
      Graphics::Vulkan::Semaphore::Destroy(TransferSemaphore[i]);
    TransferSemaphore.resize(size);
  }
  for (uint32_t i = 0; i < size; i++)
  {
    if (TransferSemaphore[i].Semaphore == VK_NULL_HANDLE)
      TransferSemaphore[i] = Graphics::Vulkan::Semaphore::Create(device);
  }
  if (size > TransferGraphicsSemaphore.size())
    TransferGraphicsSemaphore.resize(size);
  else if (size < TransferGraphicsSemaphore.size())
  {
    for (uint32_t i = size; i < TransferGraphicsSemaphore.size(); i++)
      Graphics::Vulkan::Semaphore::Destroy(TransferGraphicsSemaphore[i]);
    TransferGraphicsSemaphore.resize(size);
  }
  for (uint32_t i = 0; i < size; i++)
  {
    if (TransferGraphicsSemaphore[i].Semaphore == VK_NULL_HANDLE)
      TransferGraphicsSemaphore[i] = Graphics::Vulkan::Semaphore::Create(device);
  }

  if (size > RenderFence.size())
    RenderFence.resize(size);
  else if (size < RenderFence.size())
  {
    for (uint32_t i = size; i < RenderFence.size(); i++)
      Graphics::Vulkan::Fence::Destroy(RenderFence[i]);
    RenderFence.resize(size);
  }
  for (uint32_t i = 0; i < size; i++)
  {
    if (RenderFence[i].Fence == VK_NULL_HANDLE)
      RenderFence[i] = Graphics::Vulkan::Fence::Create(device, true);
  }
}
void Rendering::Update()
{
  const auto cameras = Core::Engine::GetComponents<Components::Camera>();
  SetupSemaphores(cameras.size());
  for (const auto fence : RenderFence)
  {
    if (!Graphics::Vulkan::Fence::IsFenceSignaled(fence))
      return;
  }
  Graphics::Vulkan::Fence::ResetFences(RenderFence);

  const auto device = VulkanInstance.Devices[0];
  Graphics::DisplaySurface::Dispatch(DisplaySurface);

  const auto swapchainImageIndex = Graphics::Vulkan::Swapchain::AquireNextImage(DisplaySurface.Swapchain);
  const auto swapchainImage = Graphics::Vulkan::Swapchain::GetImage(DisplaySurface.Swapchain, swapchainImageIndex);

  //make sure every mesh has a mesh view
  const auto meshes = Core::Engine::GetComponents<Components::Mesh>();
  for (const auto mesh : meshes)
  {
    if (Core::Engine::GetComponent<MeshView>(mesh->Root) == nullptr)
      Core::Engine::AddComponent<MeshView>(mesh->Root);
  }
  for (uint32_t cameraIndex = 0; cameraIndex < cameras.size(); cameraIndex++)
  {
    std::vector<Graphics::Vulkan::Command::Command> transferCommands;
    std::vector<Graphics::Vulkan::Command::Command> transferGraphicsCommands;
    std::vector<Graphics::Vulkan::Command::Command> drawCommands;
    const auto camera = cameras[cameraIndex];
    const auto cameraTransform = Core::Engine::GetComponent<Components::Transform>(camera->Root);
    auto cameraView = Core::Engine::GetComponent<CameraView>(camera->Root);
    if (cameraView == nullptr)
    {
      //setup camera view
      Core::Engine::AddComponent<CameraView>(camera->Root);
      cameraView = Core::Engine::GetComponent<CameraView>(camera->Root);
      cameraView->Setup(device, RenderPass, DescriptorLayouts);
      transferCommands.push_back(cameraView->TransferCommand);
    }
    else
    {
      bool isCameraUpdated = false;
      if (cameraTransform != nullptr && !cameraTransform->GetStatic())
      {
        //update view matrix
        glm::mat4 view = cameraTransform->GetModelMatrix();
        view[1][1] *= -1;
        Graphics::Vulkan::Buffer::SetData(cameraView->StagingCameraMatrix, &view, sizeof(glm::mat4));
        isCameraUpdated = true;
      }
      if (camera->GetIsDirty())
      {
        //update projection matrix
        glm::mat4 proj = glm::perspective(camera->GetFieldOfView(), (float)camera->GetResolutionWidth() / (float)camera->GetResolutionHeight(), camera->GetNearClipPlane(), camera->GetFarClipPlane());
        Graphics::Vulkan::Buffer::SetData(cameraView->StagingCameraMatrix, &proj, sizeof(glm::mat4), sizeof(glm::mat4));
        isCameraUpdated = true;
      }
      if (isCameraUpdated)
        transferCommands.push_back(cameraView->TransferCommand);
    }
    //record render commnad
    Graphics::Vulkan::Command::Begin(RenderCommand, VK_COMMAND_BUFFER_USAGE_ONE_TIME_SUBMIT_BIT);
    Graphics::Vulkan::Command::TransferImageLayout(RenderCommand, cameraView->Render.ColorImage, VK_IMAGE_LAYOUT_UNDEFINED, VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL);
    Graphics::Vulkan::Command::TransferImageLayout(RenderCommand, cameraView->Render.DepthImage, VK_IMAGE_LAYOUT_UNDEFINED, VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL);

    //run all draw commands
    Graphics::Vulkan::Command::BeginRenderPass(RenderCommand, RenderPass, cameraView->Render.Framebuffer, cameraView->Render.Framebuffer.Width, cameraView->Render.Framebuffer.Height);

    //draw command
    std::vector<std::future<void>> meshThreads;
    const auto meshViews = Core::Engine::GetComponents<MeshView>();
    for (const auto meshView : meshViews)
    {
      const auto renderCommand = RenderCommand;
      const auto renderPass = RenderPass;
      const auto framebuffer = cameraView->Render.Framebuffer;
      const auto pipeline = Pipeline;
      const auto viewport = camera->GetViewport();
      const auto resolutionWidth = camera->GetResolutionWidth();
      const auto resolutionHeight = camera->GetResolutionHeight();
      const auto descriptorLayouts = DescriptorLayouts;
      const auto cameraDescriptorSet = cameraView->DescriptorSet;
      meshThreads.push_back(std::async(std::launch::async, [device, meshView, descriptorLayouts, renderPass, framebuffer, pipeline, cameraDescriptorSet, viewport, resolutionWidth, resolutionHeight] {
        meshView->Setup(device, descriptorLayouts);
        const auto lights = meshView->GetClosestLights();
        meshView->UpdateLightsBuffer(lights);
        Graphics::Vulkan::Command::Begin(meshView->RenderCommand, VK_COMMAND_BUFFER_USAGE_RENDER_PASS_CONTINUE_BIT, renderPass, 0, framebuffer);
        const auto sets = {
            cameraDescriptorSet,
            meshView->TransformDescriptorSet,
            meshView->LightsDescriptorSet,
            meshView->MaterialDescriptorSet,
            meshView->AlbedoDescriptorSet};
        Graphics::Vulkan::Command::BindPipeline(meshView->RenderCommand, VK_PIPELINE_BIND_POINT_GRAPHICS, pipeline, sets);
        Graphics::Vulkan::Command::SetViewport(meshView->RenderCommand, viewport.x * resolutionWidth, viewport.y * resolutionHeight, viewport.z * resolutionWidth, viewport.w * resolutionHeight);
        Graphics::Vulkan::Command::BindVertexBuffer(meshView->RenderCommand, {meshView->VertexBuffer});
        Graphics::Vulkan::Command::BindIndexBuffer(meshView->RenderCommand, meshView->IndexBuffer);
        Graphics::Vulkan::Command::DrawIndexed(meshView->RenderCommand, meshView->IndexCount);
        Graphics::Vulkan::Command::End(meshView->RenderCommand);
      }));
    }

    for (uint32_t i = 0; i < meshThreads.size(); i++)
      meshThreads[i].wait();
    for (const auto mesh : meshViews)
    {
      drawCommands.push_back(mesh->RenderCommand);
      if (mesh->IsMeshDirty)
        transferCommands.push_back(mesh->MeshTransferCommand);
      if (mesh->IsTransformDirty || !mesh->IsStatic)
        transferCommands.push_back(mesh->TransformTransferCommand);
      if (mesh->IsMaterialDirty)
      {
        transferCommands.push_back(mesh->MaterialTransferCommand);
        transferGraphicsCommands.push_back(mesh->AlbedoTransferCommand);
      }
      transferCommands.push_back(mesh->LightTransferCommand);

      mesh->IsMeshDirty = false;
      mesh->IsTransformDirty = false;
      mesh->IsMaterialDirty = false;
    }
    std::vector<Graphics::Vulkan::Semaphore::Semaphore> renderWaitSemaphores;
    if (transferCommands.size() > 0)
    {
      Graphics::Vulkan::Command::Submit(transferCommands, device.Queues.Transfer[0], {}, {TransferSemaphore[cameraIndex]});
      renderWaitSemaphores.push_back(TransferSemaphore[cameraIndex]);
    }
    if (transferGraphicsCommands.size() > 0)
    {
      Graphics::Vulkan::Command::Submit(transferGraphicsCommands, device.Queues.Graphics[0], {}, {TransferGraphicsSemaphore[cameraIndex]});
      renderWaitSemaphores.push_back(TransferGraphicsSemaphore[cameraIndex]);
    }
    Graphics::Vulkan::Command::ExecuteCommands(RenderCommand, drawCommands);
    Graphics::Vulkan::Command::EndRenderPass(RenderCommand);
    if (camera->GetPresentToScreen())
    {
      Graphics::Vulkan::Command::TransferImageLayout(RenderCommand, cameraView->Render.ColorImage, VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL, VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL);
      Graphics::Vulkan::Command::TransferImageLayout(RenderCommand, swapchainImage, VK_IMAGE_LAYOUT_UNDEFINED, VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL);
      Graphics::Vulkan::Command::BlitImage(RenderCommand, cameraView->Render.ColorImage, swapchainImage, camera->GetViewport());
      Graphics::Vulkan::Command::TransferImageLayout(RenderCommand, swapchainImage, VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL, VK_IMAGE_LAYOUT_PRESENT_SRC_KHR);
    }
    Graphics::Vulkan::Command::End(RenderCommand);
    Graphics::Vulkan::Command::Submit({RenderCommand}, device.Queues.Graphics[0], renderWaitSemaphores, {RenderSemaphore[cameraIndex]}, RenderFence[cameraIndex]);

    camera->SetIsDirty(false);
  }
  Graphics::Vulkan::Swapchain::PresentImage(DisplaySurface.Swapchain, swapchainImageIndex, device.Queues.Present, RenderSemaphore);
}
void Rendering::WaitForDevice()
{
  const auto device = VulkanInstance.Devices[0];
  Graphics::Vulkan::Device::WaitForDevice(device);
}
} // namespace Systems
} // namespace Tortuga