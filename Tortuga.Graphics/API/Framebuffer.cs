using System;
using Vulkan;
using Tortuga.Utils;
using static Vulkan.VulkanNative;

namespace Tortuga.Graphics.API
{
    internal class Framebuffer
    {
        public VkFramebuffer Handle => _frameBuffer;
        public Image[] AttachmentImages => _attachmentImages;
        public ImageView[] AttachmentImageViews => _attachmentImageViews;
        public uint Width => _width;
        public uint Height => _height;
        public Device DeviceUsed => _device;

        private Image[] _attachmentImages;
        private ImageView[] _attachmentImageViews;
        private VkFramebuffer _frameBuffer;
        private uint _width, _height;
        private Device _device;

        public unsafe Framebuffer(Device device, RenderPass renderPass, uint width, uint height)
        {
            _device = device;
            _width = width;
            _height = height;

            var len = renderPass.AttachmentInfo.Length;
            _attachmentImages = new Image[len];
            _attachmentImageViews = new ImageView[len];
            for (int i = 0; i < len; i++)
            {
                if (renderPass.AttachmentInfo[i] == RenderPass.RenderPassAttachmentType.Image)
                {
                    _attachmentImages[i] = new Image(
                        _device,
                        width, height,
                        RenderPass.DEFAULT_COLOR_IMAGE_FORMAT,
                        VkImageUsageFlags.ColorAttachment | VkImageUsageFlags.TransferSrc | VkImageUsageFlags.TransferDst
                    );
                    _attachmentImageViews[i] = new ImageView(
                        _attachmentImages[i],
                        VkImageAspectFlags.Color
                    );
                }
                else if (renderPass.AttachmentInfo[i] == RenderPass.RenderPassAttachmentType.Depth)
                {
                    _attachmentImages[i] = new Image(
                        _device,
                        width, height,
                        VkFormat.D32Sfloat,
                        VkImageUsageFlags.DepthStencilAttachment | VkImageUsageFlags.TransferSrc | VkImageUsageFlags.TransferDst
                    );
                    _attachmentImageViews[i] = new ImageView(
                        _attachmentImages[i], 
                        VkImageAspectFlags.Depth
                    );
                }
                else
                    throw new NotSupportedException("Framebuffer only supports image and depth attachments");
            }

            var attachments = new NativeList<VkImageView>();
            foreach (var attachment in _attachmentImageViews)
                attachments.Add(attachment.Handle);

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