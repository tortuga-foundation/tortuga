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

            public unsafe void Begin(VkCommandBufferUsageFlags usage)
            {
                if (_level == VkCommandBufferLevel.Secondary)
                    throw new Exception("secondary command must use the other begin function");

                var beginInfo = VkCommandBufferBeginInfo.New();
                beginInfo.flags = usage;

                if (vkBeginCommandBuffer(_handle, &beginInfo) != VkResult.Success)
                    throw new Exception("failed to begin command buffer recording");
            }

            public void End()
            {
                if (vkEndCommandBuffer(_handle) != VkResult.Success)
                    throw new Exception("failed to end command buffer");
            }

            public unsafe void Submit(VkQueue queue, Command[] commands, Semaphore[] signalSemaphores = null, Semaphore[] waitSemaphores = null, Fence fence = null, VkPipelineStageFlags waitStageMask = VkPipelineStageFlags.TopOfPipe)
            {
                if (commands.Length == 0)
                    return;

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

            public unsafe void TransferImageLayout(Image image, VkImageLayout oldLayout, VkImageLayout newLayout)
            {
                //aspect flags
                VkImageAspectFlags aspect = 0;
                if (newLayout == VkImageLayout.DepthStencilAttachmentOptimal)
                {
                    aspect = VkImageAspectFlags.Depth;
                    if (image.HasStencilComponent)
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
                    barrier.image = image.ImageHandle;
                    barrier.subresourceRange.aspectMask = aspect;
                    barrier.subresourceRange.baseMipLevel = 0;
                    barrier.subresourceRange.levelCount = image.MipLevel;
                    barrier.subresourceRange.baseArrayLayer = 0;
                    barrier.subresourceRange.layerCount = 1;
                    barrier.srcAccessMask = sourceAccess;
                    barrier.dstAccessMask = destinationAccess;
                }
                vkCmdPipelineBarrier(_handle, source, destination, 0, 0, null, 0, null, 1, &barrier);
            }
        }
    }
}