#pragma warning disable 1591
using System;
using System.Numerics;
using System.Threading.Tasks;

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
            get => _resolution;
            set
            {
                _descriptorHelper.BindImage(
                    "RenderedImage",
                    0,
                    null,
                    Convert.ToInt32(value.X),
                    Convert.ToInt32(value.Y)
                ).Wait();
                _resolution = value;
            }
        }
        private Vector2 _resolution;

        /// <summary>
        /// How far can the camera see
        /// </summary>
        public float MaxCameraDistance = 100.0f;

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
        private const string RENDER_IMAGE_KEY = "RenderedImage";
        private DescriptorSetHelper _descriptorHelper;
        internal DescriptorSetHelper.DescriptorObject RenderImageDescriptorMap => _descriptorHelper.DescriptorObjectMapper[RENDER_IMAGE_KEY];

        public override Task OnEnable()
        {
            return Task.Run(() =>
            {
                _descriptorHelper = new DescriptorSetHelper();
                _descriptorHelper.InsertKey(RENDER_IMAGE_KEY, Engine.Instance.GetModule<GraphicsModule>().RenderDescriptorLayouts[0]);
                Resolution = new Vector2(1920, 1080);
            });
        }
    }
}