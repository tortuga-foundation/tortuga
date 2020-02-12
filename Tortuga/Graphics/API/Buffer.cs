using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Vulkan;
using static Vulkan.VulkanNative;

namespace Tortuga.Graphics.API
{
    internal class Buffer
    {
        public VkBuffer Handle => _buffer;
        public uint Size => _size;

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

        unsafe void SetData<T>(T data) where T : struct
        {
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
            Unsafe.Copy<T>(mappedMemory, ref data);
            vkUnmapMemory(
                Engine.Instance.MainDevice.LogicalDevice,
                _deviceMemory
            );
        }
        unsafe T GetData<T>() where T : struct
        {
            T data = new T();
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
            Unsafe.Copy<T>(ref data, mappedMemory);
            vkUnmapMemory(
                Engine.Instance.MainDevice.LogicalDevice,
                _deviceMemory
            );
            return data;
        }

        public async Task SetDataWithStaging<T>(T data) where T : struct
        {
            await Task.Run(() =>
            {
                var copyWaitFence = new Fence();
                var staging = Buffer.CreateHost(this._size, VkBufferUsageFlags.TransferSrc);
                staging.SetData(data);
                var copyPool = new CommandPool(
                    Engine.Instance.MainDevice.TransferQueueFamily
                );
                var copyCommand = copyPool.AllocateCommands()[0];
                copyCommand.Begin(VkCommandBufferUsageFlags.OneTimeSubmit);
                copyCommand.CopyBuffer(staging, this);
                copyCommand.End();
                copyCommand.Submit(
                    Engine.Instance.MainDevice.TransferQueueFamily.Queues[0],
                    null, null, copyWaitFence
                );
                copyWaitFence.Wait();
            });
        }
        public async Task<T> GetDataWithStaging<T>() where T : struct
        {
            var copyWaitFence = new Fence();
            var staging = Buffer.CreateHost(this._size, VkBufferUsageFlags.TransferSrc);
            var copyPool = new CommandPool(
                Engine.Instance.MainDevice.TransferQueueFamily
            );
            var copyCommand = copyPool.AllocateCommands()[0];
            copyCommand.Begin(VkCommandBufferUsageFlags.OneTimeSubmit);
            copyCommand.CopyBuffer(this, staging);
            copyCommand.End();
            copyCommand.Submit(
                Engine.Instance.MainDevice.TransferQueueFamily.Queues[0],
                null, null, copyWaitFence
            );
            copyWaitFence.Wait();

            return await Task.FromResult(staging.GetData<T>());
        }
    }
}