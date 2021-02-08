using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Vulkan;

namespace Tortuga.Graphics
{
    /// <summary>
    /// Renders a mesh to the camera
    /// </summary>
    public class MeshRenderer : Core.BaseComponent
    {
        private const string MODEL_KEY = "_MODEL";
        private GraphicsModule _module;
        private API.CommandBuffer _renderCommand;
        private DescriptorService _descriptorService;
        private Mesh MeshData;

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

        /// <summary>
        /// updates model matrix
        /// </summary>
        public void UpdateModel()
        {
            if (IsStatic)
                return;

            _descriptorService.BindBuffer(
                MODEL_KEY,
                0,
                Matrix.GetBytes()
            );
        }

        /// <summary>
        /// Constructs the secondary render command (for object).
        /// Used by the primary draw command
        /// </summary>
        /// <param name="renderPass">RenderPass to use for the secondary command buffer</param>
        /// <param name="framebuffer">Framebuffer to use for the secondary command buffer</param>
        /// <param name="subPass">SubPass to use for the secondary command buffer</param>
        /// <param name="ProjectionDescriptorSet">The projection matrix descriptor set</param>
        /// <param name="ViewDescriptorSet">The view matrix descriptor set</param>
        /// <param name="viewport">The viewport where the objct should be rendered</param>
        /// <returns>Secondary command buffer</returns>
        public API.CommandBuffer DrawCommand(
            API.RenderPass renderPass,
            API.Framebuffer framebuffer,
            uint subPass,
            API.DescriptorSet ProjectionDescriptorSet,
            API.DescriptorSet ViewDescriptorSet,
            Vector4 viewport
        )
        {
            //TODO: If material data is changes re-compile pipeline

            var viewportX = Convert.ToInt32(viewport.X);
            var viewportY = Convert.ToInt32(viewport.Y);
            var viewportWidth = Convert.ToUInt32(viewport.Z);
            var viewportHeight = Convert.ToUInt32(viewport.W);

            _renderCommand.Begin(
                VkCommandBufferUsageFlags.RenderPassContinue,
                renderPass,
                framebuffer,
                subPass
            );
            //TODO: Bind Pipeline
            //TODO: Bind DescriptorSets
            _renderCommand.SetScissor(
                viewportX, viewportY,
                viewportWidth, viewportHeight
            );
            _renderCommand.SetViewport(
                viewportX, viewportY,
                viewportWidth, viewportHeight
            );
            _renderCommand.BindIndexBuffer(MeshData.IndexBuffer);
            _renderCommand.BindVertexBuffers(new List<API.Buffer> { MeshData.VertexBuffer });
            _renderCommand.DrawIndexed(
                Convert.ToUInt32(
                    MeshData.Indices.Length
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