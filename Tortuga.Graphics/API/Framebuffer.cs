using System;
using Vulkan;
using Tortuga.Utils;
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
        public Device DeviceUsed => _device;

        private Image _colorImage;
        private ImageView _colorImageView;
        private Image _depthImage;
        private ImageView _depthImageView;
        private VkFramebuffer _frameBuffer;
        private uint _width, _height;
        private Device _device;

        public unsafe Framebuffer(Device device, RenderPass renderPass, uint width, uint height)
        {
            _device = device;
            _width = width;
            _height = height;
            _colorImage = new Image(
                _device,
                width, height,
                VkFormat.R32g32b32a32Sfloat,
                VkImageUsageFlags.ColorAttachment | VkImageUsageFlags.TransferSrc | VkImageUsageFlags.TransferDst
            );
            _colorImageView = new ImageView(_colorImage, VkImageAspectFlags.Color);
            _depthImage = new Image(
                _device,
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