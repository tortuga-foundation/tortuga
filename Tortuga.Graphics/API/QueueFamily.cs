#pragma warning disable CS1591
using System;
using System.Collections.Generic;
using Tortuga.Utils;
using Vulkan;

namespace Tortuga.Graphics.API
{
    [Flags]
    public enum QueueFamilyType
    {
        None = VkQueueFlags.None,
        Graphics = VkQueueFlags.Graphics,
        Compute = VkQueueFlags.Compute,
        Transfer = VkQueueFlags.Transfer,
        SparseBinding = VkQueueFlags.SparseBinding
    }
    public class QueueFamily
    {
        public Device Device => _device;
        public List<VkQueue> Queues => _queues;
        public uint Index => _index;
        public QueueFamilyType Type => _type;

        private uint _index;
        private uint _queueCount;
        private QueueFamilyType _type;
        private NativeList<float> _priorities;
        private Device _device;
        private List<VkQueue> _queues;

        public QueueFamily(uint index, uint queueCount, QueueFamilyType type)
        {
            _index = index;
            _queueCount = queueCount;
            _type = type;
            _priorities = new NativeList<float>(queueCount);
            _priorities.Count = queueCount;
            for (int i = 0; i < queueCount; i++)
                _priorities[i] = 1.0f / queueCount;
        }

        public unsafe VkDeviceQueueCreateInfo QueueCreateInfo
        => new VkDeviceQueueCreateInfo
        {
            sType = VkStructureType.DeviceQueueCreateInfo,
            queueCount = _queueCount,
            queueFamilyIndex = _index,
            pQueuePriorities = (float*)_priorities.Data.ToPointer()
        };

        public unsafe void GetQueuesFromDevice(Device device)
        {
            if (_queues != null)
                throw new Exception("queues are already setup");
            _device = device;
            _queues = new List<VkQueue>();
            for (uint i = 0; i < _queueCount; i++)
            {
                VkQueue queue;
                VulkanNative.vkGetDeviceQueue(
                    _device.Handle,
                    _index,
                    i,
                    &queue
                );
                _queues.Add(queue);
            }
        }
    }
}