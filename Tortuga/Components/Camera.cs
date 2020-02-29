using Tortuga.Graphics.API;
using System;
using System.Threading.Tasks;
using System.Numerics;
using System.Runtime.CompilerServices;
using Vulkan;

namespace Tortuga.Components
{
    public class Camera : Core.BaseComponent
    {
        internal struct CameraShaderInfo
        {
            public Matrix4x4 View;
            public Matrix4x4 Projection;
        };

        public enum ProjectionType
        {
            Perspective,
            Orthographic
        }

        internal Framebuffer Framebuffer => _framebuffer;
        internal DescriptorSetPool.DescriptorSet CameraDescriptorSet => _cameraDescriptorSet;
        public Tortuga.Rect Viewport = new Rect
        {
            x = 0,
            y = 0,
            width = 1,
            height = 1
        };
        public Tortuga.IntVector2D Resolution
        {
            get
            {
                return new IntVector2D
                {
                    x = Convert.ToInt32(_framebuffer.Width),
                    y = Convert.ToInt32(_framebuffer.Height)
                };
            }
            set
            {
                _framebuffer = new Framebuffer(
                    Engine.Instance.MainRenderPass,
                    Convert.ToUInt32(value.x), Convert.ToUInt32(value.y)
                );
            }
        }
        public float NearClipPlane = 0.01f;
        public float FarClipPlane = 100.0f;
        public float FieldOfView = 90.0f;
        public ProjectionType Projection = ProjectionType.Perspective;
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
            });
        }

        public float ToRadians(float degree)
        {
            return (degree / 360) * MathF.PI;
        }

        public Matrix4x4 ProjectionMatrix
        {
            get
            {
                if (this.Projection == ProjectionType.Perspective)
                    return Matrix4x4.CreatePerspectiveFieldOfView(ToRadians(FieldOfView), (float)Resolution.x / (float)Resolution.y, NearClipPlane, FarClipPlane);
                else if (this.Projection == ProjectionType.Orthographic)
                    return Matrix4x4.CreateOrthographic(Resolution.x, Resolution.y, NearClipPlane, FarClipPlane);

                throw new NotSupportedException("This type of projection is not supported by the camera");
            }
        }
        public Matrix4x4 ViewMatrix
        {
            get
            {
                var viewMatrix = Matrix4x4.Identity;

                var transform = MyEntity.GetComponent<Transform>();
                if (transform != null)
                    viewMatrix = transform.ToMatrix;

                viewMatrix.M22 *= -1;
                return viewMatrix;
            }
        }

        internal BufferTransferObject UpdateCameraBuffers()
        {
            return _cameraBuffer.SetDataGetTransferObject<CameraShaderInfo>(
                new CameraShaderInfo[]
                {
                    new CameraShaderInfo
                    {
                        Projection = ProjectionMatrix,
                        View = ViewMatrix
                    }
                }
            );
        }
    }
}