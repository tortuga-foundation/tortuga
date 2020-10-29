using System;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Vulkan;
using static Vulkan.VulkanNative;
using System.Runtime.InteropServices;

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


            //setup staging buffer
            if ((memoryProperty & VkMemoryPropertyFlags.HostVisible) == 0)
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
            Init(
                _device,
                Convert.ToUInt32(size),
                _bufferUsage,
                _memoryProperty
            );
        }

        private unsafe void SetData(
            IntPtr ptr,
            int sourceOffset,
            int destinationOffset,
            int size
        )
        {
            if (size == 0)
                return;
            if (size > _size)
                Console.WriteLine("[WARNING]: new data size is bigger than buffer size");

            IntPtr mappedMemory;
            if (vkMapMemory(
                _device.LogicalDevice,
                _deviceMemory,
                0,
                _size,
                0,
                (void**)&mappedMemory
            ) != VkResult.Success)
                throw new Exception("failed to map device memory");

            System.Buffer.MemoryCopy(
                IntPtr.Add(ptr, sourceOffset).ToPointer(),
                IntPtr.Add(mappedMemory, destinationOffset).ToPointer(),
                size, size
            );

            vkUnmapMemory(_device.LogicalDevice, _deviceMemory);
        }
        private unsafe IntPtr GetData()
        {
            var data = Marshal.AllocHGlobal(Convert.ToInt32(_size));

            IntPtr mappedMemory;
            if (vkMapMemory(
                _device.LogicalDevice,
                _deviceMemory,
                0,
                _size,
                0,
                (void**)&mappedMemory
            ) != VkResult.Success)
                throw new Exception("failed to map device memory");

            System.Buffer.MemoryCopy(
                mappedMemory.ToPointer(),
                data.ToPointer(),
                _size, _size
            );

            vkUnmapMemory(_device.LogicalDevice, _deviceMemory);
            return data;
        }

        public async Task SetData<T>(T[] data) where T : struct
        {
            if (data.Length == 0) return;
            var size = Unsafe.SizeOf<T>() * data.Length;
            if (size > _size)
            {
                Resize(size);
                size = Convert.ToInt32(_size);
            }

            if ((_memoryProperty & VkMemoryPropertyFlags.HostVisible) == 0)
            {
                //staging buffer is required
                unsafe
                {
                    _staging.SetData(
                        new IntPtr(Unsafe.AsPointer<T>(ref data[0])),
                        0, 0,
                        size
                    );
                }
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
            else
            {
                //set buffer data directly
                unsafe
                {
                    SetData(
                        new IntPtr(Unsafe.AsPointer<T>(ref data[0])),
                        0, 0,
                        size
                    );
                }
            }
        }

        public async Task<T[]> GetData<T>() where T : struct
        {
            var elementSize = Unsafe.SizeOf<T>();
            var data = new T[_size / elementSize];
            var ptr = IntPtr.Zero;

            if ((_memoryProperty & VkMemoryPropertyFlags.HostVisible) == 0)
            {
                //use staging buffer to get data
                await Task.Run(() =>
                {
                    var fence = new Fence(_device);
                    _transferFromCommand.Submit(
                        _device.TransferQueueFamily.Queues[0],
                        null, null,
                        fence
                    );
                    fence.Wait();
                });
                ptr = _staging.GetData();
            }
            else
            {
                //get data from buffer
                ptr = GetData();
            }
            //copy data to array
            unsafe
            {
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = Unsafe.Read<T>(
                        IntPtr.Add(ptr, i * elementSize).ToPointer()
                    );
                }
            }
            Marshal.FreeHGlobal(ptr);
            return data;
        }

        public BufferTransferObject GetTransferCmdForSetData<T>(T[] data) where T : struct
        {
            if (data.Length == 0)
                throw new Exception("data size is zero bytes");
            if ((_memoryProperty & VkMemoryPropertyFlags.HostVisible) != 0)
                throw new Exception("'GetTransferCCmdForSetData()' cannot be called for memory that is accessible by host system");
            var size = Unsafe.SizeOf<T>() * data.Length;
            if (size > _size)
            {
                Resize(size);
                size = Convert.ToInt32(_size);
            }

            unsafe
            {
                _staging.SetData(
                    new IntPtr(Unsafe.AsPointer<T>(ref data[0])),
                    0, 0,
                    size
                );
            }
            return new BufferTransferObject
            {
                commandPool = _commandPool,
                TransferCommand = _transferToCommand
            };
        }
    }
}