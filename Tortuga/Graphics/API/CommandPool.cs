using System;
using System.Collections.Generic;
using Vulkan;
using static Vulkan.VulkanNative;

namespace Tortuga.Graphics.API
{
    internal class CommandPool
    {
        public VkCommandPool Handle => _commandPool;
        public Device.QueueFamily QueueFamily => _queueFamily;

        private VkCommandPool _commandPool;
        private Device.QueueFamily _queueFamily;
        private List<VkCommandBuffer> _commandBuffers;

        public unsafe CommandPool(Device.QueueFamily queueFamily)
        {
            this._queueFamily = queueFamily;
            this._commandBuffers = new List<VkCommandBuffer>();

            var createInfo = VkCommandPoolCreateInfo.New();
            createInfo.flags = VkCommandPoolCreateFlags.ResetCommandBuffer;
            createInfo.queueFamilyIndex = queueFamily.Index;

            VkCommandPool commandPool;
            if (vkCreateCommandPool(Engine.Instance.MainDevice.LogicalDevice, &createInfo, null, &commandPool) != VkResult.Success)
                throw new Exception("failed to create command pool on device");
            _commandPool = commandPool;
        }

        unsafe ~CommandPool()
        {
            vkDestroyCommandPool(Engine.Instance.MainDevice.LogicalDevice, _commandPool, null);
        }

        public unsafe List<Command> AllocateCommands(VkCommandBufferLevel level = VkCommandBufferLevel.Primary, uint amount = 1)
        {
            var commandInfo = VkCommandBufferAllocateInfo.New();
            commandInfo.commandPool = _commandPool;
            commandInfo.level = level;
            commandInfo.commandBufferCount = amount;

            var commandbuffers = new NativeList<VkCommandBuffer>(commandInfo.commandBufferCount);
            commandbuffers.Count = commandInfo.commandBufferCount;
            if (vkAllocateCommandBuffers(
                Engine.Instance.MainDevice.LogicalDevice,
                &commandInfo,
                (VkCommandBuffer*)commandbuffers.Data.ToPointer()
            ) != VkResult.Success)
                throw new Exception("failed to allocate command buffers");

            var response = new List<Command>();
            foreach (var cm in commandbuffers)
            {
                _commandBuffers.Add(cm);
                response.Add(new Command(cm, level, _queueFamily));
            }
            return response;
        }

        internal class Command
        {
            public VkCommandBuffer Handle => _handle;
            public VkCommandBufferLevel Level => _level;
            public Device.QueueFamily QueueFamily => _queueFamily;

            private VkCommandBuffer _handle;
            private VkCommandBufferLevel _level;
            private Device.QueueFamily _queueFamily;

            public Command(VkCommandBuffer handle, VkCommandBufferLevel level, Device.QueueFamily queueFamily)
            {
                this._handle = handle;
                this._level = level;
                this._queueFamily = queueFamily;
            }

            public unsafe void Begin(VkCommandBufferUsageFlags usage, Framebuffer framebuffer = null, uint subpass = 0)
            {
                var inheritanceInfo = VkCommandBufferInheritanceInfo.New();
                var beginInfo = VkCommandBufferBeginInfo.New();
                beginInfo.flags = usage;
                if (_level == VkCommandBufferLevel.Secondary)
                {
                    if (framebuffer == null)
                        throw new Exception("secondary command buffer requires a framebuffer");

                    inheritanceInfo.renderPass = Engine.Instance.MainRenderPass.Handle;
                    inheritanceInfo.subpass = subpass;
                    inheritanceInfo.framebuffer = framebuffer.Handle;
                    beginInfo.pInheritanceInfo = &inheritanceInfo;
                }

                if (vkBeginCommandBuffer(_handle, &beginInfo) != VkResult.Success)
                    throw new Exception("failed to begin command buffer recording");
            }
            public void End()
            {
                if (vkEndCommandBuffer(_handle) != VkResult.Success)
                    throw new Exception("failed to end command buffer");
            }

            public unsafe void CopyBuffer(Buffer source, Buffer destination)
            {
                if (source.Size != destination.Size)
                    return;

                var bufferCopy = new VkBufferCopy()
                {
                    dstOffset = 0,
                    srcOffset = 0,
                    size = source.Size
                };

                vkCmdCopyBuffer(_handle, source.Handle, destination.Handle, 1, &bufferCopy);
            }

            public unsafe void BeginRenderPass(RenderPass renderPass, Framebuffer framebuffer)
            {
                var clearValues = new NativeList<VkClearValue>();
                clearValues.Add(new VkClearValue
                {
                    color = new VkClearColorValue(0, 0, 0, 0),

                });
                clearValues.Add(new VkClearValue
                {
                    depthStencil = new VkClearDepthStencilValue(0, 0)
                });

                var renderPassBeginInfo = VkRenderPassBeginInfo.New();
                renderPassBeginInfo.clearValueCount = clearValues.Count;
                renderPassBeginInfo.pClearValues = (VkClearValue*)clearValues.Data.ToPointer();
                renderPassBeginInfo.framebuffer = framebuffer.Handle;
                renderPassBeginInfo.renderPass = renderPass.Handle;
                renderPassBeginInfo.renderArea = new VkRect2D
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
                };

                vkCmdBeginRenderPass(this._handle, &renderPassBeginInfo, VkSubpassContents.SecondaryCommandBuffers);
            }
            public void EndRenderPass()
            {
                vkCmdEndRenderPass(_handle);
            }
            public void BindPipeline(Pipeline pipeline, VkPipelineBindPoint bindPoint = VkPipelineBindPoint.Graphics)
            {
                vkCmdBindPipeline(_handle, bindPoint, pipeline.Handle);
            }
            public unsafe void BlitImage(
                VkImage source,
                int sourceX,
                int sourceY,
                int sourceWidth,
                int sourceHeight,
                VkImage destination,
                int destinationX,
                int destinationY,
                int destinationWidth,
                int destinationHeight
                )
            {
                if (source == VkImage.Null)
                    return;
                if (destination == VkImage.Null)
                    return;

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
                        aspectMask = VkImageAspectFlags.Color,
                        mipLevel = 0,
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
                        aspectMask = VkImageAspectFlags.Color,
                        mipLevel = 0,
                        baseArrayLayer = 0,
                        layerCount = 1
                    }
                };
                vkCmdBlitImage(_handle, source, VkImageLayout.TransferSrcOptimal, destination, VkImageLayout.TransferDstOptimal, 1, &regionInfo, VkFilter.Linear);
            }

            public unsafe void TransferImageLayout(VkImage image, VkFormat format, VkImageLayout oldLayout, VkImageLayout newLayout, uint mipLevel = 1)
            {
                //aspect flags
                VkImageAspectFlags aspect = 0;
                if (newLayout == VkImageLayout.DepthStencilAttachmentOptimal)
                {
                    aspect = VkImageAspectFlags.Depth;
                    if (Image.HasStencil(format))
                        aspect |= VkImageAspectFlags.Stencil;
                }
                else
                {
                    aspect = VkImageAspectFlags.Color;
                }

                //source
                VkAccessFlags sourceAccess = 0;
                VkPipelineStageFlags source = 0;
                if (oldLayout == VkImageLayout.TransferDstOptimal)
                {
                    source = VkPipelineStageFlags.Transfer;
                    sourceAccess = VkAccessFlags.TransferWrite;
                }
                else if (oldLayout == VkImageLayout.TransferSrcOptimal)
                {
                    source = VkPipelineStageFlags.Transfer;
                    sourceAccess = VkAccessFlags.TransferRead;
                }
                else if (oldLayout == VkImageLayout.Undefined)
                {
                    source = VkPipelineStageFlags.TopOfPipe;
                }
                else if (oldLayout == VkImageLayout.ColorAttachmentOptimal)
                {
                    source = VkPipelineStageFlags.ColorAttachmentOutput;
                    sourceAccess = VkAccessFlags.ColorAttachmentRead;
                }
                else if (oldLayout == VkImageLayout.PresentSrcKHR)
                {
                    source = VkPipelineStageFlags.ColorAttachmentOutput;
                    sourceAccess = VkAccessFlags.ColorAttachmentRead;
                }
                else
                    throw new NotSupportedException("image transition not supported");

                //destination
                VkAccessFlags destinationAccess = 0;
                VkPipelineStageFlags destination = 0;
                if (newLayout == VkImageLayout.ShaderReadOnlyOptimal)
                {
                    destination = VkPipelineStageFlags.FragmentShader;
                    destinationAccess = VkAccessFlags.ShaderRead;
                }
                else if (newLayout == VkImageLayout.TransferDstOptimal)
                {
                    destination = VkPipelineStageFlags.Transfer;
                    destinationAccess = VkAccessFlags.TransferWrite;
                }
                else if (newLayout == VkImageLayout.TransferSrcOptimal)
                {
                    destination = VkPipelineStageFlags.Transfer;
                    destinationAccess = VkAccessFlags.TransferRead;
                }
                else if (newLayout == VkImageLayout.DepthStencilAttachmentOptimal)
                {
                    destination = VkPipelineStageFlags.EarlyFragmentTests;
                    destinationAccess = VkAccessFlags.DepthStencilAttachmentRead | VkAccessFlags.DepthStencilAttachmentWrite;
                }
                else if (newLayout == VkImageLayout.PresentSrcKHR)
                {
                    destination = VkPipelineStageFlags.ColorAttachmentOutput;
                    destinationAccess = VkAccessFlags.ColorAttachmentRead;
                }
                else if (newLayout == VkImageLayout.ColorAttachmentOptimal)
                {
                    destination = VkPipelineStageFlags.ColorAttachmentOutput;
                    destinationAccess = VkAccessFlags.ColorAttachmentRead | VkAccessFlags.ColorAttachmentWrite;
                }
                else
                    throw new NotSupportedException("image transition not supported");

                var barrier = VkImageMemoryBarrier.New();
                {
                    barrier.oldLayout = oldLayout;
                    barrier.newLayout = newLayout;
                    barrier.srcQueueFamilyIndex = QueueFamilyIgnored;
                    barrier.dstQueueFamilyIndex = QueueFamilyIgnored;
                    barrier.image = image;
                    barrier.subresourceRange.aspectMask = aspect;
                    barrier.subresourceRange.baseMipLevel = 0;
                    barrier.subresourceRange.levelCount = mipLevel;
                    barrier.subresourceRange.baseArrayLayer = 0;
                    barrier.subresourceRange.layerCount = 1;
                    barrier.srcAccessMask = sourceAccess;
                    barrier.dstAccessMask = destinationAccess;
                }
                vkCmdPipelineBarrier(_handle, source, destination, 0, 0, null, 0, null, 1, &barrier);
            }
            public unsafe void TransferImageLayout(Image image, VkImageLayout oldLayout, VkImageLayout newLayout)
                => TransferImageLayout(image.ImageHandle, image.Format, oldLayout, newLayout, image.MipLevel);

            public unsafe void SetViewport(int x, int y, uint width, uint height)
            {
                var scissor = new VkRect2D
                {
                    offset = {
                        x = x,
                        y = y
                    },
                    extent = {
                        width = width,
                        height = height
                    }
                };
                vkCmdSetScissor(_handle, 0, 1, &scissor);

                var viewport = new VkViewport
                {
                    x = x,
                    y = y,
                    width = width,
                    height = height,
                    minDepth = 0,
                    maxDepth = 1
                };
                vkCmdSetViewport(_handle, 0, 1, &viewport);
            }
            public unsafe void Draw()
            {
                vkCmdDraw(_handle, 3, 1, 0, 0);
            }
            public unsafe void ExecuteCommands(Command[] commands)
            {
                var cmds = new NativeList<VkCommandBuffer>();
                foreach (var c in commands)
                    cmds.Add(c.Handle);

                vkCmdExecuteCommands(_handle, cmds.Count, (VkCommandBuffer*)cmds.Data.ToPointer());
            }
            public unsafe void Submit(
                VkQueue queue,
                Semaphore[] signalSemaphores = null,
                Semaphore[] waitSemaphores = null,
                Fence fence = null,
                VkPipelineStageFlags waitStageMask = VkPipelineStageFlags.TopOfPipe
            ) => Command.Submit(queue, new Command[] { this }, signalSemaphores, waitSemaphores, fence, waitStageMask);
            public static unsafe void Submit(
                VkQueue queue,
                Command[] commands,
                Semaphore[] signalSemaphores = null,
                Semaphore[] waitSemaphores = null,
                Fence fence = null,
                VkPipelineStageFlags waitStageMask = VkPipelineStageFlags.TopOfPipe)
            {
                if (commands.Length == 0)
                    return;
                if (signalSemaphores == null)
                    signalSemaphores = new Semaphore[0];
                if (waitSemaphores == null)
                    waitSemaphores = new Semaphore[0];

                //get command buffers
                var uintCmdsLength = Convert.ToUInt32(commands.Length);
                var cmds = new NativeList<VkCommandBuffer>(uintCmdsLength);
                cmds.Count = uintCmdsLength;
                for (uint i = 0; i < uintCmdsLength; i++)
                    cmds[i] = commands[i].Handle;

                //get signal seamphores
                var uintSignalSemLength = Convert.ToUInt32(signalSemaphores.Length);
                var signalSem = new NativeList<VkSemaphore>(uintSignalSemLength);
                signalSem.Count = uintSignalSemLength;
                for (uint i = 0; i < uintSignalSemLength; i++)
                    signalSem[i] = signalSemaphores[i].Handle;

                //get wait semaphores
                var uintWaitSemLength = Convert.ToUInt32(waitSemaphores.Length);
                var WaitSem = new NativeList<VkSemaphore>(uintWaitSemLength);
                WaitSem.Count = uintWaitSemLength;
                for (uint i = 0; i < uintWaitSemLength; i++)
                    WaitSem[i] = waitSemaphores[i].Handle;


                var submitInfo = VkSubmitInfo.New();
                submitInfo.signalSemaphoreCount = uintSignalSemLength;
                submitInfo.pSignalSemaphores = (VkSemaphore*)signalSem.Data.ToPointer();
                submitInfo.waitSemaphoreCount = uintWaitSemLength;
                submitInfo.pWaitSemaphores = (VkSemaphore*)WaitSem.Data.ToPointer();
                submitInfo.commandBufferCount = uintCmdsLength;
                submitInfo.pCommandBuffers = (VkCommandBuffer*)cmds.Data.ToPointer();
                submitInfo.pWaitDstStageMask = &waitStageMask;

                VkFence waitFence = VkFence.Null;
                if (fence != null)
                    waitFence = fence.Handle;

                if (vkQueueSubmit(queue, 1, &submitInfo, waitFence) != VkResult.Success)
                    throw new Exception("failed to submit commands to queue");
            }
        }
    }
}