#pragma warning disable CS1591
using System;
using System.Collections.Generic;
using Tortuga.Utils;
using Vulkan;

namespace Tortuga.Graphics.API
{
    public class CommandBuffer
    {
        public CommandPool CommandPool => _commandPool;
        public VkCommandBufferLevel Level => _level;
        public VkCommandBuffer Handle => _handle;

        private CommandPool _commandPool;
        private VkCommandBufferLevel _level;
        private VkCommandBuffer _handle;

        public unsafe CommandBuffer(
            CommandPool commandPool,
            VkCommandBufferLevel level = VkCommandBufferLevel.Primary
        )
        {
            _commandPool = commandPool;
            _level = level;
            var allocateInfo = new VkCommandBufferAllocateInfo
            {
                sType = VkStructureType.CommandBufferAllocateInfo,
                commandPool = commandPool.Handle,
                level = level,
                commandBufferCount = 1
            };

            VkCommandBuffer commandBuffer;
            if (VulkanNative.vkAllocateCommandBuffers(
                commandPool.Device.Handle,
                &allocateInfo,
                &commandBuffer
            ) != VkResult.Success)
                throw new Exception("failed to allocate command buffers");
            _handle = commandBuffer;
        }

        public unsafe void Begin(VkCommandBufferUsageFlags commandBufferUsageFlag)
        {
            if (_level != VkCommandBufferLevel.Primary)
                throw new Exception("you can only use this method for primary command buffers");

            var beginInfo = new VkCommandBufferBeginInfo
            {
                sType = VkStructureType.CommandBufferBeginInfo,
                flags = commandBufferUsageFlag
            };

            if (VulkanNative.vkBeginCommandBuffer(
                _handle,
                &beginInfo
            ) != VkResult.Success)
                throw new Exception("failed to begin command buffer");
        }

        public unsafe void Begin(
            VkCommandBufferUsageFlags commandBufferUsageFlag,
            RenderPass renderPass,
            Framebuffer framebuffer,
            uint subpass = 0
        )
        {
            if (_level != VkCommandBufferLevel.Secondary)
                throw new Exception("you can only use this method for secondary command buffers");

            var inheritanceInfo = new VkCommandBufferInheritanceInfo
            {
                sType = VkStructureType.CommandBufferInheritanceInfo,
                renderPass = renderPass.Handle,
                framebuffer = framebuffer.Handle
            };

            var beginInfo = new VkCommandBufferBeginInfo
            {
                sType = VkStructureType.CommandBufferBeginInfo,
                flags = commandBufferUsageFlag,
                pInheritanceInfo = &inheritanceInfo
            };

            if (VulkanNative.vkBeginCommandBuffer(
                _handle,
                &beginInfo
            ) != VkResult.Success)
                throw new Exception("failed to begin command buffer");
        }

        public unsafe void End()
        {
            if (VulkanNative.vkEndCommandBuffer(
                _handle
            ) != VkResult.Success)
                throw new Exception("failed to end command buffer");
        }

        public unsafe void CopyBuffer(
            Buffer source,
            Buffer destination
        )
        {
            if (source.Size > destination.Size)
                throw new InvalidOperationException("source size cannot be greater than destination");

            var region = new VkBufferCopy
            {
                dstOffset = 0,
                srcOffset = 0,
                size = source.Size
            };

            VulkanNative.vkCmdCopyBuffer(
                _handle,
                source.Handle,
                destination.Handle,
                1,
                &region
            );
        }

        public unsafe void BeginRenderPass(
            Framebuffer framebuffer,
            VkSubpassContents subpassContents
        )
        {
            var renderPass = framebuffer.RenderPass;
            var clearValues = new NativeList<VkClearValue>();
            foreach (var attachment in renderPass.Attachments)
            {
                clearValues.Add(new VkClearValue()
                {
                    color = new VkClearColorValue(
                        0.0f,
                        0.0f,
                        0.0f,
                        0.0f
                    ),
                    depthStencil = new VkClearDepthStencilValue(
                        1.0f,
                        1
                    )
                });
            }

            var renderPassBeginInfo = new VkRenderPassBeginInfo
            {
                sType = VkStructureType.RenderPassBeginInfo,
                clearValueCount = clearValues.Count,
                pClearValues = (VkClearValue*)clearValues.Data.ToPointer(),
                framebuffer = framebuffer.Handle,
                renderPass = renderPass.Handle,
                renderArea = new VkRect2D
                {
                    offset = new VkOffset2D
                    {
                        x = 0,
                        y = 0
                    },
                    extent = new VkExtent2D
                    {
                        width = framebuffer.Width,
                        height = framebuffer.Height
                    }
                }
            };

            VulkanNative.vkCmdBeginRenderPass(
                _handle,
                &renderPassBeginInfo,
                subpassContents
            );
        }

        public unsafe void EndRenderPass()
        {
            VulkanNative.vkCmdEndRenderPass(_handle);
        }

        public unsafe void BindPipeline(
            Pipeline pipeline,
            VkPipelineBindPoint bindPoint = VkPipelineBindPoint.Graphics
        )
        {
            VulkanNative.vkCmdBindPipeline(
                _handle,
                bindPoint,
                pipeline.Handle
            );
        }

        public unsafe void BindDescriptorSets(
            Pipeline pipeline,
            List<DescriptorSet> descriptorSets,
            VkPipelineBindPoint bindPoint = VkPipelineBindPoint.Graphics
        )
        {
            var sets = new NativeList<VkDescriptorSet>();
            if (descriptorSets != null)
            {
                foreach (var set in descriptorSets)
                    sets.Add(set.Handle);
            }

            if (sets.Count > 0)
            {
                VulkanNative.vkCmdBindDescriptorSets(
                    _handle,
                    bindPoint,
                    pipeline.Layout,
                    0,
                    sets.Count,
                    (VkDescriptorSet*)sets.Data.ToPointer(),
                    0,
                    null
                );
            }
        }

        public unsafe void BlitImage(
            Image source,
            int sourceX, int sourceY,
            int sourceWidth, int sourceHeight,
            uint sourceMipLevel,
            Image destination,
            int destinationX, int destinationY,
            int destinationWidth, int destinationHeight,
            uint destinationMipLevel
        )
        {
            var regionInfo = new VkImageBlit
            {
                srcOffsets_0 = new VkOffset3D
                {
                    x = sourceX,
                    y = sourceY,
                    z = 0
                },
                srcOffsets_1 = new VkOffset3D
                {
                    x = sourceWidth,
                    y = sourceHeight,
                    z = 1
                },
                srcSubresource = new VkImageSubresourceLayers
                {
                    aspectMask = GetAspectFlags(
                        source.Handle,
                        source.Format,
                        source.Layout
                    ),
                    mipLevel = sourceMipLevel,
                    baseArrayLayer = 0,
                    layerCount = 1
                },
                dstOffsets_0 = new VkOffset3D
                {
                    x = destinationX,
                    y = destinationY,
                    z = 0
                },
                dstOffsets_1 = new VkOffset3D
                {
                    x = destinationWidth,
                    y = destinationHeight,
                    z = 1
                },
                dstSubresource = new VkImageSubresourceLayers
                {
                    aspectMask = GetAspectFlags(
                        destination.Handle,
                        destination.Format,
                        destination.Layout
                    ),
                    mipLevel = destinationMipLevel,
                    baseArrayLayer = 0,
                    layerCount = 1
                }
            };
            VulkanNative.vkCmdBlitImage(
                _handle,
                source.Handle,
                source.Layout,
                destination.Handle,
                destination.Layout,
                1,
                &regionInfo,
                VkFilter.Linear
            );
        }

        public unsafe void CopyBufferToImage(
            Buffer buffer,
            Image image,
            uint mipLevel = 0
        )
        {
            var region = new VkBufferImageCopy
            {
                bufferOffset = 0,
                bufferRowLength = image.Width,
                bufferImageHeight = image.Height,
                imageOffset = new VkOffset3D
                {
                    x = 0,
                    y = 0,
                    z = 0
                },
                imageExtent = new VkExtent3D
                {
                    width = image.Width,
                    height = image.Height,
                    depth = 1
                },
                imageSubresource = new VkImageSubresourceLayers
                {
                    aspectMask = GetAspectFlags(
                        image.Handle,
                        image.Format,
                        image.Layout
                    ),
                    mipLevel = mipLevel,
                    baseArrayLayer = 0,
                    layerCount = 1
                }
            };

            VulkanNative.vkCmdCopyBufferToImage(
                _handle,
                buffer.Handle,
                image.Handle,
                image.Layout,
                1,
                &region
            );
        }

        public unsafe void TransferImageLayout(
            Image image,
            VkImageLayout newLayout,
            uint mipLevel = 0,
            uint mipLevelCount = 1
        )
        {
            var aspect = GetAspectFlags(image.Handle, image.Format, newLayout);
            var sourceFlags = GetImageTransferFlags(image.Layout);
            var destinationFlags = GetImageTransferFlags(newLayout);

            var barrier = new VkImageMemoryBarrier
            {
                sType = VkStructureType.ImageMemoryBarrier,
                oldLayout = image.Layout,
                newLayout = newLayout,
                srcQueueFamilyIndex = VulkanNative.QueueFamilyIgnored,
                dstQueueFamilyIndex = VulkanNative.QueueFamilyIgnored,
                image = image.Handle,
                srcAccessMask = sourceFlags.Key,
                dstAccessMask = destinationFlags.Key,
                subresourceRange = new VkImageSubresourceRange
                {
                    aspectMask = aspect,
                    baseMipLevel = mipLevel,
                    levelCount = mipLevelCount,
                    baseArrayLayer = 0,
                    layerCount = 1,
                }
            };

            VulkanNative.vkCmdPipelineBarrier(
                _handle,
                sourceFlags.Value,
                destinationFlags.Value,
                0, 0, null, 0, null,
                1,
                &barrier
            );
            image.Layout = newLayout;
        }

        public unsafe void SetScissor(int x, int y, uint width, uint height)
        {
            var scissor = new VkRect2D
            {
                offset = {
                    x = x,
                    y = y,
                },
                extent = {
                    width = width,
                    height = height
                }
            };

            VulkanNative.vkCmdSetScissor(_handle, 0, 1, &scissor);
        }

        public unsafe void SetViewport(int x, int y, uint width, uint height)
        {
            var viewport = new VkViewport
            {
                x = x,
                y = y,
                width = width,
                height = height,
                minDepth = 0,
                maxDepth = 1
            };
            VulkanNative.vkCmdSetViewport(
                _handle,
                0,
                1,
                &viewport
            );
        }

        public unsafe void BindVertexBuffers(List<Buffer> vertexBuffers)
        {
            var buffers = new NativeList<VkBuffer>();
            var offsets = new NativeList<ulong>();
            foreach (var buf in vertexBuffers)
            {
                buffers.Add(buf.Handle);
                offsets.Add(0);
            }

            VulkanNative.vkCmdBindVertexBuffers(
                _handle,
                0,
                buffers.Count,
                (VkBuffer*)buffers.Data.ToPointer(),
                (ulong*)offsets.Data.ToPointer()
            );
        }

        public unsafe void BindIndexBuffer(Buffer indexBuffer, VkIndexType indexType = VkIndexType.Uint16)
        {
            VulkanNative.vkCmdBindIndexBuffer(
                _handle,
                indexBuffer.Handle,
                0,
                indexType
            );
        }

        public unsafe void DrawIndexed(
            uint indexCount,
            uint instanceCount,
            uint firstIndex = 0,
            int vertexOffset = 0,
            uint firstInstance = 0
        )
        {
            VulkanNative.vkCmdDrawIndexed(
                _handle,
                indexCount,
                instanceCount,
                firstIndex,
                vertexOffset,
                firstInstance
            );
        }

        public unsafe void Draw(
            uint vertexCount,
            uint instanceCount,
            uint firstVertex = 0,
            uint firstInstance = 0
        )
        {
            VulkanNative.vkCmdDraw(
                _handle,
                vertexCount,
                instanceCount,
                firstVertex,
                firstInstance
            );
        }

        public unsafe void Dispatch(
            uint x,
            uint y,
            uint z
        )
        {
            VulkanNative.vkCmdDispatch(
                _handle,
                x,
                y,
                z
            );
        }

        public unsafe void ExecuteCommands(List<CommandBuffer> commandBuffers)
        {
            var commands = new NativeList<VkCommandBuffer>();
            foreach (var commandBuffer in commandBuffers)
                commands.Add(commandBuffer.Handle);

            VulkanNative.vkCmdExecuteCommands(
                _handle,
                commands.Count,
                (VkCommandBuffer*)commands.Data.ToPointer()
            );
        }

        internal unsafe void SubmitCommand(
            VkQueue queue,
            List<Semaphore> signalSemaphore = null,
            List<Semaphore> waitSemaphore = null,
            Fence fence = null,
            VkPipelineStageFlags waitStageMask = VkPipelineStageFlags.TopOfPipe
        ) => SubmitCommands(
            new List<CommandBuffer> { this },
            queue,
            signalSemaphore,
            waitSemaphore,
            fence,
            waitStageMask
        );

        internal static unsafe void SubmitCommands(
            List<CommandBuffer> commandBuffers,
            VkQueue queue,
            List<Semaphore> signalSemaphores = null,
            List<Semaphore> waitSemaphore = null,
            Fence fence = null,
            VkPipelineStageFlags waitStageMask = VkPipelineStageFlags.TopOfPipe
        )
        {
            var commands = new NativeList<VkCommandBuffer>();
            foreach (var cmd in commandBuffers)
                commands.Add(cmd.Handle);

            var signals = new NativeList<VkSemaphore>();
            if (signalSemaphores != null)
            {
                foreach (var semaphore in signalSemaphores)
                    signals.Add(semaphore.Handle);
            }

            var waits = new NativeList<VkSemaphore>();
            if (waitSemaphore != null)
            {
                foreach (var semaphore in waitSemaphore)
                    waits.Add(semaphore.Handle);
            }

            var submitInfo = new VkSubmitInfo
            {
                sType = VkStructureType.SubmitInfo,
                signalSemaphoreCount = signals.Count,
                pSignalSemaphores = (VkSemaphore*)signals.Data.ToPointer(),
                waitSemaphoreCount = waits.Count,
                pWaitSemaphores = (VkSemaphore*)waits.Data.ToPointer(),
                commandBufferCount = commands.Count,
                pCommandBuffers = (VkCommandBuffer*)commands.Data.ToPointer(),
                pWaitDstStageMask = &waitStageMask
            };

            VkFence waitFence = VkFence.Null;
            if (fence != null)
                waitFence = fence.Handle;

            if (VulkanNative.vkQueueSubmit(
                queue,
                1,
                &submitInfo,
                waitFence
            ) != VkResult.Success)
                throw new Exception("failed to submit commands to queue");
        }

        private KeyValuePair<VkAccessFlags, VkPipelineStageFlags> GetImageTransferFlags(
            VkImageLayout layout
        )
        {
            switch (layout)
            {
                case VkImageLayout.TransferDstOptimal:
                    return new KeyValuePair<VkAccessFlags, VkPipelineStageFlags>(
                        VkAccessFlags.TransferWrite,
                        VkPipelineStageFlags.Transfer
                    );
                case VkImageLayout.TransferSrcOptimal:
                    return new KeyValuePair<VkAccessFlags, VkPipelineStageFlags>(
                        VkAccessFlags.TransferRead,
                        VkPipelineStageFlags.Transfer
                    );
                case VkImageLayout.Undefined:
                    return new KeyValuePair<VkAccessFlags, VkPipelineStageFlags>(
                        VkAccessFlags.None,
                        VkPipelineStageFlags.TopOfPipe
                    );
                case VkImageLayout.ColorAttachmentOptimal:
                    return new KeyValuePair<VkAccessFlags, VkPipelineStageFlags>(
                        VkAccessFlags.ColorAttachmentRead | VkAccessFlags.ColorAttachmentWrite,
                        VkPipelineStageFlags.ColorAttachmentOutput
                    );
                case VkImageLayout.PresentSrcKHR:
                    return new KeyValuePair<VkAccessFlags, VkPipelineStageFlags>(
                        VkAccessFlags.ColorAttachmentRead,
                        VkPipelineStageFlags.ColorAttachmentOutput
                    );
                case VkImageLayout.DepthStencilAttachmentOptimal:
                    return new KeyValuePair<VkAccessFlags, VkPipelineStageFlags>(
                        VkAccessFlags.DepthStencilAttachmentRead | VkAccessFlags.DepthStencilAttachmentWrite,
                        VkPipelineStageFlags.EarlyFragmentTests
                    );
            }

            return new KeyValuePair<VkAccessFlags, VkPipelineStageFlags>(
                VkAccessFlags.None,
                VkPipelineStageFlags.None
            );
        }
        private VkImageAspectFlags GetAspectFlags(
            VkImage image,
            VkFormat format,
            VkImageLayout layout
        )
        {
            VkImageAspectFlags aspect = 0;
            if (layout == VkImageLayout.DepthStencilAttachmentOptimal)
            {
                aspect = VkImageAspectFlags.Depth;
                if (Image.HasStencil(format))
                    aspect |= VkImageAspectFlags.Stencil;
            }
            else
                aspect = VkImageAspectFlags.Color;

            return aspect;
        }
    }
}