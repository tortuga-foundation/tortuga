using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Tortuga.Graphics.API;

namespace Tortuga.Graphics
{
    /// <summary>
    /// Type of project to use for the camera
    /// </summary>
    public enum ProjectionType
    {
        /// <summary>
        /// Perspective view, mainly used for 3D
        /// </summary>
        Perspective,
        /// <summary>
        /// Orthographic view, mainly used for 2D
        /// </summary>
        Orthographic
    }

    /// <summary>
    /// Camera used to render a scenene
    /// </summary>
    public class Camera : Core.BaseComponent
    {
        /// <summary>
        /// Field of view for the camera
        /// </summary>
        public float FieldOfView = 70.0f;
        /// <summary>
        /// Viewport, where in the window this camera should be rendered
        /// </summary>
        public Vector4 Viewport = new Vector4(0, 0, 1, 1);
        /// <summary>
        /// How far an object the camera can see
        /// </summary>
        public float FarClipPlane = 100.0f;
        /// <summary>
        /// How close an object the camera can see
        /// </summary>
        public float NearClipPlane = 0.01f;
        private ProjectionType _projectionType = ProjectionType.Perspective;
        private Vector2 _resolution;
        private DescriptorService _descriptorService;
        private Framebuffer _framebuffer;
        private Pipeline _defferedPipeline;

        /// <summary>
        /// Runs when component is enabled in the scene
        /// </summary>
        public override Task OnEnable()
        => Task.Run(() =>
        {
            var module = Engine.Instance.GetModule<GraphicsModule>();
            _descriptorService = new DescriptorService();
            _resolution = new Vector2(1920, 1080);

            //PROJECTION
            var PROJECTION_KEY = "_PROJECTION";
            _descriptorService.InsertKey(PROJECTION_KEY, module.DescriptorLayouts[PROJECTION_KEY]);
            _descriptorService.BindBuffer(PROJECTION_KEY, 0, ProjectionMatrix.GetBytes());

            //VIEW
            var VIEW_KEY = "_VIEW";
            _descriptorService.InsertKey(VIEW_KEY, module.DescriptorLayouts[VIEW_KEY]);
            _descriptorService.BindBuffer(VIEW_KEY, 0, ViewMatrix.GetBytes());

            //MRT
            var MRT_KEY = "_MRT";
            //make sure frame buffer is created with the correct MRT details
            _framebuffer = new Framebuffer(
                module.RenderPasses[MRT_KEY],
                Convert.ToUInt32(_resolution.X),
                Convert.ToUInt32(_resolution.Y)
            );
            _descriptorService.InsertKey(MRT_KEY, module.DescriptorLayouts[MRT_KEY]);
            _descriptorService.BindImage(MRT_KEY, 0, _framebuffer.Images[0], _framebuffer.ImageViews[0]);
            _descriptorService.BindImage(MRT_KEY, 1, _framebuffer.Images[1], _framebuffer.ImageViews[1]);
            _descriptorService.BindImage(MRT_KEY, 2, _framebuffer.Images[2], _framebuffer.ImageViews[2]);
            _descriptorService.BindImage(MRT_KEY, 3, _framebuffer.Images[3], _framebuffer.ImageViews[3]);

            //camera position descriptor set
            var CAMERA_KEY = "_CAMERA";
            _descriptorService.InsertKey(CAMERA_KEY, module.DescriptorLayouts[CAMERA_KEY]);
            _descriptorService.BindBuffer(CAMERA_KEY, 0, Position.GetBytes());

            //light
            var LIGHT_KEY = "_LIGHT";
            _descriptorService.InsertKey(LIGHT_KEY, module.DescriptorLayouts[LIGHT_KEY]);
            _descriptorService.BindBuffer(LIGHT_KEY, 0, new byte[] { 1 });

            //deffered pipeline
            _defferedPipeline = new GraphicsPipeline(
                module.GraphicsService.PrimaryDevice,
                module.RenderPasses[MRT_KEY],
                new List<DescriptorLayout>
                {
                    module.DescriptorLayouts[MRT_KEY],
                    module.DescriptorLayouts[CAMERA_KEY],
                    module.DescriptorLayouts[LIGHT_KEY]
                },
                new ShaderModule(
                    module.GraphicsService.PrimaryDevice,
                    "Assets/Shaders/Default/Deffered.vert"
                ),
                new ShaderModule(
                    module.GraphicsService.PrimaryDevice,
                    "Assets/Shaders/Default/Deffered.frag"
                ),
                new PipelineInputBuilder()
            );
        });

        /// <summary>
        /// Weather the camera is static or now, controlled by Transform component
        /// </summary>
        public bool IsStatic
        {
            get
            {
                var transform = MyEntity.GetComponent<Core.Transform>();
                if (transform == null)
                    return false;
                return transform.IsStatic;
            }
        }

        /// <summary>
        /// Creates a projection matrix for the camera
        /// </summary>
        public Matrix4x4 ProjectionMatrix
        {
            get
            {
                if (_projectionType == ProjectionType.Perspective)
                {
                    return Matrix4x4.CreatePerspectiveFieldOfView(
                        ToRadians(FieldOfView),
                        _resolution.X / _resolution.Y,
                        NearClipPlane,
                        FarClipPlane
                    );
                }
                else if (_projectionType == ProjectionType.Orthographic)
                {
                    return Matrix4x4.CreateOrthographic(
                        _resolution.X,
                        _resolution.Y,
                        NearClipPlane,
                        FarClipPlane
                    );
                }
                else
                    throw new NotSupportedException("This type of projection is not supported by the camera");
            }
        }

        /// <summary>
        /// Create view matrix for this camera
        /// </summary>
        public Matrix4x4 ViewMatrix
        {
            get
            {
                var transform = MyEntity.GetComponent<Core.Transform>();
                if (transform == null)
                    return Matrix4x4.Identity;

                Matrix4x4.Invert(transform.Matrix, out Matrix4x4 invertedMatrix);
                return invertedMatrix;
            }
        }

        /// <summary>
        /// Position of this camera in 3D space. Managed by Transform component
        /// </summary>
        public Vector4 Position
        {
            get
            {
                var transform = MyEntity.GetComponent<Core.Transform>();
                if (transform == null)
                    return Vector4.Zero;
                return new Vector4(transform.Position, 1.0f);
            }
        }

        private float ToRadians(float degree)
        {
            return (degree / 360) * MathF.PI;
        }
    }
}