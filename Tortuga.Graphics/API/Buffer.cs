using System;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Vulkan;
using Tortuga.Utils;
using static Vulkan.VulkanNative;

namespace Tortuga.Graphics.API
{
    internal struct BufferTransferObject
    {
        public CommandPool commandPool;
        public CommandPool.Command TransferCommand;
    };

    internal enum BufferAccessibility
    {
        HostAndDevice,
        DeviceOnly
    }

    internal class Buffer
    {
        public VkBuffer Handle => _buffer;
        public uint Size => _size;
        public Device DeviceUsed => _device;

        //staging buffer
        private Buffer _staging;
        private CommandPool _commandPool;
        private CommandPool.Command _transferToCommand;
        private CommandPool.Command _transferFromCommand;

        //buffer & memory handlers
        private VkBuffer _buffer;
        private VkMemoryRequirements _memoryRequirements;
        private VkDeviceMemory _deviceMemory;

        //details used to create the buffer
        private Device _device;
        private uint _size;
        private VkBufferUsageFlags _bufferUsage;
        private VkMemoryPropertyFlags _memoryProperty;

        public Buffer(
            Device device,
            uint size,
            VkBufferUsageFlags bufferUsage,
            BufferAccessibility accessibility
        )
        {
            Init(
                device,
                size,
                bufferUsage,
                accessibility == BufferAccessibility.HostAndDevice ?
                VkMemoryPropertyFlags.HostVisible | VkMemoryPropertyFlags.HostCoherent :
                VkMemoryPropertyFlags.DeviceLocal
            );

            if (accessibility == BufferAccessibility.DeviceOnly)
            {
                _staging = new Buffer(
                    _device,
                    size,
                    bufferUsage,
                    BufferAccessibility.HostAndDevice
                );
                //setup transfer commands
                _commandPool = new CommandPool(
                    _device,
                    _device.TransferQueueFamily
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
        ~Buffer()
        {
            Dispose();
        }

        private unsafe void Init(
            Device device,
            uint size,
            VkBufferUsageFlags bufferUsage,
            VkMemoryPropertyFlags memoryProperty
        )
        {
            if (size == 0)
                throw new Exception("cannot create buffer with size of zero bytes");

            //make sure buffer usage supports transfer data to and from buffer
            if ((bufferUsage & VkBufferUsageFlags.TransferSrc) == 0)
                bufferUsage |= VkBufferUsageFlags.TransferSrc;
            if ((bufferUsage & VkBufferUsageFlags.TransferDst) == 0)
                bufferUsage |= VkBufferUsageFlags.TransferDst;

            //store parameter information
            _size = size;
            _device = device;
            _bufferUsage = bufferUsage;
            _memoryProperty = memoryProperty;

            //buffer create info
            var bufferCreateInfo = VkBufferCreateInfo.New();
            bufferCreateInfo.size = size;
            bufferCreateInfo.usage = bufferUsage;
            bufferCreateInfo.sharingMode = VkSharingMode.Exclusive;

            //setup buffer handler
            VkBuffer buffer;
            if (vkCreateBuffer(
                device.LogicalDevice,
                &bufferCreateInfo,
                null,
                &buffer
            ) != VkResult.Success)
                throw new Exception("failed to create vulkan buffer handler");
            _buffer = buffer;

            //memory allocation info
            var memoryRequirements = GetMemoryRequirements(device.LogicalDevice, buffer);
            _memoryRequirements = memoryRequirements;
            var memoryAllocateInfo = VkMemoryAllocateInfo.New();
            memoryAllocateInfo.allocationSize = memoryRequirements.size;
            memoryAllocateInfo.memoryTypeIndex = _device.FindMemoryType(
                memoryRequirements.memoryTypeBits,
                memoryProperty
            );

            //setup device memory
            VkDeviceMemory deviceMemory;
            if (vkAllocateMemory(
                device.LogicalDevice,
                &memoryAllocateInfo,
                null,
                &deviceMemory
            ) != VkResult.Success)
                throw new Exception("failed to allocate device memory");
            _deviceMemory = deviceMemory;

            //bind buffer handler with device memory
            if (vkBindBufferMemory(
                device.LogicalDevice,
                buffer,
                deviceMemory,
                0
            ) != VkResult.Success)
                throw new Exception("failed to bind buffer handler to device memory");
        }

        private unsafe VkMemoryRequirements GetMemoryRequirements(
            VkDevice device,
            VkBuffer buffer
        )
        {
            VkMemoryRequirements memoryRequirements;
            vkGetBufferMemoryRequirements(
                device,
                buffer,
                &memoryRequirements
            );
            return memoryRequirements;
        }

        public unsafe void Dispose()
        {
            if (_buffer != VkBuffer.Null)
            {
                vkDestroyBuffer(
                    _device.LogicalDevice,
                    _buffer,
                    null
                );
                _buffer = VkBuffer.Null;
            }
            if (_deviceMemory != VkDeviceMemory.Null)
            {
                vkFreeMemory(
                    _device.LogicalDevice,
                    _deviceMemory,
                    null
                );
                _deviceMemory = VkDeviceMemory.Null;
            }
            if (_staging != null)
                _staging.Dispose();
        }

        public void Resize(int size)
        {
            Dispose();
        }

        public unsafe void SetData(IntPtr ptr, int sourceOffset, int destinationOffset, int size)
        {
            //if size is null then no need to set any data
            if (size == 0)
                return;

            IntPtr mappedMemory;
            if (vkMapMemory(
                _device.LogicalDevice,
                _deviceMemory,
                0,
                _size,
                0,
                (void**)&mappedMemory
            ) != VkResult.Success)
                throw new System.Exception("failed to map vulkan memory");
            System.Buffer.MemoryCopy(
                IntPtr.Add(ptr, sourceOffset).ToPointer(),
                IntPtr.Add(mappedMemory, destinationOffset).ToPointer(),
                size, size
            );
            vkUnmapMemory(
                _device.LogicalDevice,
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
                _device.LogicalDevice,
                _deviceMemory,
                0,
                _size,
                0,
                (void**)&mappedMemory
            ) != VkResult.Success)
                throw new System.Exception("failed to map vulkan memory");
            System.Buffer.MemoryCopy(source.Data.ToPointer(), mappedMemory, _size, _size);
            vkUnmapMemory(
                _device.LogicalDevice,
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
                _device.LogicalDevice,
                _deviceMemory,
                0,
                _size,
                0,
                (void**)&mappedMemory
            ) != VkResult.Success)
                throw new System.Exception("failed to map vulkan memory");
            System.Buffer.MemoryCopy(mappedMemory, destination.Data.ToPointer(), _size, _size);
            vkUnmapMemory(
                _device.LogicalDevice,
                _deviceMemory
            );
            var data = new T[destination.Count];
            for (int i = 0; i < destination.Count; i++)
                data[i] = destination[i];
            return data;
        }

        public async Task SetDataWithStaging<T>(T[] data) where T : struct
        {
            //if new size is zero then return
            var size = Unsafe.SizeOf<T>() * data.Length;
            if (size == 0)
                return;

            _staging.SetData(data);
            await Task.Run(() =>
            {
                var fence = new Fence(_device);
                _transferToCommand.Submit(
                    _device.TransferQueueFamily.Queues[0],
                    null, null,
                    fence
                );
                fence.Wait();
            });
        }
        public async Task<T[]> GetDataWithStaging<T>() where T : struct
        {
            //setup transfer command
            var fence = new Fence(_device);
            _transferFromCommand.Submit(
                _device.TransferQueueFamily.Queues[0],
                null, null,
                fence
            );
            fence.Wait();
            return await Task.FromResult(_staging.GetData<T>());
        }

        internal BufferTransferObject GetTransferCmdForSetData<T>(T[] data) where T : struct
        {
            _staging.SetData(data);
            return new BufferTransferObject
            {
                commandPool = _commandPool,
                TransferCommand = _transferToCommand
            };
        }
    }
}