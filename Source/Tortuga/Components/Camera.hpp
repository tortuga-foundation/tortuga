#ifndef _CAMERA_COMPONENT
#define _CAMERA_COMPONENT

#include <vector>
#include <cstring>
#include <glm/glm.hpp>
#include <glm/gtc/matrix_transform.hpp>

#include "../Core/Engine.hpp"

#include "../Graphics/Vulkan/Image.hpp"
#include "../Graphics/Vulkan/ImageView.hpp"
#include "../Graphics/Vulkan/RenderPass.hpp"
#include "../Graphics/Vulkan/Framebuffer.hpp"
#include "../Graphics/Vulkan/Buffer.hpp"
#include "../Graphics/Vulkan/CommandPool.hpp"
#include "../Graphics/Vulkan/Command.hpp"

namespace Tortuga
{
namespace Components
{
struct Camera : public Core::ECS::Component
{
private:
  glm::vec4 Viewport = {0, 0, 1, 1};
  uint32_t ResolutionWidth = 1920;
  uint32_t ResolutionHeight = 1080;
  float FieldOfView = 45;
  float NearClipPlane = 0.001f;
  float FarClipPlane = 1000.0f;
  
  //vulkan rendering objects
  Graphics::Vulkan::Image::Image ColorImage;
  Graphics::Vulkan::ImageView::ImageView ColorImageView;
  Graphics::Vulkan::Image::Image DepthImage;
  Graphics::Vulkan::ImageView::ImageView DepthImageView;
  Graphics::Vulkan::Framebuffer::Framebuffer Framebuffer;

  //perspective buffers
  Graphics::Vulkan::Buffer::Buffer PerspectiveStagingBuffer;
  Graphics::Vulkan::Buffer::Buffer PerspectiveBuffer;
  Graphics::Vulkan::CommandPool::CommandPool TransferCommandPool;
  Graphics::Vulkan::Command::Command TransferCommand;

public:
  Camera();
  Camera(uint32_t resolutionWidth, uint32_t resolutionHeight);

  void OnCreate();
  void OnDestroy();

  glm::vec4 GetViewport();
  void SetViewport(glm::vec4 viewport);
  uint32_t GetResolutionWidth();
  uint32_t GetResolutionHeight();
  float GetFieldOfView();
  void SetFieldOfView(float f);
  float GetNearClipPlane();
  void SetNearClipPlay(float f);
  float GetFarClipPlane();
  void SetFarClipPlane(float f);
  bool GetPresentToScreen();
  void SetPresentToScreen(bool presentToScreen);
};
} // namespace Components
} // namespace Tortuga

#endif