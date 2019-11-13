#include "./Light.hpp"

namespace Tortuga
{
namespace Components
{
Light::Light()
{
}

void Light::OnCreate()
{
    const auto device = Core::Engine::GetPrimaryVulkanDevice();
    uint32_t ByteSize = sizeof(glm::vec4) + sizeof(float) + sizeof(float) + sizeof(uint32_t);
    this->LightStagingBuffer = Graphics::Vulkan::Buffer::CreateHost(device, ByteSize, VK_BUFFER_USAGE_TRANSFER_SRC_BIT);
    this->LightBuffer = Graphics::Vulkan::Buffer::CreateDevice(device, this->LightStagingBuffer.Size, VK_BUFFER_USAGE_TRANSFER_DST_BIT | VK_BUFFER_USAGE_UNIFORM_BUFFER_BIT);
    this->TransferCommandPool = Graphics::Vulkan::CommandPool::Create(device, device.QueueFamilies.Transfer.Index);
    this->TransferCommand = Graphics::Vulkan::Command::Create(device, this->TransferCommandPool, Graphics::Vulkan::Command::PRIMARY);
    Graphics::Vulkan::Command::Begin(this->TransferCommand, VK_COMMAND_BUFFER_USAGE_SIMULTANEOUS_USE_BIT);
    Graphics::Vulkan::Command::CopyBuffer(this->TransferCommand, this->LightStagingBuffer, this->LightBuffer);
    Graphics::Vulkan::Command::End(this->TransferCommand);
}
void Light::OnDestroy()
{
    Graphics::Vulkan::CommandPool::Destroy(this->TransferCommandPool);
    Graphics::Vulkan::Buffer::Destroy(this->LightBuffer);
    Graphics::Vulkan::Buffer::Destroy(this->LightStagingBuffer);
}

LightType Light::GetType()
{
    return this->Type;
}
void Light::SetType(LightType type)
{
    this->Type = type;
}

glm::vec4 Light::GetColor()
{
    return this->Color;
}
void Light::SetColor(glm::vec4 color)
{
    this->Color = color;
}

float Light::GetIntensity()
{
    return this->Intensity;
}
void Light::SetIntensity(float intensity)
{
    this->Intensity = intensity;
}

float Light::GetRange()
{
    return this->Range;
}
void Light::SetRange(float range)
{
    this->Range = range;
}
} // namespace Components
} // namespace Tortuga