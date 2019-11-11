#include "./Rendering.hpp"

namespace Tortuga
{
namespace Systems
{
void Rendering::CameraView::Setup(Graphics::Vulkan::Device::Device device, Graphics::Vulkan::RenderPass::RenderPass renderPass, std::vector<Graphics::Vulkan::DescriptorLayout::DescriptorLayout> descriptorLayouts)
{
  const auto camera = Core::Engine::GetComponent<Components::Camera>(this->Root);
  const auto cameraTransform = Core::Engine::GetComponent<Components::Transform>(this->Root);
  this->Render = Graphics::CameraRender::Create(device, renderPass, camera->GetResolutionWidth(), camera->GetResolutionHeight());
  this->StagingCameraMatrix = Graphics::Vulkan::Buffer::CreateHost(device, sizeof(glm::mat4) * 2, VK_BUFFER_USAGE_TRANSFER_SRC_BIT);
  this->CameraMatrix = Graphics::Vulkan::Buffer::CreateDevice(device, sizeof(glm::mat4) * 2, VK_BUFFER_USAGE_UNIFORM_BUFFER_BIT | VK_BUFFER_USAGE_TRANSFER_DST_BIT);
  this->DescriptorPool = Graphics::Vulkan::DescriptorPool::Create(device, descriptorLayouts);
  this->DescriptorSet = Graphics::Vulkan::DescriptorSet::Create(device, this->DescriptorPool, descriptorLayouts[0]);
  Graphics::Vulkan::DescriptorSet::UpdateDescriptorSet(this->DescriptorSet, {this->CameraMatrix});
  glm::mat4 viewMatrix = glm::mat4(1.0);
  glm::mat4 projectionMatrix = glm::mat4(1.0);
  if (cameraTransform != nullptr)
  {
    viewMatrix = cameraTransform->GetModelMatrix();
    viewMatrix[1][1] *= -1;
  }
  projectionMatrix = glm::perspective(camera->GetFieldOfView(), (float)camera->GetResolutionWidth() / (float)camera->GetResolutionHeight(), camera->GetNearClipPlane(), camera->GetFarClipPlane());
  Graphics::Vulkan::Buffer::SetData(this->StagingCameraMatrix, &viewMatrix, sizeof(glm::mat4));
  Graphics::Vulkan::Buffer::SetData(this->StagingCameraMatrix, &projectionMatrix, sizeof(glm::mat4), sizeof(glm::mat4));
  this->TransferCommandPool = Graphics::Vulkan::CommandPool::Create(device, device.QueueFamilies.Transfer.Index);
  this->TransferCommand = Graphics::Vulkan::Command::Create(device, this->TransferCommandPool, Graphics::Vulkan::Command::PRIMARY);
  Graphics::Vulkan::Command::Begin(this->TransferCommand, VK_COMMAND_BUFFER_USAGE_SIMULTANEOUS_USE_BIT);
  Graphics::Vulkan::Command::CopyBuffer(this->TransferCommand, this->StagingCameraMatrix, this->CameraMatrix);
  Graphics::Vulkan::Command::End(this->TransferCommand);
}
void Rendering::CameraView::OnDestroy()
{
  Graphics::Vulkan::Fence::WaitForFences(Rendering::Instance->RenderFence);
  Graphics::CameraRender::Destroy(this->Render);
  Graphics::Vulkan::DescriptorPool::Destroy(this->DescriptorPool);
  Graphics::Vulkan::Buffer::Destroy(this->StagingCameraMatrix);
  Graphics::Vulkan::Buffer::Destroy(this->CameraMatrix);
  Graphics::Vulkan::CommandPool::Destroy(this->TransferCommandPool);
}
} // namespace Systems
} // namespace Tortuga