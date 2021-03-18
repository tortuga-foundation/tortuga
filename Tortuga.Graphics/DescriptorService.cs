using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Vulkan;

namespace Tortuga.Graphics
{
    /// <summary>
    /// Responsible for setting up descriptor sets
    /// </summary>
    public class DescriptorService
    {
        /// <summary>
        /// Contains graphics api objects required for setting up descriptor sets
        /// </summary>
        public struct DescriptorObject
        {
            internal API.DescriptorLayout Layout;
            internal API.DescriptorPool Pool;
            internal API.DescriptorSet Set;
            internal API.Buffer[] StagingBuffers;
            internal API.Buffer[] Buffers;
            internal API.Image[] Images;
            internal API.ImageView[] ImageViews;
            internal API.Sampler[] Samplers;
            internal API.CommandBuffer[] CommandBuffer;
        }
        /// <summary>
        /// All of the descriptor set created by the service
        /// </summary>
        public Dictionary<string, DescriptorObject> Handle => _handle;
        /// <summary>
        /// All of the descriptor set created by the service
        /// </summary>
        protected Dictionary<string, DescriptorObject> _handle;
        private GraphicsModule _module;

        /// <summary>
        /// Constructor for descriptor service
        /// </summary>
        public DescriptorService()
        {
            _handle = new Dictionary<string, DescriptorObject>();
            _module = Engine.Instance.GetModule<GraphicsModule>();
        }

        /// <summary>
        /// Create a new descriptor type binding
        /// </summary>
        /// <param name="key">key for this descriptor set</param>
        /// <param name="layout">what type of data does this descriptor set have?</param>
        public virtual void InsertKey(
            string key,
            API.DescriptorLayout layout
        )
        {
            if (_handle.ContainsKey(key))
                return;

            var pool = new API.DescriptorPool(layout, 1);
            _handle[key] = new DescriptorObject
            {
                Layout = layout,
                Pool = pool,
                Set = new API.DescriptorSet(pool),
                StagingBuffers = new API.Buffer[layout.Bindings.Count],
                Buffers = new API.Buffer[layout.Bindings.Count],
                Images = new API.Image[layout.Bindings.Count],
                ImageViews = new API.ImageView[layout.Bindings.Count],
                Samplers = new API.Sampler[layout.Bindings.Count],
                CommandBuffer = new API.CommandBuffer[layout.Bindings.Count]
            };
        }

        /// <summary>
        /// Remvoe an existing descriptor set
        /// </summary>
        /// <param name="key">key of the descriptor set</param>
        public virtual void RemoveKey(string key)
        {
            if (_handle.ContainsKey(key) == false)
                return;

            _handle.Remove(key);
        }

        private void TransferImageAndUpdateDescriptorSet(string key, int binding)
        {
            //update image layout for descriptor set
            var transferImageFence = new API.Fence(_module.GraphicsService.PrimaryDevice);
            var transferImageCommand = _module.CommandBufferService.GetNewCommand(
                API.QueueFamilyType.Graphics,
                CommandType.Primary
            );
            transferImageCommand.Begin(VkCommandBufferUsageFlags.OneTimeSubmit);
            transferImageCommand.TransferImageLayoutOnAllMipMaps(_handle[key].Images[binding], VkImageLayout.ShaderReadOnlyOptimal);
            transferImageCommand.End();
            _module.CommandBufferService.Submit(
                transferImageCommand,
                null, null,
                transferImageFence
            );
            transferImageFence.Wait();

            _handle[key].Set.UpdateSampledImage(
                _handle[key].ImageViews[binding],
                _handle[key].Samplers[binding],
                binding
            );
        }

        private void SetupBuffers(string key, int binding, uint size)
        {
            if (_handle.ContainsKey(key) == false)
                throw new InvalidOperationException("you must insert this key before using it");

            if (_handle[key].StagingBuffers[binding] != null)
            {
                if (_handle[key].StagingBuffers[binding].Size <= size)
                    return;
            }

            var device = _handle[key].Layout.Device;
            _handle[key].StagingBuffers[binding] = new API.Buffer(
                device,
                size,
                VkBufferUsageFlags.TransferSrc | VkBufferUsageFlags.TransferDst,
                VkMemoryPropertyFlags.HostCoherent | VkMemoryPropertyFlags.HostVisible
            );
            _handle[key].Buffers[binding] = new API.Buffer(
                device,
                size,
                VkBufferUsageFlags.TransferDst | VkBufferUsageFlags.UniformBuffer,
                VkMemoryPropertyFlags.DeviceLocal
            );
            _handle[key].CommandBuffer[binding] = _module.CommandBufferService.GetNewCommand(
                API.QueueFamilyType.Transfer,
                CommandType.Primary
            );
            _handle[key].Set.UpdateBuffer(
                _handle[key].Buffers[binding],
                binding
            );

            //record command
            _handle[key].CommandBuffer[binding].Begin(VkCommandBufferUsageFlags.SimultaneousUse);
            _handle[key].CommandBuffer[binding].CopyBuffer(
                _handle[key].StagingBuffers[binding],
                _handle[key].Buffers[binding]
            );
            _handle[key].CommandBuffer[binding].End();
        }

        private void SetupImage(
            string key,
            int binding,
            uint size,
            uint width, uint height,
            VkFormat format)
        {
            if (_handle.ContainsKey(key) == false)
                throw new InvalidOperationException("you must insert this key before using it");

            if (_handle[key].StagingBuffers[binding] != null)
            {
                if (_handle[key].StagingBuffers[binding].Size <= size)
                    return;
            }

            var device = _handle[key].Layout.Device;
            _handle[key].StagingBuffers[binding] = new API.Buffer(
                device,
                size,
                VkBufferUsageFlags.TransferSrc | VkBufferUsageFlags.TransferDst,
                VkMemoryPropertyFlags.HostCoherent | VkMemoryPropertyFlags.HostVisible
            );
            _handle[key].Images[binding] = new API.Image(
                device,
                width,
                height,
                format,
                (
                    VkImageUsageFlags.TransferDst |
                    VkImageUsageFlags.TransferSrc |
                    VkImageUsageFlags.ColorAttachment |
                    VkImageUsageFlags.Sampled
                )
            );
            _handle[key].ImageViews[binding] = new API.ImageView(
                _handle[key].Images[binding],
                VkImageAspectFlags.Color
            );
            _handle[key].Samplers[binding] = new API.Sampler(
                device
            );
            _handle[key].CommandBuffer[binding] = _module.CommandBufferService.GetNewCommand(
                API.QueueFamilyType.Transfer,
                CommandType.Primary
            );
            TransferImageAndUpdateDescriptorSet(key, binding);

            //record command
            _handle[key].CommandBuffer[binding].Begin(VkCommandBufferUsageFlags.SimultaneousUse);
            _handle[key].CommandBuffer[binding].TransferImageLayout(
                _handle[key].Images[binding],
                VkImageLayout.TransferDstOptimal
            );
            _handle[key].CommandBuffer[binding].CopyBufferToImage(
                _handle[key].StagingBuffers[binding],
                _handle[key].Images[binding]
            );
            _handle[key].CommandBuffer[binding].GenerateMipMaps(_handle[key].Images[binding]);
            _handle[key].CommandBuffer[binding].End();
        }

        /// <summary>
        /// bind a buffer to this descriptor set
        /// </summary>
        public virtual void BindBuffer<T>(string key, int binding, T[] data) where T : struct
        {
            if (data == null)
                return;

            var size = Convert.ToUInt32(
                Unsafe.SizeOf<T>() * data.Length
            );
            SetupBuffers(key, binding, size);
            _handle[key].StagingBuffers[binding].SetData(data);
            _module.CommandBufferService.Submit(
                _handle[key].CommandBuffer[binding]
            );
        }

        /// <summary>
        /// bind an image to this descriptor set
        /// </summary>
        public virtual void BindImage<T>(
            string key,
            int binding,
            T[] pixels,
            uint width,
            uint height,
            VkFormat format = VkFormat.R8g8b8a8Unorm,
            int elementPerPixel = 1
        ) where T : struct
        {
            if (pixels == null)
                return;
            if (width <= 0 || height <= 0 || elementPerPixel <= 0)
                return;

            var size = Convert.ToUInt32(
                Unsafe.SizeOf<T>() * pixels.Length
            );

            SetupImage(key, binding, size, width, height, format);
            _handle[key].StagingBuffers[binding].SetData(pixels);
            _module.CommandBufferService.Submit(
                _handle[key].CommandBuffer[binding]
            );
        }

        /// <summary>
        /// Bind a texture to the material
        /// </summary>
        public virtual void BindImage(
            string key,
            int binding,
            Texture texture,
            VkFormat format = VkFormat.R8g8b8a8Unorm
        ) => BindImage(
            key,
            binding,
            texture.Pixels,
            (uint)texture.Width,
            (uint)texture.Height,
            format
        );

        /// <summary>
        /// bind an existing image to this descriptor set
        /// </summary>
        /// <param name="key">the key for this descriptor set</param>
        /// <param name="binding">binding id for this descriptor set</param>
        /// <param name="image">image to bind with this descriptor set</param>
        /// <param name="imageView">image view to bind with this descriptor set</param>
        public virtual void BindImage(
            string key,
            int binding,
            API.Image image,
            API.ImageView imageView
        )
        {
            if (image == null)
                return;

            //un-used variables
            _handle[key].CommandBuffer[binding] = null;
            _handle[key].StagingBuffers[binding] = null;

            _handle[key].Images[binding] = image;
            _handle[key].ImageViews[binding] = imageView;
            _handle[key].Samplers[binding] = new API.Sampler(
                image.Device
            );

            TransferImageAndUpdateDescriptorSet(key, binding);
        }

        /// <summary>
        /// transfers images being used by the mesh to correct layout for rendering
        /// NOTE: you need to submit the command yourself
        /// </summary>
        public API.CommandBuffer TransferImages(Vulkan.VkImageLayout layout = VkImageLayout.ShaderReadOnlyOptimal)
        {
            var transferCommand = _module.CommandBufferService.GetNewCommand(
                API.QueueFamilyType.Graphics,
                CommandType.Primary
            );
            transferCommand.Begin(VkCommandBufferUsageFlags.OneTimeSubmit);
            //transfer images to correct layouts
            foreach (var o in _handle)
            {
                if (o.Value.Images == null) continue;

                foreach (var image in o.Value.Images)
                {
                    if (image == null) continue;
                    if (image.Layout == layout) continue;

                    transferCommand.TransferImageLayout(
                        image,
                        layout
                    );
                }
            }
            transferCommand.End();
            return transferCommand;
        }

        /// <summary>
        /// Clear's all bound textures and buffers
        /// </summary>
        public void Clear()
        {
            _handle.Clear();
        }
    }
}