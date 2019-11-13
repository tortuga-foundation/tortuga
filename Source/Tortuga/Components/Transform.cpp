#include "Transform.hpp"

#include <glm/glm.hpp>
#include <glm/gtc/matrix_transform.hpp>

#include "../Core/Engine.hpp"
#include "../Graphics/Vulkan/Buffer.hpp"
#include "../Graphics/Vulkan/CommandPool.hpp"
#include "../Graphics/Vulkan/Command.hpp"

namespace Tortuga
{
namespace Components
{
void Transform::OnCreate()
{
    const auto device = Core::Engine::GetPrimaryVulkanDevice();
    //buffer
    this->TransferStagingBuffer = Graphics::Vulkan::Buffer::CreateHost(device, sizeof(glm::mat4), VK_BUFFER_USAGE_TRANSFER_SRC_BIT);
    this->TransferBuffer = Graphics::Vulkan::Buffer::CreateDevice(device, this->TransferStagingBuffer.Size, VK_BUFFER_USAGE_TRANSFER_DST_BIT);
    //transfer
    this->TransferCommandPool = Graphics::Vulkan::CommandPool::Create(device, device.QueueFamilies.Transfer.Index);
    this->TransferCommand = Graphics::Vulkan::Command::Create(device, this->TransferCommandPool, Graphics::Vulkan::Command::PRIMARY);
    //record command
    Graphics::Vulkan::Command::Begin(this->TransferCommand, VK_COMMAND_BUFFER_USAGE_SIMULTANEOUS_USE_BIT);
    Graphics::Vulkan::Command::CopyBuffer(this->TransferCommand, this->TransferStagingBuffer, this->TransferBuffer);
    Graphics::Vulkan::Command::End(this->TransferCommand);
    //update buffer
    const auto model = GetModelMatrix();
    Graphics::Vulkan::Buffer::SetData(this->TransferStagingBuffer, &model, sizeof(glm::mat4));
    Graphics::Vulkan::Command::Submit({this->TransferCommand}, device.Queues.Transfer[0]);
    IsBuffersCreated = true;
}
void Transform::OnDestroy()
{
    IsBuffersCreated = false;
    Graphics::Vulkan::Buffer::Destroy(this->TransferStagingBuffer);
    Graphics::Vulkan::Buffer::Destroy(this->TransferBuffer);
    Graphics::Vulkan::CommandPool::Destroy(this->TransferCommandPool);
}
void Transform::UpdateBuffers()
{
    if (!IsBuffersCreated)
        return;
    const auto model = GetModelMatrix();
    const auto device = Core::Engine::GetPrimaryVulkanDevice();
    Graphics::Vulkan::Buffer::SetData(this->TransferStagingBuffer, &model, sizeof(glm::mat4));
    Graphics::Vulkan::Command::Submit({this->TransferCommand}, device.Queues.Transfer[0]);
}

glm::vec3 Transform::GetPosition()
{
    return this->Position;
}
glm::vec4 Transform::GetRotation()
{
    return this->Rotation;
}
glm::vec3 Transform::GetScale()
{
    return this->Scale;
}
void Transform::SetPosition(glm::vec3 pos)
{
    this->Position = pos;
    this->UpdateBuffers();
}
void Transform::SetRotation(glm::vec4 rot)
{
    this->Rotation = rot;
    this->UpdateBuffers();
}
void Transform::SetScale(glm::vec3 sca)
{
    this->Scale = sca;
    this->UpdateBuffers();
}
glm::mat4 Transform::GetModelMatrix()
{
    glm::mat4 transform = glm::mat4(1.0);
    transform = glm::translate(transform, Position);
    transform = glm::rotate(transform, Rotation.x, glm::vec3(Rotation.w, 0, 0));
    transform = glm::rotate(transform, Rotation.y, glm::vec3(0, Rotation.w, 0));
    transform = glm::rotate(transform, Rotation.z, glm::vec3(0, 0, Rotation.w));
    transform = glm::scale(transform, Scale);
    return transform;
}
glm::vec3 Transform::GetForward()
{
    glm::mat4 transform = glm::mat4(1.0);
    transform = glm::rotate(transform, Rotation.x, glm::vec3(Rotation.w, 0, 0));
    transform = glm::rotate(transform, Rotation.y, glm::vec3(0, Rotation.w, 0));
    transform = glm::rotate(transform, Rotation.z, glm::vec3(0, 0, Rotation.w));
    return glm::normalize(glm::vec3(glm::inverse(transform)[2]));
}
} // namespace Components
} // namespace Tortuga
