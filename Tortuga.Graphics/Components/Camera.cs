#pragma warning disable 1591
using System;
using System.Numerics;
using System.Threading.Tasks;
using Vulkan;

namespace Tortuga.Graphics
{
    public class Camera : Core.BaseComponent
    {
        /// <summary>
        /// type of camera projection
        /// </summary>
        public enum ProjectionType
        {
            /// <summary>
            /// perspective projection type
            /// </summary>
            Perspective,
            /// <summary>
            /// orthographic projection type
            /// </summary>
            Orthographic
        }
    
        /// <summary>
        /// Camera viewport
        /// </summary>
        public Vector4 Viewport = new Vector4
        {
            X = 0,
            Y = 0,
            Z = 1,
            W = 1
        };

        /// <summary>
        /// resolution of the camera
        /// </summary>
        public Vector2 Resolution
        {
            get => new Vector2(RenderedImage.Width, RenderedImage.Height);
            set => RenderedImage = new API.Image(
                API.Handler.MainDevice,
                Convert.ToUInt32(value.X),
                Convert.ToUInt32(value.Y),
                VkFormat.R8g8b8a8Unorm,
                (
                    VkImageUsageFlags.ColorAttachment | 
                    VkImageUsageFlags.TransferDst | 
                    VkImageUsageFlags.TransferSrc
                )
            );
        }

        /// <summary>
        /// How far can the camera see
        /// </summary>
        public float MaxCameraDistance = 100.0f;

        internal API.Image RenderedImage;
    
        public Matrix4x4 ViewMatrix
        {
            get
            {
                var viewMatrix = Matrix4x4.Identity;
                var transform = MyEntity.GetComponent<Core.Transform>();
                if (transform != null)
                    viewMatrix = transform.Matrix;

                Matrix4x4.Invert(viewMatrix, out Matrix4x4 invertedMatrix);
                return invertedMatrix;
            }
        }

        /// <summary>
        /// will render the camera into a window
        /// </summary>
        public Window RenderToWindow;
    
        public override Task OnEnable()
        {
            return Task.Run(() => 
            {
                RenderedImage = new API.Image(
                    API.Handler.MainDevice,
                    1920,
                    1080,
                    VkFormat.R8g8b8a8Unorm,
                    (
                        VkImageUsageFlags.TransferDst |
                        VkImageUsageFlags.TransferSrc
                    )
                );
            });
        }
    }
}