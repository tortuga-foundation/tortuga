using System;
using System.Threading.Tasks;
using Vulkan;
using Tortuga.Graphics.API;
using System.Numerics;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Tortuga.Graphics
{
    /// <summary>
    /// This object is used to render a mesh with a material
    /// </summary>
    public class RenderMesh : Core.BaseComponent
    {
        /// <summary>
        /// if false then this mesh is skipped from rendering
        /// </summary>
        public bool IsActive;

        /// <summary>
        /// material to use for rendering
        /// </summary>
        public Material Material
        {
            set => _material = value;
            get => _material;
        }
        /// <summary>
        /// Mesh data to use for rendering
        /// NOTE: please use SetMesh for setting new mesh data
        /// </summary>
        public Graphics.Mesh Mesh;
        /// <summary>
        /// Is mesh static
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
        /// Get Model matrix of this mesh
        /// </summary>
        public Matrix4x4 ModelMatrix
        {
            get
            {
                var transform = MyEntity.GetComponent<Core.Transform>();
                if (transform == null)
                    return Matrix4x4.Identity;

                return transform.Matrix;
            }
        }
        /// <summary>
        /// Get Position of this mesh
        /// </summary>
        public Vector3 Position
        {
            get
            {
                var transform = MyEntity.GetComponent<Core.Transform>();
                if (transform == null)
                    return Vector3.Zero;

                return transform.Position;
            }
        }
        /// <summary>
        /// Get Rotation of this mesh
        /// </summary>
        public Vector4 Rotation
        {
            get
            {
                var transform = MyEntity.GetComponent<Core.Transform>();
                if (transform == null)
                    return Vector4.Zero;

                return new Vector4(transform.Rotation.X, transform.Rotation.Y, transform.Rotation.Z, transform.Rotation.W);
            }
        }
        /// <summary>
        /// Get Scale of this mesh
        /// </summary>
        public Vector3 Scale
        {
            get
            {
                var transform = MyEntity.GetComponent<Core.Transform>();
                if (transform == null)
                    return Vector3.Zero;

                return transform.Scale;
            }
        }


        internal CommandPool.Command RenderCommand => _renderCommand;
        internal Graphics.API.Buffer VertexBuffers => Mesh.VertexBuffer;
        internal Graphics.API.Buffer IndexBuffer => Mesh.IndexBuffer;
        internal uint IndicesCount => Convert.ToUInt32(Mesh.Indices.Length);

        private Graphics.Material _material;
        private CommandPool _renderCommandPool;
        private CommandPool.Command _renderCommand;
        private DescriptorSetPool _uniformDescriptorPool;
        private DescriptorSetPool.DescriptorSet _uniformDescriptorSet;
        private Graphics.API.Buffer _uniformBuffer;

        /// <summary>
        /// Initialize Component
        /// </summary>
        public async override Task OnEnable()
        {
            await Task.Run(() =>
            {
                if (Material == null)
                    Material = Material.ErrorMaterial;

                _renderCommandPool = new CommandPool(Engine.Instance.MainDevice.GraphicsQueueFamily);
                _renderCommand = _renderCommandPool.AllocateCommands(VkCommandBufferLevel.Secondary)[0];
                _uniformDescriptorPool = new DescriptorSetPool(Engine.Instance.ModelDescriptorLayout);
                _uniformDescriptorSet = _uniformDescriptorPool.AllocateDescriptorSet();
                _uniformBuffer = Graphics.API.Buffer.CreateDevice(
                    System.Convert.ToUInt32(Unsafe.SizeOf<System.Numerics.Matrix4x4>()),
                    VkBufferUsageFlags.UniformBuffer
                );
                _uniformDescriptorSet.BuffersUpdate(_uniformBuffer);
                this.IsActive = true;
            });
        }

        internal Task<CommandPool.Command> RecordRenderCommand(
            Camera camera,
            Graphics.API.Buffer instanceBuffer = null,
            int InstanceCount = 0
        )
        {
            this.RenderCommand.Begin(VkCommandBufferUsageFlags.RenderPassContinue, camera.Framebuffer, 0);

            //viewport and scissor
            var windowSize = Engine.Instance.MainWindow.Size;
            int viewportX = System.Convert.ToInt32(System.Math.Round(windowSize.X * camera.Viewport.X));
            int viewportY = System.Convert.ToInt32(System.Math.Round(windowSize.Y * camera.Viewport.Y));
            uint viewportWidth = System.Convert.ToUInt32(System.Math.Round(camera.Resolution.X * camera.Viewport.Z));
            uint viewportHeight = System.Convert.ToUInt32(System.Math.Round(camera.Resolution.Y * camera.Viewport.W));
            this.RenderCommand.SetViewport(viewportX, viewportY, viewportWidth, viewportHeight);
                this.RenderCommand.SetScissor(viewportX, viewportY, viewportWidth, viewportHeight);

            var descriptorSets = new List<DescriptorSetPool.DescriptorSet>();
            descriptorSets.Add(camera.CameraDescriptorSet);
            descriptorSets.Add(this._uniformDescriptorSet);
            foreach (var d in this.Material.DescriptorSets)
                descriptorSets.Add(d);
            this.Material.ReCompilePipeline();
            this.RenderCommand.BindPipeline(this.Material.ActivePipeline);
            this.RenderCommand.BindDescriptorSets(this.Material.ActivePipeline, descriptorSets.ToArray());
            this.RenderCommand.BindVertexBuffer(this.VertexBuffers, 0);
            this.RenderCommand.BindIndexBuffer(this.IndexBuffer);
            if (instanceBuffer == null)
            {
                this.RenderCommand.DrawIndexed(this.IndicesCount);
            }
            else
            {
                this.RenderCommand.BindVertexBuffer(instanceBuffer, 1);
                this.RenderCommand.DrawIndexed(this.IndicesCount, System.Convert.ToUInt32(InstanceCount));
            }
            this.RenderCommand.End();
            return Task.FromResult(this.RenderCommand);
        }
        internal Light.FullShaderInfo RenderingLights(Light[] lights)
        {
            System.Array.Sort(lights, (Light left, Light right) =>
            {
                var leftDist = Vector3.Distance(left.Position, this.Position);
                var rightDist = Vector3.Distance(right.Position, this.Position);
                return System.Convert.ToInt32(System.MathF.Round(leftDist - rightDist));
            });
            if (lights.Length > 10)
                System.Array.Resize(ref lights, 10);
            var infoList = new List<Light.LightShaderInfo>();
            foreach (var l in lights)
                infoList.Add(l.BuildShaderInfo);
            for (int i = infoList.Count; i < 10; i++)
                infoList.Add(new Light.LightShaderInfo());
            return new Light.FullShaderInfo
            {
                Count = lights.Length,
                Light0 = infoList[0],
                Light1 = infoList[1],
                Light2 = infoList[2],
                Light3 = infoList[3],
                Light4 = infoList[4],
                Light5 = infoList[5],
                Light6 = infoList[6],
                Light7 = infoList[7],
                Light8 = infoList[8],
                Light9 = infoList[9]
            };
        }
        internal BufferTransferObject UpdateUniformBuffer()
        {
            return _uniformBuffer.SetDataGetTransferObject(new Matrix4x4[] { ModelMatrix });
        }
    }
}