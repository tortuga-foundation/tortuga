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

        private VkBuffer _buffer;
        private VkMemoryRequirements _memoryRequirements;
        private VkDeviceMemory _deviceMemory;
        private uint _size;
        private Buffer _staging;
        private CommandPool _commandPool;
        private CommandPool.Command _transferToCommand;
        private CommandPool.Command _transferFromCommand;

        public unsafe Buffer(uint size, VkBufferUsageFlags usageFlags, VkMemoryPropertyFlags memoryProperties, bool isStaging = false)
        {
            if ((usageFlags & VkBufferUsageFlags.TransferSrc) == 0)
                usageFlags |= VkBufferUsageFlags.TransferSrc;
            if ((usageFlags & VkBufferUsageFlags.TransferDst) == 0)
                usageFlags |= VkBufferUsageFlags.TransferDst;

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


            //setup staging buffer
            if (!isStaging)
            {
                _staging = new Buffer(
                    _size,
                    VkBufferUsageFlags.TransferSrc | VkBufferUsageFlags.TransferDst,
                    VkMemoryPropertyFlags.HostVisible | VkMemoryPropertyFlags.HostCoherent,
                    true
                );
                //setup transfer commands
                _commandPool = new CommandPool(
                    Engine.Instance.MainDevice.TransferQueueFamily
                );
                _transferToCommand = _commandPool.AllocateCommands()[0];
                _transferToCommand.Begin(VkCommandBufferUsageFlags.SimultaneousUse);
                _transferToCommand.CopyBuffer(_staging, this);
                _transferToCommand.End();
                _transferFromCommand = _commandPool.AllocateCommands()[0];
                _transferFromCommand.Begin(VkCommandBufferUsageFlags.SimultaneousUse);
                _transferFromCommand.CopyBuffer(this, _staging);
                _transferFromCommand.End();
            }
        }
        unsafe ~Buffer()
        {
            vkDestroyBuffer(
                Engine.Instance.MainDevice.LogicalDevice,
                _buffer,
                null
            );
            vkFreeMemory(
                Engine.Instance.MainDevice.LogicalDevice,
                _deviceMemory,
                null
            );
        }

        public static Buffer CreateHost(uint size, VkBufferUsageFlags usageFlags)
            => new Buffer(
            size,
            usageFlags,
            VkMemoryPropertyFlags.HostVisible | VkMemoryPropertyFlags.HostCoherent,
            true
        );

        public static Buffer CreateDevice(uint size, VkBufferUsageFlags usageFlags)
            => new Buffer(
            size,
            usageFlags,
            VkMemoryPropertyFlags.DeviceLocal,
            false
        );

        public unsafe void SetData(IntPtr ptr, int offset, int size)
        {
            IntPtr mappedMemory;
            if (vkMapMemory(
                Engine.Instance.MainDevice.LogicalDevice,
                _deviceMemory,
                0,
                _size,
                0,
                (void**)&mappedMemory
            ) != VkResult.Success)
                throw new System.Exception("failed to map vulkan memory");
            System.Buffer.MemoryCopy((void*)IntPtr.Add(ptr, offset), (void*)IntPtr.Add(mappedMemory, offset), size, size);
            vkUnmapMemory(
                Engine.Instance.MainDevice.LogicalDevice,
                _deviceMemory
            );
        }

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
            _staging.SetData(data);
            await Task.Run(() =>
            {
                var fence = new Fence();
                _transferToCommand.Submit(
                    Engine.Instance.MainDevice.TransferQueueFamily.Queues[0],
                    null, null,
                    fence
                );
                fence.Wait();
            });
        }
        public async Task<T[]> GetDataWithStaging<T>() where T : struct
        {
            //setup transfer command
            var fence = new Fence();
            _transferFromCommand.Submit(
                Engine.Instance.MainDevice.TransferQueueFamily.Queues[0],
                null, null,
                fence
            );
            fence.Wait();
            return await Task.FromResult(_staging.GetData<T>());
        }

        internal BufferTransferObject SetDataGetTransferObject<T>(T[] data) where T : struct
        {
            _staging.SetData(data);
            return new BufferTransferObject
            {
                commandPool = _commandPool,
                StagingBuffer = _staging,
                TransferCommand = _transferToCommand
            };
        }

        internal BufferTransferObject SetDataGetTransferObject(IntPtr ptr, int offset, int size)
        {
            _staging.SetData(ptr, offset, size);
            return new BufferTransferObject
            {
                commandPool = _commandPool,
                StagingBuffer = _staging,
                TransferCommand = _transferToCommand
            };
        }
    }
}