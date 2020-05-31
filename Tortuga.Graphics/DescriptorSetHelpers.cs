#pragma warning disable 1591
using System;
using System.Drawing;
using System.Collections.Generic;
using Vulkan;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Numerics;

namespace Tortuga.Graphics
{

    public class DescriptorSetHelper
    {
        public struct DescriptorObject
        {
            internal API.DescriptorSetLayout Layout;
            internal API.DescriptorSetPool Pool;
            internal API.DescriptorSetPool.DescriptorSet Set;
            internal API.Buffer[] Buffers;
            internal API.Image[] Images;
            internal API.ImageView[] Views;
            internal API.Sampler[] Sampler;
            internal API.CommandPool CommandPool;
            internal API.CommandPool.Command Command;
            internal API.Fence WaitFence;
        }
        internal Dictionary<string, DescriptorObject> DescriptorObjectMapper => _descriptorObjectMapper;
        private Dictionary<string, DescriptorObject> _descriptorObjectMapper;

        public DescriptorSetHelper()
        {
            _descriptorObjectMapper = new Dictionary<string, DescriptorObject>();
        }

        public void InsertKey(string key, API.DescriptorSetLayout layout)
        {
            if (_descriptorObjectMapper.ContainsKey(key) == false)
            {
                var pool = new API.DescriptorSetPool(layout);
                var commandPool = new API.CommandPool(layout.DeviceUsed, layout.DeviceUsed.GraphicsQueueFamily);
                _descriptorObjectMapper[key] = new DescriptorObject
                {
                    Layout = layout,
                    Pool = pool,
                    Set = pool.AllocateDescriptorSet(),
                    Buffers = new API.Buffer[layout.CreateInfoUsed.Length],
                    Images = new API.Image[layout.CreateInfoUsed.Length],
                    Views = new API.ImageView[layout.CreateInfoUsed.Length],
                    Sampler = new API.Sampler[layout.CreateInfoUsed.Length],
                    CommandPool = commandPool,
                    Command = commandPool.AllocateCommands()[0],
                    WaitFence = new API.Fence(layout.DeviceUsed)
                };
            }
        }

        public void RemoveKey(string key)
        {
            if (_descriptorObjectMapper.ContainsKey(key))
                _descriptorObjectMapper.Remove(key);
        }

        /// <summary>
        /// Set's up an image on the GPU so it can be used by the shader
        /// If you want to leave the image blank then set pixels to null
        /// </summary>
        /// <param name="key">key identifier for this descriptor set</param>
        /// <param name="binding">binding for the descriptor set</param>
        /// <param name="pixels">each pixel of the iamge</param>
        /// <param name="width">width of the image</param>
        /// <param name="height">height of the image</param>
        public Task BindImage(string key, int binding, ShaderPixel[] pixels, int width, int height)
        {
            return Task.Run(() =>
            {
                if (pixels != null && pixels.Length != width * height)
                    throw new InvalidOperationException("color length does not match width and height");

                if (_descriptorObjectMapper.ContainsKey(key) == false)
                    throw new InvalidOperationException("you must setup the descriptor set by using InitializeLayout");

                bool createNew = false;
                if (_descriptorObjectMapper[key].Images[binding] == null)
                    createNew = true;
                else if (
                    _descriptorObjectMapper[key].Images[binding].Width != width ||
                    _descriptorObjectMapper[key].Images[binding].Height != height
                ) createNew = true;

                if (createNew)
                {
                    _descriptorObjectMapper[key].Buffers[binding] = API.Buffer.CreateHost(
                        _descriptorObjectMapper[key].Layout.DeviceUsed,
                        Convert.ToUInt32(width * height * Unsafe.SizeOf<ShaderPixel>()),
                        VkBufferUsageFlags.TransferSrc | VkBufferUsageFlags.TransferDst
                    );
                    _descriptorObjectMapper[key].Images[binding] = new API.Image(
                        _descriptorObjectMapper[key].Layout.DeviceUsed,
                        Convert.ToUInt32(width),
                        Convert.ToUInt32(height),
                        VkFormat.R8g8b8a8Unorm,
                        (
                            VkImageUsageFlags.TransferDst |
                            VkImageUsageFlags.TransferSrc |
                            VkImageUsageFlags.ColorAttachment |
                            VkImageUsageFlags.Sampled
                        )
                    );
                    _descriptorObjectMapper[key].Views[binding] = new API.ImageView(
                        _descriptorObjectMapper[key].Images[binding],
                        VkImageAspectFlags.Color
                    );
                    _descriptorObjectMapper[key].Sampler[binding] = new API.Sampler(
                        _descriptorObjectMapper[key].Layout.DeviceUsed
                    );
                    _descriptorObjectMapper[key].Set.UpdateSampledImage(
                        VkImageLayout.ShaderReadOnlyOptimal,
                        _descriptorObjectMapper[key].Views[binding],
                        _descriptorObjectMapper[key].Sampler[binding],
                        Convert.ToUInt32(binding)
                    );
                }

                if (pixels != null)
                {
                    _descriptorObjectMapper[key].Buffers[binding].SetData(pixels);
                    _descriptorObjectMapper[key].Command.Begin(
                        VkCommandBufferUsageFlags.OneTimeSubmit
                    );
                    _descriptorObjectMapper[key].Command.TransferImageLayout(
                        _descriptorObjectMapper[key].Images[binding],
                        VkImageLayout.Undefined,
                        VkImageLayout.TransferDstOptimal
                    );
                    _descriptorObjectMapper[key].Command.BufferToImage(
                        _descriptorObjectMapper[key].Buffers[binding],
                        _descriptorObjectMapper[key].Images[binding]
                    );
                    _descriptorObjectMapper[key].Command.End();
                    _descriptorObjectMapper[key].Command.Submit(
                        _descriptorObjectMapper[key].Layout.DeviceUsed.GraphicsQueueFamily.Queues[0],
                        null, null,
                        _descriptorObjectMapper[key].WaitFence
                    );
                    _descriptorObjectMapper[key].WaitFence.Wait();
                }
            });
        }

        private void BufferSetup(string key, int binding, byte[] data, int size = -1)
        {
            int actualSize = size;
            if (data != null)
                actualSize = sizeof(byte) * data.Length;
            if (actualSize < 1)
                throw new InvalidOperationException("invalid parameters passed to the function, you must include data or have a valid buffer size (greater than 0)");

            if (_descriptorObjectMapper.ContainsKey(key) == false)
                throw new InvalidOperationException("you must setup the descriptor set by using InitializeLayout");

            bool createNew = false;
            if (_descriptorObjectMapper[key].Buffers[binding] == null)
                createNew = true;
            else if (actualSize != _descriptorObjectMapper[key].Buffers[binding].Size) createNew = true;

            if (createNew)
            {
                _descriptorObjectMapper[key].Buffers[binding] = API.Buffer.CreateDevice(
                    _descriptorObjectMapper[key].Layout.DeviceUsed,
                    Convert.ToUInt32(actualSize),
                    VkBufferUsageFlags.TransferSrc | VkBufferUsageFlags.TransferDst | VkBufferUsageFlags.UniformBuffer
                );
                _descriptorObjectMapper[key].Set.UpdateBuffer(
                    _descriptorObjectMapper[key].Buffers[binding],
                    Convert.ToUInt32(binding)
                );
            }
        }

        /// <summary>
        /// Set's up an buffer on the GPU so your data can be passed to the shader
        /// If you want to leave the buffer blank set data to null
        /// and specify the size 
        /// </summary>
        /// <param name="key">key identifier for the descriptor set</param>
        /// <param name="binding">binding for the descriptor set</param>
        /// <param name="data">data to transfer to the shader</param>
        /// <param name="size">size of the data, only required if data is null</param>
        public async Task BindBuffer(string key, int binding, byte[] data, int size = -1)
        {
            this.BufferSetup(key, binding, data, size);
            if (data != null)
                await _descriptorObjectMapper[key].Buffers[binding].SetDataWithStaging(data);
        }

        internal API.BufferTransferObject BindBufferWithTransferObject(string key, int binding, byte[] data)
        {
            if (data == null)
                throw new InvalidOperationException("data cannot be null");

            this.BufferSetup(key, binding, data, -1);
            return _descriptorObjectMapper[key].Buffers[binding].SetDataGetTransferObject(data);
        }

        public void RemoveBinding(string key, int binding)
        {
            _descriptorObjectMapper[key].Buffers[binding] = null;
            _descriptorObjectMapper[key].Images[binding] = null;
            _descriptorObjectMapper[key].Views[binding] = null;
            _descriptorObjectMapper[key].Sampler[binding] = null;
        }

        public static byte[] MatrixToBytes(Matrix4x4 mat)
        {
            var bytes = new List<byte>();
            //1
            foreach (var b in BitConverter.GetBytes(mat.M11))
                bytes.Add(b);
            foreach (var b in BitConverter.GetBytes(mat.M12))
                bytes.Add(b);
            foreach (var b in BitConverter.GetBytes(mat.M13))
                bytes.Add(b);
            foreach (var b in BitConverter.GetBytes(mat.M14))
                bytes.Add(b);
            //2
            foreach (var b in BitConverter.GetBytes(mat.M21))
                bytes.Add(b);
            foreach (var b in BitConverter.GetBytes(mat.M22))
                bytes.Add(b);
            foreach (var b in BitConverter.GetBytes(mat.M23))
                bytes.Add(b);
            foreach (var b in BitConverter.GetBytes(mat.M24))
                bytes.Add(b);

            //3
            foreach (var b in BitConverter.GetBytes(mat.M31))
                bytes.Add(b);
            foreach (var b in BitConverter.GetBytes(mat.M32))
                bytes.Add(b);
            foreach (var b in BitConverter.GetBytes(mat.M33))
                bytes.Add(b);
            foreach (var b in BitConverter.GetBytes(mat.M34))
                bytes.Add(b);

            //4
            foreach (var b in BitConverter.GetBytes(mat.M41))
                bytes.Add(b);
            foreach (var b in BitConverter.GetBytes(mat.M42))
                bytes.Add(b);
            foreach (var b in BitConverter.GetBytes(mat.M43))
                bytes.Add(b);
            foreach (var b in BitConverter.GetBytes(mat.M44))
                bytes.Add(b);

            return bytes.ToArray();
        }
    }
}