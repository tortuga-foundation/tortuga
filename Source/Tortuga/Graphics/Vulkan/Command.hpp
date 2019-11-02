#ifndef _VULKAN_COMMAND
#define _VULKAN_COMMAND

#include <vulkan/vulkan.h>
#include <glm/glm.hpp>

#include "./ErrorCheck.hpp"
#include "./Device.hpp"
#include "./Pipeline.hpp"
#include "./CommandPool.hpp"
#include "./DescriptorSet.hpp"
#include "./Buffer.hpp"
#include "./Image.hpp"
#include "./Semaphore.hpp"
#include "./Fence.hpp"
#include "./RenderPass.hpp"
#include "./Framebuffer.hpp"

namespace Tortuga
{
namespace Graphics
{
namespace Vulkan
{
namespace Command
{
enum Type
{
  PRIMARY,
  SECONDARY
};
struct Command
{
  VkDevice Device = VK_NULL_HANDLE;
  VkCommandBuffer Command = VK_NULL_HANDLE;
  Type CommandType;
};

Command Create(Device::Device device, CommandPool::CommandPool pool, Type type);
std::vector<Command> Create(Device::Device device, CommandPool::CommandPool pool, Type type, uint32_t amount);

void Begin(Command data, VkCommandBufferUsageFlags usage, RenderPass::RenderPass renderPass = {VK_NULL_HANDLE, VK_NULL_HANDLE, {}}, uint32_t subPass = 0, Framebuffer::Framebuffer framebuffer = {VK_NULL_HANDLE, VK_NULL_HANDLE});
void End(Command data);
void BeginRenderPass(Command data, RenderPass::RenderPass renderPass, Framebuffer::Framebuffer framebuffer, uint32_t width, uint32_t height);
void EndRenderPass(Command data);
void CopyBuffer(Command data, Buffer::Buffer source, Buffer::Buffer destination, uint32_t sourceOffset = 0, uint32_t destinationOffset = 0);
void BindPipeline(Command data, VkPipelineBindPoint BindPoint, Pipeline::Pipeline pipeline, std::vector<DescriptorSet::DescriptorSet> descriptorSets = {});
void BindVertexBuffer(Command data, std::vector<Buffer::Buffer> buffers, uint32_t firstBinding = 0);
void BindIndexBuffer(Command data, Buffer::Buffer buffer, uint32_t offset = 0);
void DrawIndexed(Command data, uint32_t indexCount, uint32_t instanceCount = 1, uint32_t indexOffset = 0, uint32_t vertexOffset = 0, uint32_t instanceOffset = 0);
void Draw(Command data, uint32_t vertexCount, uint32_t instanceCount = 1, uint32_t vertexOffset = 0, uint32_t instanceOffset = 0);
void SetViewport(Command data, uint32_t x, uint32_t y, uint32_t width, uint32_t height);
void Compute(Command data, uint32_t x, uint32_t y, uint32_t z);
void ExecuteCommands(Command data, std::vector<Command> commands);
void Submit(std::vector<Command> data, VkQueue queue, std::vector<Semaphore::Semaphore> wait = {}, std::vector<Semaphore::Semaphore> signal = {}, Fence::Fence fence = {VK_NULL_HANDLE, VK_NULL_HANDLE});
void TransferImageLayout(Command data, Image::Image image, VkImageLayout oldLayout, VkImageLayout newLayout);
void BufferToImage(Command data, Buffer::Buffer buffer, Image::Image image);
void CopyImage(Command data, Image::Image source, Image::Image destination);
void BlitImage(Command data, Image::Image source, Image::Image destination, glm::vec4 viewport = glm::vec4(0, 0, 1, 1));

} // namespace Command
} // namespace Vulkan
} // namespace Graphics
} // namespace Tortuga

#endif