#pragma warning disable 1591
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace Tortuga.Graphics
{
    public class Camera : Core.BaseComponent
    {
        public enum TypeOfRenderTarget
        {
            DeferredRendering = -1,
            Color = 0,
            Normal = 1,
            Position = 2,
            Detail = 3
        }

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

        public TypeOfRenderTarget RenderTarget = TypeOfRenderTarget.DeferredRendering;

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
                    Engine.Instance.GetModule<GraphicsModule>().MeshRenderPassMRT,
                    Convert.ToUInt32(MathF.Round(value.X)),
                    Convert.ToUInt32(MathF.Round(value.Y))
                );
                _defferedFramebuffer = new API.Framebuffer(
                    Engine.Instance.GetModule<GraphicsModule>().DefferedRenderPass,
                    Convert.ToUInt32(MathF.Round(value.X)),
                    Convert.ToUInt32(MathF.Round(value.Y))
                );
                if (_descriptorHelper == null)
                    _descriptorHelper = new DescriptorSetHelper();
                _descriptorHelper.InsertKey(UI_PROJECTION_KEY, Tortuga.UI.UiResources.Instance.DescriptorSetLayouts[0]);
                _descriptorHelper.BindBuffer(UI_PROJECTION_KEY, 0, new Matrix4x4[]
                {
                    Matrix4x4.CreateOrthographicOffCenter(0, Resolution.X, Resolution.Y, 0, 0, 1)
                }).Wait();
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

        #region Framebuffers

        internal API.Framebuffer Framebuffer => _framebuffer;
        private API.Framebuffer _framebuffer;
        internal API.Framebuffer DefferedFramebuffer => _defferedFramebuffer;
        private API.Framebuffer _defferedFramebuffer;

        #endregion

        #region descriptor sets

        private DescriptorSetHelper _descriptorHelper;
        private const string PROJECTION_KEY = "PROJECTION";
        private const string VIEW_KEY = "VIEW";
        private const string MRT_KEY = "MRT";
        private const string CAMERA_POSITION_KEY = "CAMERA_POSITION";
        private const string LIGHT_KEY = "LIGHT";
        private const string UI_PROJECTION_KEY = "UI_PROJECTION";
        internal API.DescriptorSetPool.DescriptorSet ProjectionDescriptor => _descriptorHelper.DescriptorObjectMapper[PROJECTION_KEY].Set;
        internal API.DescriptorSetPool.DescriptorSet ViewDescriptor => _descriptorHelper.DescriptorObjectMapper[VIEW_KEY].Set;
        internal API.DescriptorSetPool.DescriptorSet MrtDescriptorSet => _descriptorHelper.DescriptorObjectMapper[MRT_KEY].Set;
        internal API.DescriptorSetPool.DescriptorSet CameraPositionDescriptorSet => _descriptorHelper.DescriptorObjectMapper[CAMERA_POSITION_KEY].Set;
        internal API.DescriptorSetPool.DescriptorSet LightDescriptorSet => _descriptorHelper.DescriptorObjectMapper[LIGHT_KEY].Set;
        internal API.DescriptorSetPool.DescriptorSet UiProjecionSet => _descriptorHelper.DescriptorObjectMapper[UI_PROJECTION_KEY].Set;

        #endregion

        #region deffered pipeline

        internal API.Pipeline DefferedPipeline => _defferedPipeline;
        private API.Pipeline _defferedPipeline;

        private API.Shader _vertexShader;
        private API.Shader _fragmentShader;

        private int _lightsAmount = 0;

        #endregion

        public override Task OnEnable()
        {
            return Task.Run(() =>
            {
                Resolution = new Vector2(1920, 1080);
                var module = Engine.Instance.GetModule<GraphicsModule>();
                //setup descriptor sets
                if (_descriptorHelper == null)
                    _descriptorHelper = new DescriptorSetHelper();
                _descriptorHelper.InsertKey(PROJECTION_KEY, module.MeshDescriptorSetLayouts[0]);
                _descriptorHelper.InsertKey(VIEW_KEY, module.MeshDescriptorSetLayouts[1]);
                _descriptorHelper.BindBuffer(PROJECTION_KEY, 0, DescriptorSetHelper.MatrixToBytes(ProjectionMatrix)).Wait();
                _descriptorHelper.BindBuffer(VIEW_KEY, 0, DescriptorSetHelper.MatrixToBytes(ViewMatrix)).Wait();

                //mrt descriptor set
                _descriptorHelper.InsertKey(MRT_KEY, module.DefferedDescriptorSetLayouts[0]);
                _descriptorHelper.BindImage(MRT_KEY, 0, _framebuffer.AttachmentImages[0], _framebuffer.AttachmentViews[0]);
                _descriptorHelper.BindImage(MRT_KEY, 1, _framebuffer.AttachmentImages[1], _framebuffer.AttachmentViews[1]);
                _descriptorHelper.BindImage(MRT_KEY, 2, _framebuffer.AttachmentImages[2], _framebuffer.AttachmentViews[2]);
                _descriptorHelper.BindImage(MRT_KEY, 3, _framebuffer.AttachmentImages[3], _framebuffer.AttachmentViews[3]);

                //camera position descriptor set
                _descriptorHelper.InsertKey(CAMERA_POSITION_KEY, module.DefferedDescriptorSetLayouts[1]);
                var bytes = new List<byte>();
                {
                    var pos = Vector4.Zero;
                    var transform = MyEntity.GetComponent<Core.Transform>();
                    if (transform != null)
                        pos = new Vector4(transform.Position, 1.0f);

                    foreach (var b in BitConverter.GetBytes(pos.X))
                        bytes.Add(b);
                    foreach (var b in BitConverter.GetBytes(pos.Y))
                        bytes.Add(b);
                    foreach (var b in BitConverter.GetBytes(pos.Z))
                        bytes.Add(b);
                    foreach (var b in BitConverter.GetBytes(pos.W))
                        bytes.Add(b);
                }
                _descriptorHelper.BindBuffer(CAMERA_POSITION_KEY, 0, bytes.ToArray()).Wait();

                //light descriptor set
                _descriptorHelper.InsertKey(LIGHT_KEY, module.DefferedDescriptorSetLayouts[2]);
                _descriptorHelper.BindBuffer(LIGHT_KEY, 0, new byte[] { 1 }).Wait();

                //ui projection descriptor set
                _descriptorHelper.InsertKey(UI_PROJECTION_KEY, Tortuga.UI.UiResources.Instance.DescriptorSetLayouts[0]);
                _descriptorHelper.BindBuffer(UI_PROJECTION_KEY, 0, new Matrix4x4[]
                {
                    Matrix4x4.CreateOrthographicOffCenter(0, Resolution.X, Resolution.Y, 0, 0, 1)
                }).Wait();

                //deffered pipeline
                _vertexShader = new API.Shader(
                    API.Handler.MainDevice,
                    "Assets/Shaders/Default/Deffered.vert"
                );
                _fragmentShader = new API.Shader(
                    API.Handler.MainDevice,
                    "Assets/Shaders/Default/Deffered.frag"
                );
                _fragmentShader.CreateOrUpdateSpecialization(0, 0);
                _defferedPipeline = new API.Pipeline(
                    module.DefferedRenderPass,
                    module.DefferedDescriptorSetLayouts,
                    _vertexShader, _fragmentShader,
                    new PipelineInputBuilder()
                );
            });
        }

        internal API.BufferTransferObject[] UpdateLightInfo(Light.LightInfo[] lights)
        {
            if (lights.Length != _lightsAmount)
            {
                _fragmentShader.CreateOrUpdateSpecialization(0, lights.Length);
                var module = Engine.Instance.GetModule<GraphicsModule>();
                _defferedPipeline = new API.Pipeline(
                    module.DefferedRenderPass,
                    module.DefferedDescriptorSetLayouts,
                    _vertexShader, _fragmentShader,
                    new PipelineInputBuilder()
                );
                _lightsAmount = lights.Length;
            }
            var transferObject = new API.BufferTransferObject();
            if (lights.Length > 0)
                transferObject = _descriptorHelper.BindBufferWithTransferObject(LIGHT_KEY, 0, lights);
            else
                transferObject = _descriptorHelper.BindBufferWithTransferObject(LIGHT_KEY, 0, new byte[] { 1 });
            return new API.BufferTransferObject[] { transferObject };
        }

        private float ToRadians(float degree)
        {
            return (degree / 360) * MathF.PI;
        }

        internal API.BufferTransferObject[] UpdateView()
        {
            if (this.IsStatic)
                return new API.BufferTransferObject[] { };

            return new API.BufferTransferObject[]
            {
                _descriptorHelper.BindBufferWithTransferObject(VIEW_KEY, 0, DescriptorSetHelper.MatrixToBytes(ViewMatrix))
            };
        }
    }
}