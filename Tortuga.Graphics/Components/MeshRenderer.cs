using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Tortuga.Graphics.API;
using Vulkan;

namespace Tortuga.Graphics
{
    /// <summary>
    /// Renders a mesh to the camera
    /// </summary>
    public class MeshRenderer : Core.BaseComponent
    {
        /// <summary>
        /// mesh to use for rendering
        /// </summary>
        public Mesh Mesh
        {
            get => _meshData;
            set => _meshData = value;
        }
        /// <summary>
        /// material to use for rendering
        /// </summary>
        public Material Material
        {
            get => _materialData;
            set => _materialData = value;
        }

        private const string MODEL_KEY = "_MODEL";
        private GraphicsModule _module;
        private API.CommandBuffer _renderCommand;
        private DescriptorService _descriptorService;
        private Mesh _meshData;
        private Material _materialData;

        /// <summary>
        /// runs on start
        /// </summary>
        public override Task OnEnable()
        => Task.Run(() =>
        {
            _descriptorService = new DescriptorService();

            _module = Engine.Instance.GetModule<GraphicsModule>();

            _renderCommand = _module.CommandBufferService.GetNewCommand(
                API.QueueFamilyType.Graphics,
                CommandType.Secondary
            );

            _descriptorService.InsertKey(MODEL_KEY, _module.DescriptorLayouts[MODEL_KEY]);
            _descriptorService.BindBuffer(MODEL_KEY, 0, Matrix.GetBytes());
        });

        private byte[] _matrixCache = null;

        /// <summary>
        /// updates model matrix
        /// </summary>
        public void UpdateDescriptorSet()
        {
            var matrix = Matrix.GetBytes();
            if (_matrixCache == matrix)
                return;

            if (IsStatic && _matrixCache != null)
                return;

            _matrixCache = matrix;
            _descriptorService.BindBuffer(
                MODEL_KEY,
                0,
                _matrixCache
            );
        }

        private List<DescriptorSet> GetDescriptorSets(
            DescriptorSet projection,
            DescriptorSet view,
            DescriptorSet model
        )
        {
            var descriptors = new List<DescriptorSet>();
            foreach (var o in _materialData.Handle)
            {
                if (o.Key.StartsWith('_'))
                {
                    switch (o.Key)
                    {
                        case "_PROJECTION":
                            descriptors.Add(projection);
                            break;
                        case "_VIEW":
                            descriptors.Add(view);
                            break;
                        case "_MODEL":
                            descriptors.Add(model);
                            break;
                        default:
                            throw new NotSupportedException("unknown type of descriptor set being used");
                    }
                }
                else
                {
                    descriptors.Add(o.Value.Set);
                }
            }
            return descriptors;
        }

        /// <summary>
        /// Constructs the secondary render command (for object).
        /// Used by the primary draw command
        /// </summary>
        /// <param name="framebuffer">Framebuffer to use for the secondary command buffer</param>
        /// <param name="subPass">SubPass to use for the secondary command buffer</param>
        /// <param name="projectionDescriptorSet">The projection matrix descriptor set</param>
        /// <param name="viewDescriptorSet">The view matrix descriptor set</param>
        /// <param name="viewport">The viewport where the objct should be rendered</param>
        /// <param name="resolution">The resolution of the rendering viewport</param>
        /// <returns>Secondary command buffer</returns>
        public API.CommandBuffer DrawCommand(
            API.Framebuffer framebuffer,
            uint subPass,
            API.DescriptorSet projectionDescriptorSet,
            API.DescriptorSet viewDescriptorSet,
            Vector4 viewport,
            Vector2 resolution
        )
        {
            _materialData.ReCompilePipeline();

            var materialDescriptorSets = GetDescriptorSets(
                projectionDescriptorSet,
                viewDescriptorSet,
                _descriptorService.Handle[MODEL_KEY].Set
            );

            var viewportX = Convert.ToInt32(viewport.X * resolution.X);
            var viewportY = Convert.ToInt32(viewport.Y * resolution.Y);
            var viewportWidth = Convert.ToUInt32(viewport.Z * resolution.X);
            var viewportHeight = Convert.ToUInt32(viewport.W * resolution.Y);

            _renderCommand.Begin(
                VkCommandBufferUsageFlags.RenderPassContinue,
                framebuffer.RenderPass,
                framebuffer,
                subPass
            );
            _renderCommand.BindPipeline(
                _materialData.Pipeline
            );
            _renderCommand.BindDescriptorSets(
                _materialData.Pipeline,
                materialDescriptorSets
            );
            _renderCommand.SetScissor(
                viewportX, viewportY,
                viewportWidth, viewportHeight
            );
            _renderCommand.SetViewport(
                viewportX, viewportY,
                viewportWidth, viewportHeight
            );
            _renderCommand.BindIndexBuffer(_meshData.IndexBuffer);
            _renderCommand.BindVertexBuffers(new List<API.Buffer> { _meshData.VertexBuffer });
            _renderCommand.DrawIndexed(
                Convert.ToUInt32(
                    _meshData.Indices.Length
                ),
                1
            );
            _renderCommand.End();
            return _renderCommand;
        }

        /// <summary>
        /// Returns Tortuga.Core.Transform.Matrix
        /// </summary>
        public Matrix4x4 Matrix
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
        /// Returns Tortuga.Core.Transform.IsStatic
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
    }
}