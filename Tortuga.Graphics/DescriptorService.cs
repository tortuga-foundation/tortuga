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
        private Dictionary<string, DescriptorObject> _descriptor;
        private GraphicsModule _module;

        /// <summary>
        /// Constructor for descriptor service
        /// </summary>
        public DescriptorService()
        {
            _descriptor = new Dictionary<string, DescriptorObject>();
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
            if (_descriptor.ContainsKey(key))
                return;

            var pool = new API.DescriptorPool(layout, 1);
            _descriptor[key] = new DescriptorObject
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
            if (_descriptor.ContainsKey(key) == false)
                return;

            _descriptor.Remove(key);
        }

        private void SetupBuffers(string key, int binding, uint size)
        {
            if (_descriptor.ContainsKey(key) == false)
                throw new InvalidOperationException("you must insert this key before using it");

            if (_descriptor[key].StagingBuffers[binding] != null)
            {
                if (_descriptor[key].StagingBuffers[binding].Size <= size)
                    return;
            }

            var device = _descriptor[key].Layout.Device;
            _descriptor[key].StagingBuffers[binding] = new API.Buffer(
                device,
                size,
                VkBufferUsageFlags.TransferSrc | VkBufferUsageFlags.TransferDst,
                VkMemoryPropertyFlags.HostCoherent | VkMemoryPropertyFlags.HostVisible
            );
            _descriptor[key].Buffers[binding] = new API.Buffer(
                device,
                size,
                VkBufferUsageFlags.TransferDst | VkBufferUsageFlags.UniformBuffer,
                VkMemoryPropertyFlags.DeviceLocal
            );
            _descriptor[key].CommandBuffer[binding] = _module.CommandBufferService.GetNewCommand(
                API.QueueFamilyType.Transfer,
                CommandType.Primary
            );

            //record command
            _descriptor[key].CommandBuffer[binding].Begin(VkCommandBufferUsageFlags.SimultaneousUse);
            _descriptor[key].CommandBuffer[binding].CopyBuffer(
                _descriptor[key].StagingBuffers[binding],
                _descriptor[key].Buffers[binding]
            );
            _descriptor[key].CommandBuffer[binding].End();
        }

        private void SetupImage(string key, int binding, uint size, uint width, uint height)
        {
            if (_descriptor.ContainsKey(key) == false)
                throw new InvalidOperationException("you must insert this key before using it");

            if (_descriptor[key].StagingBuffers[binding] != null)
            {
                if (_descriptor[key].StagingBuffers[binding].Size <= size)
                    return;
            }

            var device = _descriptor[key].Layout.Device;
            _descriptor[key].StagingBuffers[binding] = new API.Buffer(
                device,
                size,
                VkBufferUsageFlags.TransferSrc | VkBufferUsageFlags.TransferDst,
                VkMemoryPropertyFlags.HostCoherent | VkMemoryPropertyFlags.HostVisible
            );
            _descriptor[key].Images[binding] = new API.Image(
                device,
                width,
                height,
                VkFormat.R8g8b8a8Unorm,
                (
                    VkImageUsageFlags.TransferDst |
                    VkImageUsageFlags.TransferSrc |
                    VkImageUsageFlags.ColorAttachment |
                    VkImageUsageFlags.Sampled
                )
            );
            _descriptor[key].ImageViews[binding] = new API.ImageView(
                _descriptor[key].Images[binding],
                VkImageAspectFlags.Color
            );
            _descriptor[key].Samplers[binding] = new API.Sampler(
                device
            );
            _descriptor[key].CommandBuffer[binding] = _module.CommandBufferService.GetNewCommand(
                API.QueueFamilyType.Transfer,
                CommandType.Primary
            );

            //record command
            _descriptor[key].CommandBuffer[binding].Begin(VkCommandBufferUsageFlags.SimultaneousUse);
            _descriptor[key].CommandBuffer[binding].CopyBufferToImage(
                _descriptor[key].StagingBuffers[binding],
                _descriptor[key].Images[binding]
            );
            _descriptor[key].CommandBuffer[binding].End();
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
            _descriptor[key].StagingBuffers[binding].SetData(data);
            _module.CommandBufferService.Submit(
                _descriptor[key].CommandBuffer[binding]
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

            SetupImage(key, binding, size, width, height);
            _descriptor[key].StagingBuffers[binding].SetData(pixels);
            _module.CommandBufferService.Submit(
                _descriptor[key].CommandBuffer[binding]
            );
        }

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
            _descriptor[key].CommandBuffer[binding] = null;
            _descriptor[key].StagingBuffers[binding] = null;

            _descriptor[key].Images[binding] = image;
            _descriptor[key].ImageViews[binding] = imageView;
            _descriptor[key].Samplers[binding] = new API.Sampler(
                image.Device
            );
        }
    }
}