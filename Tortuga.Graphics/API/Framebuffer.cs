using System;
using Vulkan;
using Tortuga.Utils;
using static Vulkan.VulkanNative;
using System.Collections.Generic;

namespace Tortuga.Graphics.API
{
    internal class Framebuffer
    {
        public VkFramebuffer Handle => _frameBuffer;
        public Image[] AttachmentImages => _attachmentImages;
        public ImageView[] AttachmentViews => _attachmentViews;
        public uint Width => _width;
        public uint Height => _height;

        private Image[] _attachmentImages;
        private ImageView[] _attachmentViews;
        private VkFramebuffer _frameBuffer;
        private uint _width, _height;
        private Device _device;

        public unsafe Framebuffer(RenderPass renderPass, uint width, uint height)
        {
            _width = width;
            _height = height;
            _device = renderPass.DeviceInUse;

            var attachmentImages = new List<Image>();
            var attachmentViews = new List<ImageView>();
            for (int i = 0; i < renderPass.ColorAttachments.Length; i++)
            {
                var img = new Image(
                    _device,
                    width, height,
                    RenderPass.DEFAULT_COLOR_FORMAT,
                    VkImageUsageFlags.ColorAttachment | VkImageUsageFlags.TransferSrc | VkImageUsageFlags.TransferDst
                );
                attachmentImages.Add(img);
                attachmentViews.Add(new ImageView(
                    img,
                    VkImageAspectFlags.Color
                ));
            }
            if (renderPass.DepthAttachment != null)
            {
                var img = new Image(
                    _device,
                    width, height,
                    RenderPass.DEFAULT_DEPTH_FORMAT,
                    VkImageUsageFlags.DepthStencilAttachment | VkImageUsageFlags.TransferSrc | VkImageUsageFlags.TransferDst
                );
                attachmentImages.Add(img);
                attachmentViews.Add(new ImageView(
                    img,
                    VkImageAspectFlags.Depth
                ));
            }
            _attachmentImages = attachmentImages.ToArray();
            _attachmentViews = attachmentViews.ToArray();

            var attachments = new NativeList<VkImageView>();
            foreach (var views in _attachmentViews)
                attachments.Add(views.Handle);

            var framebufferCreateInfo = VkFramebufferCreateInfo.New();
            framebufferCreateInfo.renderPass = renderPass.Handle;
            framebufferCreateInfo.attachmentCount = attachments.Count;
            framebufferCreateInfo.pAttachments = (VkImageView*)attachments.Data.ToPointer();
            framebufferCreateInfo.width = width;
            framebufferCreateInfo.height = height;
            framebufferCreateInfo.layers = 1;

            VkFramebuffer frameBuffer;
            if (vkCreateFramebuffer(
                _device.LogicalDevice,
                &framebufferCreateInfo,
                null,
                &frameBuffer
            ) != VkResult.Success)
                throw new Exception("failed to create framebuffer");
            _frameBuffer = frameBuffer;
        }

        unsafe ~Framebuffer()
        {
            vkDestroyFramebuffer(
                _device.LogicalDevice,
                _frameBuffer,
                null
            );
        }
    }
}