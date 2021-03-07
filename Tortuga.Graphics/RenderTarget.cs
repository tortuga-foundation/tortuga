using System;
using Tortuga.Graphics.API;
using Vulkan;

namespace Tortuga.Graphics
{
    /// <summary>
    /// Can be used to render a camera's output to an object
    /// </summary>
    public class RenderTarget
    {
        /// <summary>
        /// Image
        /// </summary>
        public Image RenderedImage => _renderedImage;
        /// <summary>
        /// Image View
        /// </summary>
        public ImageView RenderedImageView => _renderedImageView;
        /// <summary>
        /// Rendered image from the camera
        /// </summary>
        protected Image _renderedImage;
        /// <summary>
        /// Image view for the rendered image
        /// </summary>
        protected ImageView _renderedImageView;

        /// <summary>
        /// Constructor for RenderTarget
        /// </summary>
        public RenderTarget(
            Device device,
            uint width, uint height
        )
        {
            _renderedImage = new Image(
                device,
                width, height,
                API.RenderPassAttachment.Default.Format,
                VkImageUsageFlags.ColorAttachment |
                VkImageUsageFlags.TransferDst |
                VkImageUsageFlags.TransferSrc
            );
            _renderedImageView = new ImageView(
                _renderedImage,
                VkImageAspectFlags.Color
            );
        }

        /// <summary>
        /// Convert's Render Target to an image
        /// </summary>
        public void ConvertToImage()
        {
            //TODO
            throw new NotImplementedException();
        }

        /// <summary>
        /// Convert's render target to texture
        /// </summary>
        public void ConverToTexture()
        {
            //TODO
            throw new NotImplementedException();
        }
    }
}