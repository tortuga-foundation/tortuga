#pragma warning disable CS1591
using System.Collections.Generic;
using System.Linq;
using Tortuga.Graphics.API;
using Vulkan;
using System.Threading.Tasks;
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
        private List<VkQueue> _queuesInUse;
        private Dictionary<QueueFamilyType, QueueFamily> _queueFamilies;
        private Dictionary<QueueFamilyType, CommandPool> _commandPools;
        private List<Task> _tasks;

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

            _tasks = new List<Task>();
            _queuesInUse = new List<VkQueue>();
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

        private VkQueue WaitForFreeQueue(QueueFamily queueFamily)
        {
            var freeQueues = queueFamily.Queues.Where(queue => (
                _queuesInUse.FindIndex(q => q.Handle == queue.Handle)
            ) == -1).ToList();
            while (freeQueues.Count == 0)
            {
                Task.Delay(1).Wait();
                freeQueues = queueFamily.Queues.Where(queue =>
                {
                    if (_queuesInUse == null || _queuesInUse.Count == 0)
                        return true;

                    if (_queuesInUse.FindIndex(q => q.Handle == queue.Handle) == -1)
                        return true;
                    return false;
                }).ToList();
            }
            var queueFound = freeQueues[0];
            _queuesInUse.Add(queueFound);
            return queueFound;
        }

        public void Submit(
            CommandBuffer command,
            List<Semaphore> signalSemaphores = null,
            List<Semaphore> waitSemaphores = null,
            Fence fence = null
        ) => Submit(
            new List<CommandBuffer> { command },
            signalSemaphores,
            waitSemaphores,
            fence
        );

        public void Submit(
            List<CommandBuffer> commands,
            List<Semaphore> signalSemaphores = null,
            List<Semaphore> waitSemaphores = null,
            Fence fence = null
        )
        {
            //make sure atleast one command is passed
            if (commands.Count == 0) return;

            //remove all completed tasks
            _tasks.RemoveAll(t => t.IsCompleted);

            //if no fence is provided, create one
            if (fence == null)
                fence = new Fence(_device);

            //get a free device queue
            var queue = WaitForFreeQueue(commands[0].CommandPool.QueueFamily);

            //submit command to queue
            CommandBuffer.SubmitCommands(
                commands,
                queue,
                signalSemaphores,
                waitSemaphores,
                fence,
                VkPipelineStageFlags.TopOfPipe
            );

            //create a task to free queue when command is complete
            _tasks.Add(
                Task.Run(() =>
                {
                    fence.Wait();
                    _queuesInUse.Remove(queue);
                })
            );
        }

        public unsafe void Present(
            Swapchain swapchain,
            uint imageIndex = 0,
            List<Semaphore> waitSemaphores = null
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
            var queue = WaitForFreeQueue(swapchain.PresentQueueFamily);

            if (VulkanNative.vkQueuePresentKHR(
                queue,
                &presentInfo
            ) != VkResult.Success)
                throw new Exception("failed to present swapchain image");

            //create a task to free queue when command is complete
            _queuesInUse.Remove(queue);
        }
    }
}