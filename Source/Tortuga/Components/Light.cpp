#include "./Light.hpp"

#include "./Transform.hpp"

namespace Tortuga
{
namespace Components
{
struct LightInfo
{
    glm::vec4 position;
    glm::vec4 forward;
    glm::vec4 color;
    uint32_t type;
    float intensity;
    float range;
};
Light::Light()
{
}

void Light::OnCreate()
{
    const auto device = Core::Engine::GetPrimaryVulkanDevice();

    //create buffers
    this->LightStagingBuffer = Graphics::Vulkan::Buffer::CreateHost(device, sizeof(LightInfo), VK_BUFFER_USAGE_TRANSFER_SRC_BIT);
    this->LightBuffer = Graphics::Vulkan::Buffer::CreateDevice(device, this->LightStagingBuffer.Size, VK_BUFFER_USAGE_TRANSFER_DST_BIT | VK_BUFFER_USAGE_UNIFORM_BUFFER_BIT);
    this->TransferCommandPool = Graphics::Vulkan::CommandPool::Create(device, device.QueueFamilies.Transfer.Index);
    this->TransferCommand = Graphics::Vulkan::Command::Create(device, this->TransferCommandPool, Graphics::Vulkan::Command::PRIMARY);

    //record commands
    Graphics::Vulkan::Command::Begin(this->TransferCommand, VK_COMMAND_BUFFER_USAGE_SIMULTANEOUS_USE_BIT);
    Graphics::Vulkan::Command::CopyBuffer(this->TransferCommand, this->LightStagingBuffer, this->LightBuffer);
    Graphics::Vulkan::Command::End(this->TransferCommand);

    //update buffers
    LightInfo info;
    const auto transform = Core::Engine::GetComponent<Transform>(this->Root);
    if (transform)
    {
        info.position = glm::vec4(transform->GetPosition(), 1.0f);
        info.forward = glm::vec4(transform->GetForward(), 1.0f);
    }
    info.color = this->Color;
    info.type = this->Type;
    info.intensity = this->Intensity;
    info.range = this->Range;
    Graphics::Vulkan::Buffer::SetData(this->LightStagingBuffer, &info, sizeof(LightInfo));
    Graphics::Vulkan::Command::Submit({this->TransferCommand}, device.Queues.Transfer[0]);
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