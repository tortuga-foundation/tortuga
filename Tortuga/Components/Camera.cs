using Tortuga.Graphics.API;
using System;
using System.Threading.Tasks;
using System.Numerics;
using System.Runtime.CompilerServices;
using Vulkan;

namespace Tortuga.Components
{
    /// <summary>
    /// camera component used to render the scene
    /// </summary>
    public class Camera : Core.BaseComponent
    {
        internal struct CameraShaderInfo
        {
            public Matrix4x4 View;
            public Matrix4x4 Projection;
            public int PositionX;
            public int PositionY;
            public int Width;
            public int Height;
        };

        /// <summary>
        /// Type of camera
        /// </summary>
        public enum ProjectionType
        {
            /// <summary>
            /// Perspective
            /// </summary>
            Perspective,
            /// <summary>
            /// Orthographic
            /// </summary>
            Orthographic
        }

        internal Framebuffer Framebuffer => _framebuffer;
        internal DescriptorSetPool.DescriptorSet CameraDescriptorSet => _cameraDescriptorSet;
        internal DescriptorSetPool.DescriptorSet UiDescriptorSet => _uiDescriptorSet;
        /// <summary>
        /// Viewport of the camera
        /// </summary>
        public Vector4 Viewport = new Vector4
        {
            X = 0,
            Y = 0,
            Z = 1,
            W = 1
        };
        /// <summary>
        /// Camera resolution, this get's set automatically using Settings.Graphics.RenderResolutionScale
        /// </summary>
        public Vector2 Resolution
        {
            get
            {
                return new Vector2
                {
                    X = Convert.ToInt32(_framebuffer.Width),
                    Y = Convert.ToInt32(_framebuffer.Height)
                };
            }
            set
            {
                _framebuffer = new Framebuffer(
                    Engine.Instance.MainRenderPass,
                    Convert.ToUInt32(value.X), Convert.ToUInt32(value.Y)
                );
            }
        }
        /// <summary>
        /// How cose can the camera see
        /// </summary>
        public float NearClipPlane = 0.01f;
        /// <summary>
        /// How far can the camera see
        /// </summary>
        public float FarClipPlane = 100.0f;
        /// <summary>
        /// Field of view for the camera
        /// </summary>
        public float FieldOfView = 90.0f;
        /// <summary>
        /// The type of camera
        /// </summary>
        public ProjectionType Projection = ProjectionType.Perspective;
        /// <summary>
        /// Is the camera static
        /// </summary>
        public bool IsStatic
        {
            get
            {
                var transform = MyEntity.GetComponent<Transform>();
                if (transform == null)
                    return false;

                return transform.IsStatic;
            }
        }

        private Framebuffer _framebuffer;
        private DescriptorSetPool _cameraDescriptorPool;
        private DescriptorSetPool.DescriptorSet _cameraDescriptorSet;
        private Tortuga.Graphics.API.Buffer _cameraBuffer;

        private DescriptorSetPool _uiDescriptorPool;
        private DescriptorSetPool.DescriptorSet _uiDescriptorSet;
        private Tortuga.Graphics.API.Buffer _uiBuffer;

        /// <summary>
        /// Initialize the camera
        /// </summary>
        public override async Task OnEnable()
        {
            await Task.Run(() =>
            {
                _framebuffer = new Framebuffer(
                    Engine.Instance.MainRenderPass,
                    1920, 1080
                );
                _cameraBuffer = Tortuga.Graphics.API.Buffer.CreateDevice(
                    Convert.ToUInt32(Unsafe.SizeOf<CameraShaderInfo>()),
                    VkBufferUsageFlags.UniformBuffer
                );
                _cameraDescriptorPool = new DescriptorSetPool(Engine.Instance.CameraDescriptorLayout);
                _cameraDescriptorSet = _cameraDescriptorPool.AllocateDescriptorSet();
                _cameraDescriptorSet.BuffersUpdate(new Tortuga.Graphics.API.Buffer[]{
                    _cameraBuffer
                });

                //user interface
                _uiBuffer = Tortuga.Graphics.API.Buffer.CreateDevice(
                    Convert.ToUInt32(Unsafe.SizeOf<Matrix4x4>()),
                    VkBufferUsageFlags.UniformBuffer
                );
                _uiDescriptorPool = new DescriptorSetPool(Engine.Instance.UiCameraDescriptorLayout);
                _uiDescriptorSet = _uiDescriptorPool.AllocateDescriptorSet();
                _uiDescriptorSet.BuffersUpdate(_uiBuffer);
            });
        }

        private float ToRadians(float degree)
        {
            return (degree / 360) * MathF.PI;
        }

        /// <summary>
        /// Get the projection matrix of the camera
        /// </summary>
        public Matrix4x4 ProjectionMatrix
        {
            get
            {
                if (this.Projection == ProjectionType.Perspective)
                    return Matrix4x4.CreatePerspectiveFieldOfView(ToRadians(FieldOfView), Resolution.X / Resolution.Y, NearClipPlane, FarClipPlane);
                else if (this.Projection == ProjectionType.Orthographic)
                    return Matrix4x4.CreateOrthographic(Resolution.X, Resolution.Y, NearClipPlane, FarClipPlane);

                throw new NotSupportedException("This type of projection is not supported by the camera");
            }
        }
        /// <summary>
        /// get the view matrix of the camera
        /// </summary>
        public Matrix4x4 ViewMatrix
        {
            get
            {
                var viewMatrix = Matrix4x4.Identity;

                var transform = MyEntity.GetComponent<Transform>();
                if (transform != null)
                    viewMatrix = transform.Matrix;

                viewMatrix.M22 *= -1;
                return viewMatrix;
            }
        }

        /// <summary>
        /// update camera graphics buffers
        /// </summary>
        public async Task UpdateCameraBuffers()
        {
            var windowSize = Engine.Instance.MainWindow.Size;
            await _cameraBuffer.SetDataWithStaging<CameraShaderInfo>(
                new CameraShaderInfo[]
                {
                    new CameraShaderInfo
                    {
                        Projection = ProjectionMatrix,
                        View = ViewMatrix,
                        PositionX = Convert.ToInt32(Math.Round(windowSize.X * Viewport.X)),
                        PositionY = Convert.ToInt32(Math.Round(windowSize.Y * Viewport.Y)),
                        Width = Convert.ToInt32(MathF.Round(Resolution.X)),
                        Height = Convert.ToInt32(MathF.Round(Resolution.Y))
                    }
                }
            );
            await _uiBuffer.SetDataWithStaging<Matrix4x4>(
                new Matrix4x4[]{
                    Matrix4x4.CreateOrthographicOffCenter(
                        0.0f,
                        Resolution.X,
                        0.0f,
                        Resolution.Y,
                        -1.0f,
                        1.0f
                    )
                }
            );
        }
        internal BufferTransferObject[] UpdateCameraBuffersSemaphore()
        {
            var windowSize = Engine.Instance.MainWindow.Size;
            return new BufferTransferObject[]{
                _cameraBuffer.SetDataGetTransferObject<CameraShaderInfo>(
                    new CameraShaderInfo[]
                    {
                        new CameraShaderInfo
                        {
                            Projection = ProjectionMatrix,
                            View = ViewMatrix,
                            PositionX = Convert.ToInt32(Math.Round(windowSize.X * Viewport.X)),
                            PositionY = Convert.ToInt32(Math.Round(windowSize.Y * Viewport.Y)),
                            Width = Convert.ToInt32(MathF.Round(Resolution.X)),
                            Height = Convert.ToInt32(MathF.Round(Resolution.Y))
                        }
                    }
                ),
                _uiBuffer.SetDataGetTransferObject<Matrix4x4>(
                    new Matrix4x4[]{
                        Matrix4x4.CreateOrthographicOffCenter(
                            0.0f,
                            Resolution.X,
                            0.0f,
                            Resolution.Y,
                            -1.0f,
                            1.0f
                        )
                    }
                )
            };
        }
    }
}