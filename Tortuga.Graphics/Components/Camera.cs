#pragma warning disable 1591
using System;
using System.Collections.Generic;
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
        
        public ProjectionType Type
        {
            get => _type;
            set
            {
                _type = value;
                _descriptorHelper.BindBuffer(PROJECTION_KEY, 0, DescriptorSetHelper.MatrixToBytes(ProjectionMatrix)).Wait();
            }
        }
        private ProjectionType _type = ProjectionType.Perspective;

        /// <summary>
        /// camera field of view
        /// </summary>
        public float FieldOfView = 90.0f;

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
                _framebuffer = new API.Framebuffer(
                    Engine.Instance.GetModule<GraphicsModule>().RenderPass,
                    Convert.ToUInt32(MathF.Round(value.X)),
                    Convert.ToUInt32(MathF.Round(value.Y))
                );
                _resolution = value;
            }
        }
        private Vector2 _resolution;

        /// <summary>
        /// How close can the camera see
        /// </summary>
        public float NearClipPlane = 0.01f;
        /// <summary>
        /// How far can the camera see
        /// </summary>
        public float FarClipPlane = 100.0f;

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

        public bool IsStatic
        {
            get
            {
                bool isStatic = false;
                var transform = MyEntity.GetComponent<Core.Transform>();
                if (transform != null)
                    isStatic = transform.IsStatic;

                return isStatic;
            }
        }

        private float ToRadians(float degree)
        {
            return (degree / 360) * MathF.PI;
        }

        /// <summary>
        /// projection matrix
        /// </summary>
        public Matrix4x4 ProjectionMatrix
        {
            get
            {
                if (this.Type == ProjectionType.Perspective)
                    return Matrix4x4.CreatePerspectiveFieldOfView(ToRadians(FieldOfView), Resolution.X / Resolution.Y, NearClipPlane, FarClipPlane);
                else if (this.Type == ProjectionType.Orthographic)
                    return Matrix4x4.CreateOrthographic(Resolution.X, Resolution.Y, NearClipPlane, FarClipPlane);

                throw new NotSupportedException("This type of projection is not supported by the camera");
            }
        }

        /// <summary>
        /// will render the camera into a window
        /// </summary>
        public Window RenderToWindow;
        internal API.Framebuffer Framebuffer => _framebuffer;
        private API.Framebuffer _framebuffer;
        private DescriptorSetHelper _descriptorHelper;
        private const string PROJECTION_KEY = "PROJECTION";
        private const string VIEW_KEY = "VIEW";
        internal API.DescriptorSetPool.DescriptorSet ProjectionDescriptor => _descriptorHelper.DescriptorObjectMapper[PROJECTION_KEY].Set;
        internal API.DescriptorSetPool.DescriptorSet ViewDescriptor => _descriptorHelper.DescriptorObjectMapper[VIEW_KEY].Set;
        internal API.BufferTransferObject[] UpdateView()
        {
            if (this.IsStatic)
                return new API.BufferTransferObject[]{};

            return new API.BufferTransferObject[]
            {
                _descriptorHelper.BindBufferWithTransferObject(VIEW_KEY, 0, DescriptorSetHelper.MatrixToBytes(ViewMatrix))
            };
        }

        public override Task OnEnable()
        {
            return Task.Run(() =>
            {
                Resolution = new Vector2(1920, 1080);
                var module = Engine.Instance.GetModule<GraphicsModule>();
                _descriptorHelper = new DescriptorSetHelper();
                _descriptorHelper.InsertKey(PROJECTION_KEY, module.RenderDescriptorLayouts[0]);
                _descriptorHelper.InsertKey(VIEW_KEY, module.RenderDescriptorLayouts[1]);
                _descriptorHelper.BindBuffer(PROJECTION_KEY, 0, DescriptorSetHelper.MatrixToBytes(ProjectionMatrix)).Wait();
                _descriptorHelper.BindBuffer(VIEW_KEY, 0, DescriptorSetHelper.MatrixToBytes(ViewMatrix)).Wait();
            });
        }
    }
}