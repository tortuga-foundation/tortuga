using System;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Vulkan;
using static Vulkan.VulkanNative;

namespace Tortuga.Graphics.API
{
    internal struct BufferTransferObject
    {
        public CommandPool commandPool;
        public CommandPool.Command TransferCommand;
        public Buffer StagingBuffer;
    };

    internal class Buffer
    {
        public VkBuffer Handle => _buffer;
        public uint Size => _size;
        internal uint ReservedDescriptorSet;

        private VkBuffer _buffer;
        private VkMemoryRequirements _memoryRequirements;
        private VkDeviceMemory _deviceMemory;
        private uint _size;

        public unsafe Buffer(uint size, VkBufferUsageFlags usageFlags, VkMemoryPropertyFlags memoryProperties)
        {
            this._size = size;
            var bufferCreateInfo = VkBufferCreateInfo.New();
            bufferCreateInfo.size = size;
            bufferCreateInfo.usage = usageFlags;
            bufferCreateInfo.sharingMode = VkSharingMode.Exclusive;

            VkBuffer buffer;
            if (vkCreateBuffer(
                Engine.Instance.MainDevice.LogicalDevice,
                &bufferCreateInfo,
                null,
                &buffer
            ) != VkResult.Success)
                throw new System.Exception("failed to create vulkan buffer");
            _buffer = buffer;

            VkMemoryRequirements memoryRequirements;
            vkGetBufferMemoryRequirements(
                Engine.Instance.MainDevice.LogicalDevice,
                _buffer,
                &memoryRequirements
            );
            _memoryRequirements = memoryRequirements;

            var allocInfo = VkMemoryAllocateInfo.New();
            allocInfo.allocationSize = _memoryRequirements.size;
            allocInfo.memoryTypeIndex = Engine.Instance.MainDevice.FindMemoryType(
                _memoryRequirements.memoryTypeBits,
                memoryProperties
            );

            VkDeviceMemory deviceMemory;
            if (vkAllocateMemory(
                Engine.Instance.MainDevice.LogicalDevice,
                &allocInfo,
                null,
                &deviceMemory
            ) != VkResult.Success)
                throw new System.Exception("failed to allocate device memory for vulkan buffer");
            _deviceMemory = deviceMemory;
            if (vkBindBufferMemory(
                Engine.Instance.MainDevice.LogicalDevice,
                _buffer,
                _deviceMemory,
                0
            ) != VkResult.Success)
                throw new System.Exception("failed to bind buffer handle to device memory");
        }

        public static Buffer CreateHost(uint size, VkBufferUsageFlags usageFlags)
            => new Buffer(
            size,
            usageFlags,
            VkMemoryPropertyFlags.HostVisible | VkMemoryPropertyFlags.HostCoherent
        );

        public static Buffer CreateDevice(uint size, VkBufferUsageFlags usageFlags)
            => new Buffer(
            size,
            usageFlags,
            VkMemoryPropertyFlags.DeviceLocal
        );

        public unsafe void SetData<T>(T[] data) where T : struct
        {
            var source = new NativeList<T>();
            foreach (var t in data)
                source.Add(t);
            source.Count = Convert.ToUInt32(data.Length);
            void* mappedMemory;
            if (vkMapMemory(
                Engine.Instance.MainDevice.LogicalDevice,
                _deviceMemory,
                0,
                _size,
                0,
                (void**)&mappedMemory
            ) != VkResult.Success)
                throw new System.Exception("failed to map vulkan memory");
            System.Buffer.MemoryCopy(source.Data.ToPointer(), mappedMemory, _size, _size);
            vkUnmapMemory(
                Engine.Instance.MainDevice.LogicalDevice,
                _deviceMemory
            );
        }
        public unsafe T[] GetData<T>() where T : struct
        {
            uint tSize = Convert.ToUInt32(Unsafe.SizeOf<T>());
            var destination = new NativeList<T>(tSize);
            destination.Count = _size / tSize;

            void* mappedMemory;
            if (vkMapMemory(
                Engine.Instance.MainDevice.LogicalDevice,
                _deviceMemory,
                0,
                _size,
                0,
                (void**)&mappedMemory
            ) != VkResult.Success)
                throw new System.Exception("failed to map vulkan memory");
            System.Buffer.MemoryCopy(mappedMemory, destination.Data.ToPointer(), _size, _size);
            vkUnmapMemory(
                Engine.Instance.MainDevice.LogicalDevice,
                _deviceMemory
            );
            var data = new T[destination.Count];
            for (int i = 0; i < destination.Count; i++)
                data[i] = destination[i];
            return data;
        }

        public async Task SetDataWithStaging<T>(T[] data) where T : struct
        {
            await Task.Run(() =>
            {
                //setup staging buffer
                var staging = Buffer.CreateHost(_size, VkBufferUsageFlags.TransferSrc);
                staging.SetData(data);

                //setup transfer command
                var fence = new Fence();
                var pool = new CommandPool(Engine.Instance.MainDevice.TransferQueueFamily);
                var command = pool.AllocateCommands()[0];
                command.Begin(VkCommandBufferUsageFlags.OneTimeSubmit);
                command.CopyBuffer(staging, this);
                command.End();
                command.Submit(
                    Engine.Instance.MainDevice.TransferQueueFamily.Queues[0],
                    null, null,
                    fence
                );
                fence.Wait();
            });
        }
        public async Task<T[]> GetDataWithStaging<T>() where T : struct
        {
            //setup staging buffer
            var staging = Buffer.CreateHost(_size, VkBufferUsageFlags.TransferSrc);

            //setup transfer command
            var fence = new Fence();
            var pool = new CommandPool(Engine.Instance.MainDevice.TransferQueueFamily);
            var command = pool.AllocateCommands()[0];
            command.Begin(VkCommandBufferUsageFlags.OneTimeSubmit);
            command.CopyBuffer(this, staging);
            command.End();
            command.Submit(
                Engine.Instance.MainDevice.TransferQueueFamily.Queues[0],
                null, null,
                fence
            );
            fence.Wait();
            return await Task.FromResult(staging.GetData<T>());
        }

        internal BufferTransferObject SetDataGetTransferObject<T>(T[] data) where T : struct
        {
            //setup staging buffer
            var staging = Buffer.CreateHost(_size, VkBufferUsageFlags.TransferSrc);
            staging.SetData(data);

            //setup transfer command
            var pool = new CommandPool(Engine.Instance.MainDevice.TransferQueueFamily);
            var command = pool.AllocateCommands()[0];
            command.Begin(VkCommandBufferUsageFlags.OneTimeSubmit);
            command.CopyBuffer(staging, this);
            command.End();
            return new BufferTransferObject
            {
                commandPool = pool,
                StagingBuffer = staging,
                TransferCommand = command
            };
        }
    }
}