#pragma warning disable CS1591
using System.Collections.Generic;
using Tortuga.Graphics.API;
using System.Threading;
using Vulkan;
using Tortuga.Utils;
using System;

namespace Tortuga.Graphics
{
    public enum CommandType
    {
        Primary,
        Secondary
    }

    public class CommandBufferService
    {
        private Device _device;
        private Dictionary<QueueFamilyType, QueueFamily> _queueFamilies;
        private Dictionary<QueueFamilyType, CommandPool> _commandPools;
        private int _queueCounter = 0;
        private Mutex _lock = new Mutex();

        public CommandBufferService(Device device)
        {
            _device = device;
            //setup different types of queue families
            _queueFamilies = new Dictionary<QueueFamilyType, QueueFamily>
            {
                {
                    QueueFamilyType.Graphics,
                    device.GraphicsQueueFamily
                },
                {
                    QueueFamilyType.Compute,
                    device.ComputeQueueFamily
                },
                {
                    QueueFamilyType.Transfer,
                    device.TransferQueueFamily
                }
            };

            //setup command pools for each queue family
            _commandPools = new Dictionary<QueueFamilyType, CommandPool>();
            foreach (var queueFamily in _queueFamilies)
                _commandPools.Add(queueFamily.Key, new CommandPool(queueFamily.Value));
        }

        public CommandBuffer GetNewCommand(
            QueueFamilyType queueType,
            CommandType commandType
        )
        {
            var type = VkCommandBufferLevel.Primary;
            if (commandType == CommandType.Secondary)
                type = VkCommandBufferLevel.Secondary;

            return new CommandBuffer(_commandPools[queueType], type);
        }

        private VkQueue GetQueue(QueueFamily queueFamily)
        {
            _lock.WaitOne();
            if (queueFamily.Queues.Count == 0)
                throw new InvalidOperationException("queue family does not contain any queues");

            if (_queueCounter >= queueFamily.Queues.Count)
                _queueCounter = 0;

            var queue = queueFamily.Queues[_queueCounter];
            _queueCounter++;
            _lock.ReleaseMutex();
            return queue;
        }

        public void Submit(
            CommandBuffer command,
            List<API.Semaphore> signalSemaphores = null,
            List<API.Semaphore> waitSemaphores = null
        ) => Submit(
            new List<CommandBuffer> { command },
            signalSemaphores,
            waitSemaphores
        );

        public void Submit(
            List<CommandBuffer> commands,
            List<API.Semaphore> signalSemaphores = null,
            List<API.Semaphore> waitSemaphores = null
        )
        {
            //make sure atleast one command is passed
            if (commands.Count == 0) return;

            //get a free device queue
            var queue = GetQueue(commands[0].CommandPool.QueueFamily);

            //submit command to queue
            CommandBuffer.SubmitCommands(
                commands,
                queue,
                signalSemaphores,
                waitSemaphores,
                VkPipelineStageFlags.TopOfPipe
            );
        }

        public unsafe void Present(
            Swapchain swapchain,
            uint imageIndex = 0,
            List<API.Semaphore> waitSemaphores = null
        )
        {
            var semaphores = new NativeList<VkSemaphore>();
            if (waitSemaphores != null)
            {
                foreach (var s in waitSemaphores)
                    semaphores.Add(s.Handle);
            }

            var swapchains = new NativeList<VkSwapchainKHR>();
            swapchains.Add(swapchain.Handle);

            var presentInfo = new VkPresentInfoKHR
            {
                sType = VkStructureType.PresentInfoKHR,
                swapchainCount = 1,
                pSwapchains = (VkSwapchainKHR*)swapchains.Data.ToPointer(),
                pImageIndices = &imageIndex,
                waitSemaphoreCount = semaphores.Count,
                pWaitSemaphores = (VkSemaphore*)semaphores.Data.ToPointer(),
            };

            //get a free device queue
            var queue = GetQueue(swapchain.PresentQueueFamily);

            if (VulkanNative.vkQueuePresentKHR(
                queue,
                &presentInfo
            ) != VkResult.Success)
                throw new Exception("failed to present swapchain image");
        }
    }
}