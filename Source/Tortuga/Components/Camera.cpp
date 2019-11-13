#include "./Camera.hpp"

namespace Tortuga
{
namespace Components
{
Camera::Camera() {}
Camera::Camera(uint32_t resolutionWidth, uint32_t resolutionHeight)
{
    this->ResolutionWidth = resolutionWidth;
    this->ResolutionHeight = resolutionHeight;
}

void Camera::OnCreate()
{
    const auto device = Core::Engine::GetPrimaryVulkanDevice();
    //rendering
    this->ColorImage = Graphics::Vulkan::Image::Create(device, this->ResolutionWidth, this->ResolutionHeight, VK_FORMAT_R8G8B8A8_UNORM, VK_IMAGE_USAGE_COLOR_ATTACHMENT_BIT | VK_IMAGE_USAGE_TRANSFER_SRC_BIT);
    this->ColorImageView = Graphics::Vulkan::ImageView::Create(device, this->ColorImage, VK_IMAGE_ASPECT_COLOR_BIT);
    this->DepthImage = Graphics::Vulkan::Image::Create(device, this->ResolutionWidth, this->ResolutionHeight, VK_FORMAT_D32_SFLOAT, VK_IMAGE_USAGE_DEPTH_STENCIL_ATTACHMENT_BIT | VK_IMAGE_USAGE_TRANSFER_SRC_BIT);
    this->DepthImageView = Graphics::Vulkan::ImageView::Create(device, this->DepthImage, VK_IMAGE_ASPECT_DEPTH_BIT);
    this->RenderPass = Graphics::Vulkan::RenderPass::Create(device);
    this->Framebuffer = Graphics::Vulkan::Framebuffer::Create(device, this->ResolutionWidth, this->ResolutionHeight, this->RenderPass, {this->ColorImageView, this->DepthImageView});

    //perspective buffers
    this->PerspectiveStagingBuffer = Graphics::Vulkan::Buffer::CreateHost(device, sizeof(glm::mat4), VK_BUFFER_USAGE_TRANSFER_SRC_BIT);
    this->PerspectiveBuffer = Graphics::Vulkan::Buffer::CreateDevice(device, this->PerspectiveStagingBuffer.Size, VK_BUFFER_USAGE_TRANSFER_DST_BIT | VK_BUFFER_USAGE_UNIFORM_BUFFER_BIT);

    //transfer command
    this->TransferCommandPool = Graphics::Vulkan::CommandPool::Create(device, device.QueueFamilies.Transfer.Index);
    this->TransferCommand = Graphics::Vulkan::Command::Create(device, this->TransferCommandPool, Graphics::Vulkan::Command::PRIMARY);
    Graphics::Vulkan::Command::Begin(this->TransferCommand, VK_COMMAND_BUFFER_USAGE_SIMULTANEOUS_USE_BIT);
    Graphics::Vulkan::Command::CopyBuffer(this->TransferCommand, this->PerspectiveStagingBuffer, this->PerspectiveBuffer);
    Graphics::Vulkan::Command::End(this->TransferCommand);

    //update buffers
    const glm::mat4 perspective = glm::perspective(this->FieldOfView, (float)this->ResolutionWidth / (float)this->ResolutionHeight, this->NearClipPlane, this->FarClipPlane);
    Graphics::Vulkan::Buffer::SetData(this->PerspectiveStagingBuffer, &perspective, sizeof(glm::mat4));
    Graphics::Vulkan::Command::Submit({this->TransferCommand}, device.Queues.Transfer[0]);
}
void Camera::OnDestroy()
{
    Graphics::Vulkan::Buffer::Destroy(this->PerspectiveBuffer);
    Graphics::Vulkan::Buffer::Destroy(this->PerspectiveStagingBuffer);
    Graphics::Vulkan::CommandPool::Destroy(this->TransferCommandPool);

    Graphics::Vulkan::Framebuffer::Destroy(this->Framebuffer);
    Graphics::Vulkan::RenderPass::Destroy(this->RenderPass);
    Graphics::Vulkan::ImageView::Destroy(this->DepthImageView);
    Graphics::Vulkan::Image::Destroy(this->DepthImage);
    Graphics::Vulkan::ImageView::Destroy(this->ColorImageView);
    Graphics::Vulkan::Image::Destroy(this->ColorImage);
}

glm::vec4 Camera::GetViewport()
{
    return this->Viewport;
}
void Camera::SetViewport(glm::vec4 viewport)
{
    this->Viewport = viewport;
}
uint32_t Camera::GetResolutionWidth()
{
    return this->ResolutionWidth;
}
uint32_t Camera::GetResolutionHeight()
{
    return this->ResolutionHeight;
}
float Camera::GetFieldOfView()
{
    return this->FieldOfView;
}
void Camera::SetFieldOfView(float f)
{
    this->FieldOfView = f;
}
float Camera::GetNearClipPlane()
{
    return this->NearClipPlane;
}
void Camera::SetNearClipPlay(float f)
{
    this->NearClipPlane = f;
}
float Camera::GetFarClipPlane()
{
    return this->FarClipPlane;
}
void Camera::SetFarClipPlane(float f)
{
    this->FarClipPlane = f;
}
} // namespace Components
} // namespace Tortuga