#include "./Command.hpp"

namespace Tortuga
{
namespace Graphics
{
namespace Vulkan
{
namespace Command
{
Command Create(Device::Device device, CommandPool::CommandPool pool, Type type)
{
  Command data = {};
  data.Device = device.Device;
  data.CommandType = type;

  VkCommandBufferAllocateInfo commandInfo = {};
  {
    commandInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_ALLOCATE_INFO;
    commandInfo.pNext = 0;
    commandInfo.commandPool = pool.CommandPool;
    if (type == PRIMARY)
      commandInfo.level = VK_COMMAND_BUFFER_LEVEL_PRIMARY;
    else
      commandInfo.level = VK_COMMAND_BUFFER_LEVEL_SECONDARY;
    commandInfo.commandBufferCount = 1;
  }
  ErrorCheck::Callback(vkAllocateCommandBuffers(data.Device, &commandInfo, &data.Command));

  return data;
}
std::vector<Command> Create(Device::Device device, CommandPool::CommandPool pool, Type type, uint32_t amount)
{
  VkCommandBufferAllocateInfo commandInfo = {};
  {
    commandInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_ALLOCATE_INFO;
    commandInfo.pNext = 0;
    commandInfo.commandPool = pool.CommandPool;
    if (type == PRIMARY)
      commandInfo.level = VK_COMMAND_BUFFER_LEVEL_PRIMARY;
    else
      commandInfo.level = VK_COMMAND_BUFFER_LEVEL_SECONDARY;
    commandInfo.commandBufferCount = amount;
  }
  std::vector<VkCommandBuffer> commands(amount);
  ErrorCheck::Callback(vkAllocateCommandBuffers(device.Device, &commandInfo, commands.data()));

  std::vector<Command> data(commands.size());
  for (uint32_t i = 0; i < data.size(); i++)
  {
    data[i].Device = device.Device;
    data[i].Command = commands[i];
    data[i].CommandType = type;
  }

  return data;
}

void Begin(Command data, VkCommandBufferUsageFlags usage, RenderPass::RenderPass renderPass, uint32_t subPass, Framebuffer::Framebuffer framebuffer)
{
  VkCommandBufferInheritanceInfo inheritanceInfo = {};
  {
    inheritanceInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_INHERITANCE_INFO;
    inheritanceInfo.renderPass = renderPass.RenderPass;
    inheritanceInfo.subpass = subPass;
    inheritanceInfo.framebuffer = framebuffer.Framebuffer;
  }

  VkCommandBufferBeginInfo beginInfo = {};
  {
    beginInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_BEGIN_INFO;
    beginInfo.flags = usage;
    if (data.CommandType == SECONDARY)
      beginInfo.pInheritanceInfo = &inheritanceInfo;
  }
  ErrorCheck::Callback(vkBeginCommandBuffer(data.Command, &beginInfo));
}
void End(Command data)
{
  ErrorCheck::Callback(vkEndCommandBuffer(data.Command));
}
void BeginRenderPass(Command data, RenderPass::RenderPass renderPass, Framebuffer::Framebuffer framebuffer, uint32_t width, uint32_t height)
{
  std::vector<VkClearValue> clearValues(2);
  {
    clearValues[0] = {0.0f, 0.0f, 0.0f, 1.0f};
    clearValues[1] = {1.0f, 0};
  }

  VkRenderPassBeginInfo beginInfo = {};
  {
    beginInfo.sType = VK_STRUCTURE_TYPE_RENDER_PASS_BEGIN_INFO;
    beginInfo.renderPass = renderPass.RenderPass;
    beginInfo.framebuffer = framebuffer.Framebuffer;
    beginInfo.renderArea.offset = {0, 0};
    beginInfo.renderArea.extent = {width, height};
    beginInfo.clearValueCount = clearValues.size();
    beginInfo.pClearValues = clearValues.data();
  }

  vkCmdBeginRenderPass(data.Command, &beginInfo, VK_SUBPASS_CONTENTS_SECONDARY_COMMAND_BUFFERS);
}
void EndRenderPass(Command data)
{
  vkCmdEndRenderPass(data.Command);
}
void CopyBuffer(Command data, Buffer::Buffer source, Buffer::Buffer destination, uint32_t sourceOffset, uint32_t destinationOffset)
{
  if (source.Size > destination.Size)
  {
    Console::Warning("Failed to copy buffer because source is larger than the destination");
    return;
  }

  VkBufferCopy bufferCopy = {};
  {
    bufferCopy.srcOffset = sourceOffset;
    bufferCopy.dstOffset = destinationOffset;
    bufferCopy.size = source.Size;
  }
  vkCmdCopyBuffer(data.Command, source.Buffer, destination.Buffer, 1, &bufferCopy);
}
void BindPipeline(Command data, VkPipelineBindPoint BindPoint, Pipeline::Pipeline pipeline, std::vector<DescriptorSet::DescriptorSet> descriptorSets)
{
  std::vector<VkDescriptorSet> vulkanDescriptorSets(descriptorSets.size());
  for (uint32_t i = 0; i < vulkanDescriptorSets.size(); i++)
  {
    vulkanDescriptorSets[i] = descriptorSets[i].set;
  }

  vkCmdBindPipeline(data.Command, BindPoint, pipeline.Pipeline);
  if (vulkanDescriptorSets.size() > 0)
    vkCmdBindDescriptorSets(data.Command, BindPoint, pipeline.Layout, 0, vulkanDescriptorSets.size(), vulkanDescriptorSets.data(), 0, VK_NULL_HANDLE);
}
void BindVertexBuffer(Command data, std::vector<Buffer::Buffer> buffers, uint32_t firstBinding)
{
  std::vector<VkBuffer> vulkanBuffers(buffers.size());
  std::vector<VkDeviceSize> offsets(buffers.size());
  for (uint32_t i = 0; i < buffers.size(); i++)
  {
    offsets[i] = 0;
    vulkanBuffers[i] = buffers[i].Buffer;
  }

  vkCmdBindVertexBuffers(data.Command, 0, vulkanBuffers.size(), vulkanBuffers.data(), offsets.data());
}
void BindIndexBuffer(Command data, Buffer::Buffer buffer, uint32_t offset)
{
  vkCmdBindIndexBuffer(data.Command, buffer.Buffer, offset, VK_INDEX_TYPE_UINT32);
}
void DrawIndexed(Command data, uint32_t indexCount, uint32_t instanceCount, uint32_t indexOffset, uint32_t vertexOffset, uint32_t instanceOffset)
{
  vkCmdDrawIndexed(data.Command, indexCount, instanceCount, indexOffset, vertexOffset, instanceOffset);
}
void Draw(Command data, uint32_t vertexCount, uint32_t instanceCount, uint32_t vertexOffset, uint32_t instanceOffset)
{
  vkCmdDraw(data.Command, vertexCount, instanceCount, vertexOffset, instanceOffset);
}
void SetViewport(Command data, uint32_t x, uint32_t y, uint32_t width, uint32_t height)
{
  VkViewport viewport = {};
  {
    viewport.x = x;
    viewport.y = y;
    viewport.width = width;
    viewport.height = height;
    viewport.minDepth = 0;
    viewport.maxDepth = 1;
  }
  VkRect2D scissor = {};
  {
    scissor.offset = {x, y};
    scissor.extent = {width, height};
  }

  vkCmdSetViewport(data.Command, 0, 1, &viewport);
  vkCmdSetScissor(data.Command, 0, 1, &scissor);
}
void Compute(Command data, uint32_t x, uint32_t y, uint32_t z)
{
  vkCmdDispatch(data.Command, x, y, z);
}
void ExecuteCommands(Command data, std::vector<Command> commands)
{
  std::vector<VkCommandBuffer> commandBuffers(commands.size());
  for (uint32_t i = 0; i < commands.size(); i++)
    commandBuffers[i] = commands[i].Command;

  vkCmdExecuteCommands(data.Command, commandBuffers.size(), commandBuffers.data());
}
void Submit(std::vector<Command> data, VkQueue queue, std::vector<Semaphore::Semaphore> wait, std::vector<Semaphore::Semaphore> signal, Fence::Fence fence)
{
  if (data.size() <= 0)
    return;

  std::vector<VkSemaphore> waitSemaphores(wait.size());
  for (uint32_t i = 0; i < wait.size(); i++)
    waitSemaphores[i] = wait[i].Semaphore;

  std::vector<VkSemaphore> signalSemaphores(signal.size());
  for (uint32_t i = 0; i < signal.size(); i++)
    signalSemaphores[i] = signal[i].Semaphore;

  std::vector<VkCommandBuffer> cmdBuffers(data.size());
  for (uint32_t i = 0; i < cmdBuffers.size(); i++)
    cmdBuffers[i] = data[i].Command;

  VkSubmitInfo submitInfo = {};
  VkShaderStageFlags waitDstStageMask = VK_PIPELINE_STAGE_COMPUTE_SHADER_BIT;
  {
    submitInfo.sType = VK_STRUCTURE_TYPE_SUBMIT_INFO;
    submitInfo.pWaitDstStageMask = &waitDstStageMask;
    submitInfo.commandBufferCount = cmdBuffers.size();
    submitInfo.pCommandBuffers = cmdBuffers.data();
    submitInfo.waitSemaphoreCount = waitSemaphores.size();
    submitInfo.pWaitSemaphores = waitSemaphores.data();
    submitInfo.signalSemaphoreCount = signalSemaphores.size();
    submitInfo.pSignalSemaphores = signalSemaphores.data();
  }
  ErrorCheck::Callback(vkQueueSubmit(queue, 1, &submitInfo, fence.Fence));
}
void TransferImageLayout(Command data, Image::Image image, VkImageLayout oldLayout, VkImageLayout newLayout, uint32_t mipLevel)
{
  if (mipLevel <= 0)
  {
    Console::Error("Mip Level cannot be 0");
    mipLevel = 1;
  }
  //aspect flags
  VkImageAspectFlags aspect = 0;
  if (newLayout == VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL)
  {
    aspect = VK_IMAGE_ASPECT_DEPTH_BIT;
    if (Image::HasStencilComponent(image.Format))
      aspect |= VK_IMAGE_ASPECT_STENCIL_BIT;
  }
  else
  {
    aspect = VK_IMAGE_ASPECT_COLOR_BIT;
  }

  //source
  VkAccessFlags sourceAccess = 0;
  VkPipelineStageFlags source = 0;
  if (oldLayout == VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL)
  {
    source = VK_PIPELINE_STAGE_TRANSFER_BIT;
    sourceAccess = VK_ACCESS_TRANSFER_WRITE_BIT;
  }
  else if (oldLayout == VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL)
  {
    source = VK_PIPELINE_STAGE_TRANSFER_BIT;
    sourceAccess = VK_ACCESS_TRANSFER_READ_BIT;
  }
  else if (oldLayout == VK_IMAGE_LAYOUT_UNDEFINED)
  {
    source = VK_PIPELINE_STAGE_TOP_OF_PIPE_BIT;
  }
  else if (oldLayout == VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL)
  {
    source = VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT;
    sourceAccess = VK_ACCESS_COLOR_ATTACHMENT_READ_BIT;
  }
  else if (oldLayout == VK_IMAGE_LAYOUT_PRESENT_SRC_KHR)
  {
    source = VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT;
    sourceAccess = VK_ACCESS_COLOR_ATTACHMENT_READ_BIT;
  }
  else
  {
    Console::Fatal("image transition not supported");
  }

  //destination
  VkAccessFlags destinationAccess = 0;
  VkPipelineStageFlags destination = 0;
  if (newLayout == VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL)
  {
    destination = VK_PIPELINE_STAGE_FRAGMENT_SHADER_BIT;
    destinationAccess = VK_ACCESS_SHADER_READ_BIT;
  }
  else if (newLayout == VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL)
  {
    destination = VK_PIPELINE_STAGE_TRANSFER_BIT;
    destinationAccess = VK_ACCESS_TRANSFER_WRITE_BIT;
  }
  else if (newLayout == VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL)
  {
    destination = VK_PIPELINE_STAGE_TRANSFER_BIT;
    destinationAccess = VK_ACCESS_TRANSFER_READ_BIT;
  }
  else if (newLayout == VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL)
  {
    destination = VK_PIPELINE_STAGE_EARLY_FRAGMENT_TESTS_BIT;
    destinationAccess = VK_ACCESS_DEPTH_STENCIL_ATTACHMENT_READ_BIT | VK_ACCESS_DEPTH_STENCIL_ATTACHMENT_WRITE_BIT;
  }
  else if (newLayout == VK_IMAGE_LAYOUT_PRESENT_SRC_KHR)
  {
    destination = VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT;
    destinationAccess = VK_ACCESS_COLOR_ATTACHMENT_READ_BIT;
  }
  else if (newLayout == VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL)
  {
    destination = VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT;
    destinationAccess = VK_ACCESS_COLOR_ATTACHMENT_READ_BIT | VK_ACCESS_COLOR_ATTACHMENT_WRITE_BIT;
  }
  else
  {
    Console::Fatal("image transition not supported");
  }

  auto barrier = VkImageMemoryBarrier();
  {
    barrier.sType = VK_STRUCTURE_TYPE_IMAGE_MEMORY_BARRIER;
    barrier.oldLayout = oldLayout;
    barrier.newLayout = newLayout;
    barrier.srcQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED;
    barrier.dstQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED;
    barrier.image = image.Image;
    barrier.subresourceRange.aspectMask = aspect;
    barrier.subresourceRange.baseMipLevel = 0;
    barrier.subresourceRange.levelCount = mipLevel;
    barrier.subresourceRange.baseArrayLayer = 0;
    barrier.subresourceRange.layerCount = 1;
    barrier.srcAccessMask = sourceAccess;
    barrier.dstAccessMask = destinationAccess;
  }
  vkCmdPipelineBarrier(data.Command, source, destination, 0, 0, nullptr, 0, nullptr, 1, &barrier);
}
void BufferToImage(Command data, Buffer::Buffer buffer, Image::Image image)
{
  auto imageLayers = VkImageSubresourceLayers();
  {
    imageLayers.aspectMask = VK_IMAGE_ASPECT_COLOR_BIT;
    imageLayers.mipLevel = 0;
    imageLayers.baseArrayLayer = 0;
    imageLayers.layerCount = 1;
  }
  auto regionInfo = VkBufferImageCopy();
  {
    regionInfo.bufferOffset = 0;
    regionInfo.bufferRowLength = 0;
    regionInfo.bufferImageHeight = 0;
    regionInfo.imageSubresource = imageLayers;
    regionInfo.imageOffset = {0, 0, 0};
    regionInfo.imageExtent = {image.Width, image.Height, 1};
  }
  vkCmdCopyBufferToImage(data.Command, buffer.Buffer, image.Image, VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL, 1, &regionInfo);
}
void CopyImage(Command data, Image::Image source, Image::Image destination)
{
  if (source.Width != destination.Width || source.Height != destination.Height)
  {
    Console::Warning("Cannot copy images if they are not the same size, please use blit iamge instead");
    return;
  }

  auto subResource = VkImageSubresourceLayers();
  {
    subResource.aspectMask = VK_IMAGE_ASPECT_COLOR_BIT;
    subResource.mipLevel = 0;
    subResource.baseArrayLayer = 0;
    subResource.layerCount = 1;
  }
  auto copyInfo = VkImageCopy();
  {
    copyInfo.srcSubresource = subResource;
    copyInfo.srcOffset = {0, 0, 0};
    copyInfo.dstSubresource = subResource;
    copyInfo.dstOffset = {0, 0, 0};
    copyInfo.extent = {source.Width, source.Height, 1};
  }
  vkCmdCopyImage(data.Command, source.Image, VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL, destination.Image, VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL, 1, &copyInfo);
}
void BlitImage(Command data, Image::Image source, Image::Image destination, glm::vec4 viewport)
{
  auto blitInfo = VkImageBlit();
  {
    blitInfo.srcSubresource = VkImageSubresourceLayers();
    blitInfo.srcSubresource.aspectMask = VK_IMAGE_ASPECT_COLOR_BIT;
    blitInfo.srcSubresource.mipLevel = 0;
    blitInfo.srcSubresource.baseArrayLayer = 0;
    blitInfo.srcSubresource.layerCount = 1;
    blitInfo.srcOffsets[0] = {viewport.x * source.Width, viewport.y * source.Height, 0};
    blitInfo.srcOffsets[1] = {viewport.z * source.Width, viewport.w * source.Height, 1};

    blitInfo.dstSubresource = VkImageSubresourceLayers();
    blitInfo.dstSubresource.aspectMask = VK_IMAGE_ASPECT_COLOR_BIT;
    blitInfo.dstSubresource.mipLevel = 0;
    blitInfo.dstSubresource.baseArrayLayer = 0;
    blitInfo.dstSubresource.layerCount = 1;
    blitInfo.dstOffsets[0] = {viewport.x * destination.Width, viewport.y * destination.Height, 0};
    blitInfo.dstOffsets[1] = {viewport.z * destination.Width, viewport.w * destination.Height, 1};
  }
  vkCmdBlitImage(data.Command, source.Image, VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL, destination.Image, VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL, 1, &blitInfo, VK_FILTER_LINEAR);
}

void CreateMipMap(Command data, Image::Image image)
{
  for (uint32_t i = 0; i < image.MipMapLevel; i++)
  {
    float sourceWidth = image.Width;
    float sourceHeight = image.Height;
    if (i > 0)
    {
      const float iTimes2 = i * 2;
      sourceWidth /= iTimes2;
      sourceHeight /= iTimes2;
    }

    const float devideRatio = ((i + 1) * 2);
    const float destinationWidth = image.Width / devideRatio;
    const float destinationHeight = image.Height / devideRatio;

    auto blitInfo = VkImageBlit();
    {
      blitInfo.srcSubresource = VkImageSubresourceLayers();
      blitInfo.srcSubresource.aspectMask = VK_IMAGE_ASPECT_COLOR_BIT;
      blitInfo.srcSubresource.mipLevel = i;
      blitInfo.srcSubresource.baseArrayLayer = 0;
      blitInfo.srcSubresource.layerCount = 1;
      blitInfo.srcOffsets[0] = {0, 0, 0};
      blitInfo.srcOffsets[1] = {sourceWidth, sourceHeight, 1};

      blitInfo.dstSubresource = VkImageSubresourceLayers();
      blitInfo.dstSubresource.aspectMask = VK_IMAGE_ASPECT_COLOR_BIT;
      blitInfo.dstSubresource.mipLevel = i + 1;
      blitInfo.dstSubresource.baseArrayLayer = 0;
      blitInfo.dstSubresource.layerCount = 1;
      blitInfo.dstOffsets[0] = {0, 0, 0};
      blitInfo.dstOffsets[1] = {destinationWidth, destinationHeight, 1};
    }
    vkCmdBlitImage(data.Command, image.Image, VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL, image.Image, VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL, 1, &blitInfo, VK_FILTER_LINEAR);
  }
}
} // namespace Command
} // namespace Vulkan
} // namespace Graphics
} // namespace Tortuga