#pragma warning disable CS1591
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Vulkan;

namespace Tortuga.Graphics.API
{
    public class Buffer
    {
        public uint Size => _size;
        public Device Device => _device;
        public VkBufferUsageFlags BufferUsageFlags => _bufferUsage;
        public VkMemoryPropertyFlags MemoryPropertyFlags => _memoryProperty;
        public VkMemoryRequirements MemoryRequirements => _memoryRequirements;
        public VkDeviceMemory MemoryHandle => _memoryHandle;
        public VkBuffer Handle => _handle;

        private uint _size;
        private Device _device;
        private VkBufferUsageFlags _bufferUsage;
        private VkMemoryPropertyFlags _memoryProperty;
        private VkMemoryRequirements _memoryRequirements;
        private VkDeviceMemory _memoryHandle;
        private VkBuffer _handle;

        public unsafe Buffer(
            Device device,
            uint size,
            VkBufferUsageFlags bufferUsageFlags,
            VkMemoryPropertyFlags memoryProperty
        )
        {
            if (size == 0)
                throw new Exception("cannot create buffer with size of zero bytes");

            //make sure buffer supports transfer data to and from buffer
            if ((bufferUsageFlags & VkBufferUsageFlags.TransferSrc) == 0)
                bufferUsageFlags |= VkBufferUsageFlags.TransferSrc;
            if ((bufferUsageFlags & VkBufferUsageFlags.TransferDst) == 0)
                bufferUsageFlags |= VkBufferUsageFlags.TransferDst;

            //store parameter information
            _size = size;
            _device = device;
            _bufferUsage = bufferUsageFlags;
            _memoryProperty = memoryProperty;

            //buffer create info
            var bufferCreateInfo = new VkBufferCreateInfo
            {
                sType = VkStructureType.BufferCreateInfo,
                size = size,
                usage = bufferUsageFlags,
                sharingMode = VkSharingMode.Exclusive
            };

            //setup buffer handler
            VkBuffer buffer;
            if (VulkanNative.vkCreateBuffer(
                device.Handle,
                &bufferCreateInfo,
                null,
                &buffer
            ) != VkResult.Success)
                throw new Exception("failed to create vulkan buffer handle");
            _handle = buffer;

            //memory allocation info
            _memoryRequirements = GetMemoryRequirements(device.Handle, buffer);
            var memoryAllocateInfo = new VkMemoryAllocateInfo
            {
                sType = VkStructureType.MemoryAllocateInfo,
                allocationSize = _memoryRequirements.size,
                memoryTypeIndex = device.FindMemoryType(
                    _memoryRequirements.memoryTypeBits,
                    _memoryProperty
                )
            };

            //setup device memory
            VkDeviceMemory deviceMemory;
            if (VulkanNative.vkAllocateMemory(
                device.Handle,
                &memoryAllocateInfo,
                null,
                &deviceMemory
            ) != VkResult.Success)
                throw new Exception("failed to allocat device memory");
            _memoryHandle = deviceMemory;

            //bind buffer handle with device memory
            if (VulkanNative.vkBindBufferMemory(
                device.Handle,
                buffer,
                deviceMemory,
                0
            ) != VkResult.Success)
                throw new Exception("failed to bind buffer handler to device memory");
        }

        unsafe ~Buffer()
        {
            if (_handle != VkBuffer.Null)
            {
                VulkanNative.vkDestroyBuffer(
                    _device.Handle,
                    _handle,
                    null
                );
                _handle = VkBuffer.Null;
            }
            if (_memoryHandle != VkDeviceMemory.Null)
            {
                VulkanNative.vkFreeMemory(
                    _device.Handle,
                    _memoryHandle,
                    null
                );
                _memoryHandle = VkDeviceMemory.Null;
            }
        }

        public unsafe VkMemoryRequirements GetMemoryRequirements(
            VkDevice device,
            VkBuffer buffer
        )
        {
            VkMemoryRequirements memoryRequirements;
            VulkanNative.vkGetBufferMemoryRequirements(
                device,
                buffer,
                &memoryRequirements
            );
            return memoryRequirements;
        }
        public unsafe void SetData(IntPtr ptr, int size)
        {
            if (size == 0)
            {
                Console.WriteLine("[WARNING]: cannot set buffer data with zero bytes");
                return;
            }
            if (size > _size)
            {
                Console.WriteLine("[WARNING]: new data size is bigger than buffer size");
                return;
            }

            var mappedMemory = IntPtr.Zero;
            if (VulkanNative.vkMapMemory(
                _device.Handle,
                _memoryHandle,
                0,
                _size,
                0,
                (void**)&mappedMemory
            ) != VkResult.Success)
                throw new Exception("failed to map device memory");

            System.Buffer.MemoryCopy(
                IntPtr.Add(ptr, 0).ToPointer(),
                IntPtr.Add(mappedMemory, 0).ToPointer(),
                size, size
            );

            VulkanNative.vkUnmapMemory(_device.Handle, _memoryHandle);
        }
        public unsafe IntPtr GetData()
        {
            var data = Marshal.AllocHGlobal(Convert.ToInt32(_size));
            var mappedMemory = IntPtr.Zero;
            if (VulkanNative.vkMapMemory(
                _device.Handle,
                _memoryHandle,
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

            VulkanNative.vkUnmapMemory(_device.Handle, _memoryHandle);
            return data;
        }
        public unsafe void SetData<T>(T[] data) where T : struct
        => SetData(
            new IntPtr(Unsafe.AsPointer<T>(ref data[0])),
            Unsafe.SizeOf<T>() * data.Length
        );
        public unsafe T[] GetData<T>() where T : struct
        {
            var elementSize = Unsafe.SizeOf<T>();
            var data = new T[_size / elementSize];
            var ptr = GetData();
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = Unsafe.Read<T>(
                    IntPtr.Add(ptr, i * elementSize).ToPointer()
                );
            }
            Marshal.FreeHGlobal(ptr);
            return data;
        }
    }
}