#include "./Material.hpp"

namespace Tortuga
{
namespace Components
{
Material::Material()
{
    this->shouldCompileShaders = true;
    this->VertexShaderPath = "Assets/Shaders/Simple.vert";
    this->FragmentShaderPath = "Assets/Shaders/Simple.frag";
}
Material::Material(std::string vertex, std::string fragment)
{
    this->shouldCompileShaders = true;
    this->VertexShaderPath = "Assets/Shaders/" + vertex;
    this->FragmentShaderPath = "Assets/Shaders/" + fragment;
}
Material::Material(Graphics::Vulkan::Shader::Shader vertex, Graphics::Vulkan::Shader::Shader fragment)
{
    this->VertexShader = vertex;
    this->FragmentShader = fragment;
}
void Material::OnCreate()
{
    const auto device = Core::Engine::GetPrimaryVulkanDevice();
    const auto mipMapLevels = glm::round(glm::max(glm::log(glm::max(this->BaseColor.Width, this->BaseColor.Height)) * 2.0, 1.0));

    //setup descriptor layouts
    this->DescriptorLayouts.clear();
    //mode, view and projection matrix
    this->DescriptorLayouts.push_back(Graphics::Vulkan::DescriptorLayout::Create(device, VK_SHADER_STAGE_VERTEX_BIT, VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER, 3));
    //light info
    this->DescriptorLayouts.push_back(Graphics::Vulkan::DescriptorLayout::Create(device, VK_SHADER_STAGE_VERTEX_BIT | VK_SHADER_STAGE_FRAGMENT_BIT, VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER, 1));
    //color, normal and detail images
    this->DescriptorLayouts.push_back(Graphics::Vulkan::DescriptorLayout::Create(device, VK_SHADER_STAGE_FRAGMENT_BIT, VK_DESCRIPTOR_TYPE_SAMPLED_IMAGE, 3));

    //compile shaders
    if (shouldCompileShaders)
    {
        const auto vertexCode = Graphics::Vulkan::Shader::GetFullShaderCode(this->VertexShaderPath);
        const auto vertexCompiled = Graphics::Vulkan::Shader::CompileShader(vertexCode.code, vertexCode.location, vertexCode.type);
        this->VertexShader = Graphics::Vulkan::Shader::Create(device, vertexCompiled, VK_SHADER_STAGE_VERTEX_BIT);
        const auto fragmentCode = Graphics::Vulkan::Shader::GetFullShaderCode(this->FragmentShaderPath);
        const auto fragmentCompiled = Graphics::Vulkan::Shader::CompileShader(fragmentCode.code, fragmentCode.location, fragmentCode.type);
        this->FragmentShader = Graphics::Vulkan::Shader::Create(device, fragmentCompiled, VK_SHADER_STAGE_FRAGMENT_BIT);
    }
    if (this->VertexShader.Shader == VK_NULL_HANDLE)
    {
        Console::Error("no vertex shader provided to the material");
        return;
    }
    if (this->FragmentShader.Shader == VK_NULL_HANDLE)
    {
        Console::Error("no fragment shader provided to the material");
        return;
    }
    this->Pipeline = Graphics::Vulkan::Pipeline::Create(device, {this->VertexShader, this->FragmentShader}, Core::Engine::GetVulkanRenderPass(), Graphics::Vertex::GetBindingDescription(), Graphics::Vertex::GetAttributeDescriptions(), this->DescriptorLayouts);

    //base color
    this->ColorStagingBuffer = Graphics::Vulkan::Buffer::CreateHost(device, this->BaseColor.TotalByteSize, VK_BUFFER_USAGE_TRANSFER_SRC_BIT);
    this->ColorImage = Graphics::Vulkan::Image::Create(device, this->BaseColor.Width, this->BaseColor.Height, VK_FORMAT_R8G8B8A8_UNORM, VK_IMAGE_USAGE_TRANSFER_DST_BIT | VK_IMAGE_USAGE_SAMPLED_BIT, mipMapLevels);
    this->ColorImageView = Graphics::Vulkan::ImageView::Create(device, this->ColorImage, VK_IMAGE_ASPECT_COLOR_BIT);
    //normal
    this->NormalStagingBuffer = Graphics::Vulkan::Buffer::CreateHost(device, this->Normal.TotalByteSize, VK_BUFFER_USAGE_TRANSFER_SRC_BIT);
    this->NormalImage = Graphics::Vulkan::Image::Create(device, this->Normal.Width, this->Normal.Height, VK_FORMAT_R8G8B8A8_UNORM, VK_IMAGE_USAGE_TRANSFER_DST_BIT | VK_IMAGE_USAGE_SAMPLED_BIT, mipMapLevels);
    this->NormalImageView = Graphics::Vulkan::ImageView::Create(device, this->NormalImage, VK_IMAGE_ASPECT_COLOR_BIT);
    //detail1
    this->Detail1StagingBuffer = Graphics::Vulkan::Buffer::CreateHost(device, Detail1.TotalByteSize, VK_BUFFER_USAGE_TRANSFER_SRC_BIT);
    this->Detail1Image = Graphics::Vulkan::Image::Create(device, Detail1.Width, Detail1.Height, VK_FORMAT_R8G8B8A8_UNORM, mipMapLevels);
    this->Detail1ImageView = Graphics::Vulkan::ImageView::Create(device, this->Detail1Image, VK_IMAGE_ASPECT_COLOR_BIT);
    //transfer
    this->TransferCommandPool = Graphics::Vulkan::CommandPool::Create(device, device.QueueFamilies.Graphics.Index);
    this->ColorTransferCommand = Graphics::Vulkan::Command::Create(device, this->TransferCommandPool, Graphics::Vulkan::Command::PRIMARY);
    this->NormalTransferCommand = Graphics::Vulkan::Command::Create(device, this->TransferCommandPool, Graphics::Vulkan::Command::PRIMARY);
    this->Detail1TransferCommand = Graphics::Vulkan::Command::Create(device, this->TransferCommandPool, Graphics::Vulkan::Command::PRIMARY);

    //record commands
    //color
    Graphics::Vulkan::Command::Begin(this->ColorTransferCommand, VK_COMMAND_BUFFER_USAGE_SIMULTANEOUS_USE_BIT);
    Graphics::Vulkan::Command::TransferImageLayout(this->ColorTransferCommand, this->ColorImage, VK_IMAGE_LAYOUT_UNDEFINED, VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL);
    Graphics::Vulkan::Command::BufferToImage(this->ColorTransferCommand, this->ColorStagingBuffer, this->ColorImage);
    Graphics::Vulkan::Command::TransferImageLayout(this->ColorTransferCommand, this->ColorImage, VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL, VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL);
    Graphics::Vulkan::Command::End(this->ColorTransferCommand);
    //normal
    Graphics::Vulkan::Command::Begin(this->NormalTransferCommand, VK_COMMAND_BUFFER_USAGE_SIMULTANEOUS_USE_BIT);
    Graphics::Vulkan::Command::TransferImageLayout(this->NormalTransferCommand, this->NormalImage, VK_IMAGE_LAYOUT_UNDEFINED, VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL);
    Graphics::Vulkan::Command::BufferToImage(this->NormalTransferCommand, this->NormalStagingBuffer, this->NormalImage);
    Graphics::Vulkan::Command::TransferImageLayout(this->NormalTransferCommand, this->NormalImage, VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL, VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL);
    Graphics::Vulkan::Command::End(this->NormalTransferCommand);
    //detail1
    Graphics::Vulkan::Command::Begin(this->Detail1TransferCommand, VK_COMMAND_BUFFER_USAGE_SIMULTANEOUS_USE_BIT);
    Graphics::Vulkan::Command::TransferImageLayout(this->Detail1TransferCommand, this->Detail1Image, VK_IMAGE_LAYOUT_UNDEFINED, VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL);
    Graphics::Vulkan::Command::BufferToImage(this->Detail1TransferCommand, this->Detail1StagingBuffer, this->Detail1Image);
    Graphics::Vulkan::Command::TransferImageLayout(this->Detail1TransferCommand, this->Detail1Image, VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL, VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL);
    Graphics::Vulkan::Command::End(this->Detail1TransferCommand);

    //update buffers
    Graphics::Vulkan::Command::Submit({this->ColorTransferCommand, this->NormalTransferCommand, this->Detail1TransferCommand}, device.Queues.Graphics[0]);
    //setup descriptor sets
    this->DescriptorPool = Graphics::Vulkan::DescriptorPool::Create(device, this->DescriptorLayouts, 1);
    this->DescriptorSet = Graphics::Vulkan::DescriptorSet::Create(device, this->DescriptorPool, this->DescriptorLayouts[2]);
    Graphics::Vulkan::DescriptorSet::UpdateDescriptorSet(this->DescriptorSet, {this->ColorImageView, this->NormalImageView, this->Detail1ImageView}, this->BaseSampler);
}
void Material::OnDestroy()
{
    for (const auto layout : this->DescriptorLayouts)
        Graphics::Vulkan::DescriptorLayout::Destroy(layout);
    this->DescriptorLayouts.clear();

    Graphics::Vulkan::Pipeline::Destroy(this->Pipeline);
    Graphics::Vulkan::Shader::Destroy(this->VertexShader);
    Graphics::Vulkan::Shader::Destroy(this->FragmentShader);

    Graphics::Vulkan::DescriptorPool::Destroy(this->DescriptorPool);
    Graphics::Vulkan::Image::Destroy(this->Detail1Image);
    Graphics::Vulkan::Image::Destroy(this->NormalImage);
    Graphics::Vulkan::Image::Destroy(this->ColorImage);
    Graphics::Vulkan::ImageView::Destroy(this->Detail1ImageView);
    Graphics::Vulkan::ImageView::Destroy(this->NormalImageView);
    Graphics::Vulkan::ImageView::Destroy(this->ColorImageView);
    Graphics::Vulkan::Sampler::Destroy(this->BaseSampler);
    Graphics::Vulkan::CommandPool::Destroy(this->TransferCommandPool);
}

Graphics::Pixel Material::GetBaseColor()
{
    return this->BaseColor.Pixels[0];
}
void Material::SetBaseColor(Graphics::Pixel color)
{
    this->BaseColor.Pixels[0] = color;
}

float Material::GetMetalic()
{
    return ((float)this->Detail1.Pixels[0].r) / 255.0f;
}
void Material::SetMetalic(float metalic)
{
    this->Detail1.Pixels[0].r = glm::round(metalic * 255.0f);
}

float Material::GetRoughness()
{
    return ((float)this->Detail1.Pixels[0].g) / 255.0f;
}
void Material::SetRoughness(float roughness)
{
    this->Detail1.Pixels[0].g = glm::round(this->Detail1.Pixels[0].g * 255.0f);
}

Graphics::Image Material::GetColorTexture()
{
    return this->BaseColor;
}
void Material::SetColorTexture(Graphics::Image image)
{
    this->BaseColor = image;
}
Graphics::Image Material::GetNormalTexture()
{
    return this->Normal;
}
void Material::SetNormalTexture(Graphics::Image image)
{
    this->Normal = image;
}
Graphics::Image Material::GetDetail1Texture()
{
    return this->Detail1;
}
void Material::SetDetail1Texture(Graphics::Image image)
{
    this->Detail1 = image;
}
} // namespace Components
} // namespace Tortuga