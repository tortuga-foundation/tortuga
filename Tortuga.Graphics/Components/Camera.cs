using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Tortuga.Graphics.API;
using Tortuga.Core;
using static Tortuga.Graphics.Light;
using System.Linq;

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
        /// What resolution the camera renders at
        /// </summary>
        public Vector2 Resolution
        {
            get => _resolution;
            set
            {
                uint width = Convert.ToUInt32(value.X);
                uint height = Convert.ToUInt32(value.Y);
                _mrtFramebuffer = new Framebuffer(
                    _graphicsModule.RenderPasses["_MRT"],
                    width, height
                );
                _defferedFramebuffer = new Framebuffer(
                    _graphicsModule.RenderPasses["_DEFFERED"],
                    width, height
                );
                _resolution = value;
            }
        }
        /// <summary>
        /// Field of view for the camera
        /// </summary>
        public float FieldOfView = 70.0f;
        /// <summary>
        /// Viewport, where in the render target this camera should be rendered
        /// 0,0,1,1 = full screen
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

        /// <summary>
        /// Where the camera rendered output should be stored
        /// </summary>
        public RenderTarget RenderTarget = null;

        internal DescriptorService DescriptorService => _descriptorService;
        internal Framebuffer MrtFramebuffer => _mrtFramebuffer;
        internal Framebuffer DefferedFramebuffer => _defferedFramebuffer;
        internal static Pipeline DefferedPipeline => _defferedPipeline;

        private ProjectionType _projectionType = ProjectionType.Perspective;
        private Vector2 _resolution;
        private DescriptorService _descriptorService;
        private Framebuffer _mrtFramebuffer;
        private Framebuffer _defferedFramebuffer;
        private static Pipeline _defferedPipeline;
        private GraphicsModule _graphicsModule;

        /// <summary>
        /// Runs when component is enabled in the scene
        /// </summary>
        public override Task OnEnable()
        => Task.Run(() =>
        {
            _graphicsModule = Engine.Instance.GetModule<GraphicsModule>();
            _descriptorService = new DescriptorService();
            _resolution = new Vector2(1920, 1080);

            //PROJECTION
            var PROJECTION_KEY = "_PROJECTION";
            _descriptorService.InsertKey(PROJECTION_KEY, _graphicsModule.DescriptorLayouts[PROJECTION_KEY]);
            _descriptorService.BindBuffer(PROJECTION_KEY, 0, ProjectionMatrix.GetBytes());

            //VIEW
            var VIEW_KEY = "_VIEW";
            _descriptorService.InsertKey(VIEW_KEY, _graphicsModule.DescriptorLayouts[VIEW_KEY]);
            _descriptorService.BindBuffer(VIEW_KEY, 0, ViewMatrix.GetBytes());

            //MRT
            var MRT_KEY = "_MRT";
            _mrtFramebuffer = new Framebuffer(
                _graphicsModule.RenderPasses[MRT_KEY],
                Convert.ToUInt32(_resolution.X),
                Convert.ToUInt32(_resolution.Y)
            );
            //make sure frame buffer is created with the correct MRT details
            _descriptorService.InsertKey(MRT_KEY, _graphicsModule.DescriptorLayouts[MRT_KEY]);
            int mrtBindingCount = 0;
            foreach (var attachment in _mrtFramebuffer.RenderPass.Attachments)
            {
                if (attachment.Format != API.RenderPassAttachment.Default.Format) continue;

                _descriptorService.BindImage(
                    MRT_KEY,
                    mrtBindingCount,
                    _mrtFramebuffer.Images[mrtBindingCount],
                    _mrtFramebuffer.ImageViews[mrtBindingCount]
                );
                mrtBindingCount++;
            }

            //camera position descriptor set
            var CAMERA_KEY = "_CAMERA";
            _descriptorService.InsertKey(CAMERA_KEY, _graphicsModule.DescriptorLayouts[CAMERA_KEY]);
            _descriptorService.BindBuffer(CAMERA_KEY, 0, Position.GetBytes());

            //light
            var LIGHT_KEY = "_LIGHT";
            _descriptorService.InsertKey(LIGHT_KEY, _graphicsModule.DescriptorLayouts[LIGHT_KEY]);

            //deffered pipeline
            var DEFFERED_KEY = "_DEFFERED";
            _defferedFramebuffer = new Framebuffer(
                _graphicsModule.RenderPasses[DEFFERED_KEY],
                Convert.ToUInt32(_resolution.X),
                Convert.ToUInt32(_resolution.Y)
            );
            if (_defferedPipeline == null)
            {
                _defferedPipeline = new GraphicsPipeline(
                    _graphicsModule.GraphicsService.PrimaryDevice,
                    _graphicsModule.RenderPasses[DEFFERED_KEY],
                    new List<DescriptorLayout>
                    {
                        _graphicsModule.DescriptorLayouts[MRT_KEY],
                        _graphicsModule.DescriptorLayouts[CAMERA_KEY],
                        _graphicsModule.DescriptorLayouts[LIGHT_KEY]
                    },
                    new List<ShaderModule>()
                    {
                        new ShaderModule(
                            _graphicsModule.GraphicsService.PrimaryDevice,
                            "Assets/Shaders/Default/Deffered.vert"
                        ),
                        new ShaderModule(
                            _graphicsModule.GraphicsService.PrimaryDevice,
                            "Assets/Shaders/Default/Deffered.frag"
                        )
                    },
                    new PipelineInputBuilder()
                );
            }
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
                        FieldOfView.ToRadians(),
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

        /// <summary>
        /// Descriptor set for projection
        /// </summary>
        public DescriptorSet ProjectionDescriptorSet
        => _descriptorService.Handle["_PROJECTION"].Set;

        /// <summary>
        /// Descriptor set for view
        /// </summary>
        public DescriptorSet ViewDescriptorSet
        => _descriptorService.Handle["_VIEW"].Set;

        /// <summary>
        /// Mrt descriptor sets
        /// </summary>
        public List<DescriptorSet> MrtDescriptorSets
        => new List<DescriptorSet>
        {
            _descriptorService.Handle["_MRT"].Set,
            _descriptorService.Handle["_CAMERA"].Set,
            _descriptorService.Handle["_LIGHT"].Set
        };

        /// <summary>
        /// updates camera's descriptor set objects
        /// </summary>
        public void UpdateDescriptorSets()
        {
            if (this.IsStatic)
                return;

            _descriptorService.BindBuffer("_VIEW", 0, ViewMatrix.GetBytes());
        }

        /// <summary>
        /// updates the light information
        /// </summary>
        /// <param name="lights">list of lights to use for rendering</param>
        public void UpdateLights(Light[] lights)
        {
            _descriptorService.BindBuffer(
                "_LIGHT",
                0,
                lights.Select(light => light.ToShaderInfo).ToArray()
            );
        }
    }
}