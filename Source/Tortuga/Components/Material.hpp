#ifndef _MATERIAL_COMPONENT
#define _MATERIAL_COMPONENT

#include <glm/glm.hpp>

#include "../Core/Engine.hpp"
#include "../Core/ECS/Entity.hpp"

#include "../Graphics/Image.hpp"
#include "../Graphics/Vulkan/Buffer.hpp"
#include "../Graphics/Vulkan/Image.hpp"
#include "../Graphics/Vulkan/ImageView.hpp"
#include "../Graphics/Vulkan/Sampler.hpp"
#include "../Graphics/Vulkan/DescriptorPool.hpp"
#include "../Graphics/Vulkan/DescriptorSet.hpp"
#include "../Graphics/Vulkan/CommandPool.hpp"
#include "../Graphics/Vulkan/Command.hpp"

namespace Tortuga
{
namespace Components
{
struct Material : Core::ECS::Component
{
private:
  Graphics::Image BaseColor = Graphics::Image::White();
  Graphics::Image Normal = Graphics::Image::Blue();
  Graphics::Image Detail1 = Graphics::Image::White();

  //pipeline
  std::string VertexShaderPath;
  std::string FragmentShaderPath;
  bool shouldCompileShaders = false;
  Graphics::Vulkan::Shader::Shader VertexShader;
  Graphics::Vulkan::Shader::Shader FragmentShader;

  //vulkan buffers
  Graphics::Vulkan::Sampler::Sampler BaseSampler;
  //color
  Graphics::Vulkan::Buffer::Buffer ColorStagingBuffer;
  Graphics::Vulkan::Image::Image ColorImage;
  Graphics::Vulkan::ImageView::ImageView ColorImageView;
  //normal
  Graphics::Vulkan::Buffer::Buffer NormalStagingBuffer;
  Graphics::Vulkan::Image::Image NormalImage;
  Graphics::Vulkan::ImageView::ImageView NormalImageView;
  //detail1
  Graphics::Vulkan::Buffer::Buffer Detail1StagingBuffer;
  Graphics::Vulkan::Image::Image Detail1Image;
  Graphics::Vulkan::ImageView::ImageView Detail1ImageView;
  //transfer
  Graphics::Vulkan::CommandPool::CommandPool TransferCommandPool;
  Graphics::Vulkan::Command::Command ColorTransferCommand;
  Graphics::Vulkan::Command::Command NormalTransferCommand;
  Graphics::Vulkan::Command::Command Detail1TransferCommand;
  //descriptor sets
  Graphics::Vulkan::DescriptorPool::DescriptorPool DescriptorPool;
  Graphics::Vulkan::DescriptorSet::DescriptorSet DescriptorSet;

public:
  Material();
  Material(std::string vertex, std::string fragment);
  Material(Graphics::Vulkan::Shader::Shader vertex, Graphics::Vulkan::Shader::Shader fragment);

  void OnCreate();
  void OnDestroy();

  //variables
  Graphics::Pixel GetBaseColor();
  void SetBaseColor(Graphics::Pixel color);
  float GetMetalic();
  void SetMetalic(float metalic);
  float GetRoughness();
  void SetRoughness(float roughness);

  //images
  Graphics::Image GetColorTexture();
  void SetColorTexture(Graphics::Image image);
  Graphics::Image GetNormalTexture();
  void SetNormalTexture(Graphics::Image image);
  Graphics::Image GetDetail1Texture();
  void SetDetail1Texture(Graphics::Image image);
};
} // namespace Components
} // namespace Tortuga

#endif