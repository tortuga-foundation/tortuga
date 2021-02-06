using System;
using Vulkan;

namespace Tortuga.Graphics
{
    /// <summary>
    /// Can be used to render a camera's output to an object
    /// </summary>
    public class RenderTarget
    {
        /// <summary>
        /// Rendered image from the camera
        /// </summary>
        VkImage RenderedImage { get; set; }
        /// <summary>
        /// Image view for the rendered image
        /// </summary>
        VkImageView RenderedImageView { get; set; }

        /// <summary>
        /// Constructor for RenderTarget
        /// </summary>
        public RenderTarget()
        {
            RenderedImage = VkImage.Null;
            RenderedImageView = VkImageView.Null;
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