#ifndef _MATERIAL
#define _MATERIAL

#include "../Graphics/Vulkan/Command.hpp"
#include "../Graphics/Vulkan/DescriptorPool.hpp"
#include "../Graphics/Vulkan/DescriptorSet.hpp"
#include "../Graphics/Vulkan/Buffer.hpp"
#include "../Graphics/Vulkan/Image.hpp"
#include "../Graphics/Vulkan/ImageView.hpp"
#include "../Graphics/Vulkan/Sampler.hpp"
#include "../Graphics/Image.hpp"

namespace Tortuga
{
namespace Component
{
struct Material
{
  Graphics::Vulkan::CommandPool::CommandPool TransferPool;
  Graphics::Vulkan::Command::Command TransferCommand;
  Graphics::Vulkan::Buffer::Buffer StagingAlbedo;
  Graphics::Vulkan::Image::Image AlbedoImage;
  Graphics::Vulkan::ImageView::ImageView AlbedoImageView;
  Graphics::Vulkan::Sampler::Sampler AlbedoImageSampler;

  Material()
  {
    auto device = Core::Engine::GetMainDevice();
    TransferPool = Graphics::Vulkan::CommandPool::Create(device, device.QueueFamilies.Graphics.Index);
    TransferCommand = Graphics::Vulkan::Command::Create(device, TransferPool, Graphics::Vulkan::Command::PRIMARY);
  }
  ~Material()
  {
    Graphics::Vulkan::CommandPool::Destroy(TransferPool);
    Graphics::Vulkan::Buffer::Destroy(StagingAlbedo);
    Graphics::Vulkan::Image::Destroy(AlbedoImage);
    Graphics::Vulkan::ImageView::Destroy(AlbedoImageView);
    Graphics::Vulkan::Sampler::Destroy(AlbedoImageSampler);
  }

  void SetAlbedo(Graphics::Image image)
  {
    auto device = Core::Engine::GetMainDevice();
    {
      if (StagingAlbedo.Size != image.ByteSize)
        Graphics::Vulkan::Buffer::Destroy(StagingAlbedo);
      if (StagingAlbedo.Buffer == VK_NULL_HANDLE)
        StagingAlbedo = Graphics::Vulkan::Buffer::CreateHostSrc(device, image.ByteSize);
      if (AlbedoImage.Width != image.Width || AlbedoImage.Height != image.Height)
        Graphics::Vulkan::Image::Destroy(AlbedoImage);
      if (AlbedoImage.Image == VK_NULL_HANDLE)
        AlbedoImage = Graphics::Vulkan::Image::Create(device, image.Width, image.Height, VK_FORMAT_R8G8B8A8_UNORM, VK_IMAGE_USAGE_SAMPLED_BIT | VK_IMAGE_USAGE_TRANSFER_DST_BIT);

      Graphics::Vulkan::ImageView::Destroy(AlbedoImageView);
      AlbedoImageView = Graphics::Vulkan::ImageView::Create(device, AlbedoImage, VK_IMAGE_ASPECT_COLOR_BIT);
      Graphics::Vulkan::Sampler::Destroy(AlbedoImageSampler);
      AlbedoImageSampler = Graphics::Vulkan::Sampler::Create(device);
    }
    Graphics::Vulkan::Buffer::SetData(StagingAlbedo, image.Pixels, image.ByteSize);
    Graphics::Vulkan::Command::Begin(TransferCommand, VK_COMMAND_BUFFER_USAGE_ONE_TIME_SUBMIT_BIT);
    Graphics::Vulkan::Command::TransferImageLayout(TransferCommand, AlbedoImage, VK_IMAGE_LAYOUT_UNDEFINED, VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL);
    Graphics::Vulkan::Command::BufferToImage(TransferCommand, StagingAlbedo, AlbedoImage, {0, 0}, {image.Width, image.Height});
    Graphics::Vulkan::Command::TransferImageLayout(TransferCommand, AlbedoImage, VK_IMAGE_LAYOUT_UNDEFINED, VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL);
    Graphics::Vulkan::Command::End(TransferCommand);
    Graphics::Vulkan::Command::Submit({TransferCommand}, device.Queues.Graphics[0]);
  }
};
} // namespace Component
} // namespace Tortuga

#endif