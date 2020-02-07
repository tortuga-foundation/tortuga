using System;
using Vulkan;
using static Vulkan.VulkanNative;

namespace Tortuga.Graphics.API
{
    internal class Framebuffer
    {
        public VkFramebuffer Handle => _frameBuffer;
        public Image ColorImage => _colorImage;
        public Image DepthImage => _depthImage;
        public ImageView ColorImageView => _colorImageView;
        public ImageView DepthImageView => _depthImageView;
        public uint Width => _width;
        public uint Height => _height;

        private Image _colorImage;
        private ImageView _colorImageView;
        private Image _depthImage;
        private ImageView _depthImageView;
        private VkFramebuffer _frameBuffer;
        private uint _width, _height;

        public unsafe Framebuffer(RenderPass renderPass, uint width, uint height)
        {
            _width = width;
            _height = height;
            _colorImage = new Image(
                width, height,
                VkFormat.R8g8b8a8Unorm,
                VkImageUsageFlags.ColorAttachment | VkImageUsageFlags.TransferSrc | VkImageUsageFlags.TransferDst
            );
            _colorImageView = new ImageView(_colorImage, VkImageAspectFlags.Color);
            _depthImage = new Image(
                width, height,
                VkFormat.D32Sfloat,
                VkImageUsageFlags.DepthStencilAttachment | VkImageUsageFlags.TransferSrc | VkImageUsageFlags.TransferDst
            );
            _depthImageView = new ImageView(_depthImage, VkImageAspectFlags.Depth);

            var attachments = new NativeList<VkImageView>();
            attachments.Add(_colorImageView.Handle);
            attachments.Add(_depthImageView.Handle);

            var framebufferCreateInfo = VkFramebufferCreateInfo.New();
            framebufferCreateInfo.renderPass = renderPass.Handle;
            framebufferCreateInfo.attachmentCount = attachments.Count;
            framebufferCreateInfo.pAttachments = (VkImageView*)attachments.Data.ToPointer();
            framebufferCreateInfo.width = width;
            framebufferCreateInfo.height = height;
            framebufferCreateInfo.layers = 1;

            VkFramebuffer frameBuffer;
            if (vkCreateFramebuffer(
                Engine.Instance.MainDevice.LogicalDevice,
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
                Engine.Instance.MainDevice.LogicalDevice,
                _frameBuffer,
                null
            );
        }
    }
}