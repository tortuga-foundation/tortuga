#pragma warning disable CS1591
using System.Collections.Generic;
using System.Linq;
using Tortuga.Graphics.API;
using Vulkan;
using System.Threading.Tasks;

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
                freeQueues = queueFamily.Queues.Where(queue => (
                    _queuesInUse.FindIndex(q => q.Handle == queue.Handle)
                ) == -1).ToList();
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
    }
}