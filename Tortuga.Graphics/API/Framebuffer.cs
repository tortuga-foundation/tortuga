#pragma warning disable CS1591
using System;
using System.Collections.Generic;
using Tortuga.Utils;
using Vulkan;

namespace Tortuga.Graphics.API
{
    public class Framebuffer
    {
        public uint Width => _width;
        public uint Height => _height;
        public Device Device => _device;
        public RenderPass RenderPass => _renderPass;
        public VkFramebuffer Handle => _handle;
        public List<Image> Images => _images;
        public List<ImageView> ImageViews => _imageViews;

        private uint _width;
        private uint _height;
        private Device _device;
        private RenderPass _renderPass;
        private VkFramebuffer _handle;
        private List<Image> _images;
        private List<ImageView> _imageViews;

        public unsafe Framebuffer(
            RenderPass renderPass,
            uint width, uint height,
            uint layers = 1
        )
        {
            _width = width;
            _height = height;
            _device = renderPass.Device;
            _renderPass = renderPass;

            _images = new List<Image>();
            _imageViews = new List<ImageView>();
            foreach (var attachment in renderPass.Attachments)
            {
                var img = new Image(
                    _device,
                    width, height,
                    attachment.Format,
                    attachment.ImageUsageFlags,
                    1
                );
                _images.Add(img);
                _imageViews.Add(new ImageView(
                    img,
                    attachment.ImageAspectFlags
                ));
            }

            var attachments = new NativeList<VkImageView>();
            foreach (var view in _imageViews)
                attachments.Add(view.Handle);

            var framebufferCreateInfo = new VkFramebufferCreateInfo
            {
                sType = VkStructureType.FramebufferCreateInfo,
                renderPass = renderPass.Handle,
                attachmentCount = attachments.Count,
                pAttachments = (VkImageView*)attachments.Data.ToPointer(),
                width = width,
                height = height,
                layers = layers
            };

            VkFramebuffer framebuffer;
            if (VulkanNative.vkCreateFramebuffer(
                _device.Handle,
                &framebufferCreateInfo,
                null,
                &framebuffer
            ) != VkResult.Success)
                throw new Exception("failed to create framebuffer");
            _handle = framebuffer;
        }

        unsafe ~Framebuffer()
        {
            if (_handle != VkFramebuffer.Null)
            {
                VulkanNative.vkDestroyFramebuffer(
                    _device.Handle,
                    _handle,
                    null
                );
                _handle = VkFramebuffer.Null;
            }
        }
    }
}