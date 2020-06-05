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
        public Buffer StagingBuffer;
    };

    internal class Buffer
    {
        public VkBuffer Handle => _buffer;
        public uint Size => _size;
        public Device DeviceUsed => _device;

        private VkBuffer _buffer;
        private VkMemoryRequirements _memoryRequirements;
        private VkDeviceMemory _deviceMemory;
        private uint _size;
        private Buffer _staging;
        private CommandPool _commandPool;
        private CommandPool.Command _transferToCommand;
        private CommandPool.Command _transferFromCommand;
        private Device _device;

        public unsafe Buffer(Device device, uint size, VkBufferUsageFlags usageFlags, VkMemoryPropertyFlags memoryProperties, bool isStaging = false)
        {
            _device = device;
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
                _device.LogicalDevice,
                &bufferCreateInfo,
                null,
                &buffer
            ) != VkResult.Success)
                throw new System.Exception("failed to create vulkan buffer");
            _buffer = buffer;

            VkMemoryRequirements memoryRequirements;
            vkGetBufferMemoryRequirements(
                _device.LogicalDevice,
                _buffer,
                &memoryRequirements
            );
            _memoryRequirements = memoryRequirements;

            var allocInfo = VkMemoryAllocateInfo.New();
            allocInfo.allocationSize = _memoryRequirements.size;
            allocInfo.memoryTypeIndex = _device.FindMemoryType(
                _memoryRequirements.memoryTypeBits,
                memoryProperties
            );

            VkDeviceMemory deviceMemory;
            if (vkAllocateMemory(
                _device.LogicalDevice,
                &allocInfo,
                null,
                &deviceMemory
            ) != VkResult.Success)
                throw new System.Exception("failed to allocate device memory for vulkan buffer");
            _deviceMemory = deviceMemory;
            if (vkBindBufferMemory(
                _device.LogicalDevice,
                _buffer,
                _deviceMemory,
                0
            ) != VkResult.Success)
                throw new System.Exception("failed to bind buffer handle to device memory");


            //setup staging buffer
            if (!isStaging)
            {
                _staging = new Buffer(
                    _device,
                    _size,
                    VkBufferUsageFlags.TransferSrc | VkBufferUsageFlags.TransferDst,
                    VkMemoryPropertyFlags.HostVisible | VkMemoryPropertyFlags.HostCoherent,
                    true
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

        public static Buffer CreateHost(Device device, uint size, VkBufferUsageFlags usageFlags)
            => new Buffer(
            device,
            size,
            usageFlags,
            VkMemoryPropertyFlags.HostVisible | VkMemoryPropertyFlags.HostCoherent,
            true
        );

        public static Buffer CreateDevice(Device device, uint size, VkBufferUsageFlags usageFlags)
            => new Buffer(
            device,
            size,
            usageFlags,
            VkMemoryPropertyFlags.DeviceLocal,
            false
        );

        public unsafe void SetData(IntPtr ptr, int offset, int size)
        {
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
                IntPtr.Add(ptr, offset).ToPointer(), 
                IntPtr.Add(mappedMemory, offset).ToPointer(),
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