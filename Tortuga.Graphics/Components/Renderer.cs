using System;
using System.Numerics;
using System.Threading.Tasks;
using Vulkan;

namespace Tortuga.Graphics
{
    /// <summary>
    /// Used to render a mesh into the scene
    /// </summary>
    public class Renderer : Core.BaseComponent
    {
        /// <summary>
        /// mesh data
        /// </summary>
        public Mesh MeshData;
        /// <summary>
        /// material data
        /// </summary>
        public Material MaterialData;

        internal Matrix4x4 Matrix
        {
            get
            {
                var mat = Matrix4x4.Identity;
                var transform = MyEntity.GetComponent<Core.Transform>();
                if (transform != null)
                    mat = transform.Matrix;
                return mat;
            }
        }
        /// <summary>
        /// get's if the renderer is static using the transform.static
        /// </summary>
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

        private API.CommandPool _renderCommandPool;
        private API.CommandPool.Command _renderCommand;
        private GraphicsModule _module;
        private DescriptorSetHelper _descriptorHelper;
        private const string MODEL_KEY = "MODEL";

        /// <summary>
        /// runs on start
        /// </summary>
        public override Task OnEnable()
        {
            return Task.Run(() =>
            {
                _module = Engine.Instance.GetModule<GraphicsModule>();

                //setup draw command
                _renderCommandPool = new API.CommandPool(
                    API.Handler.MainDevice, 
                    API.Handler.MainDevice.GraphicsQueueFamily
                );
                _renderCommand = _renderCommandPool.AllocateCommands(VkCommandBufferLevel.Secondary)[0];
                
                //setup descriptor helpers
                _descriptorHelper = new DescriptorSetHelper();
                _descriptorHelper.InsertKey(MODEL_KEY, _module.RenderDescriptorLayouts[2]);
                _descriptorHelper.BindBuffer(MODEL_KEY, 0, DescriptorSetHelper.MatrixToBytes(this.Matrix)).Wait();
            });
        }

        internal API.BufferTransferObject[] UpdateModel()
        {
            if (this.IsStatic)
                return new API.BufferTransferObject[]{};
            
            var transfer = _descriptorHelper.BindBufferWithTransferObject(
                MODEL_KEY, 
                0, 
                DescriptorSetHelper.MatrixToBytes(this.Matrix)
            );
            return new API.BufferTransferObject[] { transfer };
        }

        internal API.DescriptorSetPool.DescriptorSet ModelDescriptorSet => _descriptorHelper.DescriptorObjectMapper[MODEL_KEY].Set;

        internal API.CommandPool.Command BuildDrawCommand(Camera camera)
        {
            _renderCommand.Begin(
                VkCommandBufferUsageFlags.RenderPassContinue,
                _module.RenderPass,
                camera.Framebuffer
            );
            _renderCommand.BindPipeline(MaterialData.Pipeline);
            _renderCommand.BindDescriptorSets(
                MaterialData.Pipeline, 
                new API.DescriptorSetPool.DescriptorSet[]{
                    camera.ProjectionDescriptor,
                    camera.ViewDescriptor,
                    this.ModelDescriptorSet,
                    MaterialData.DescriptorSet
                }
            );
            _renderCommand.SetScissor(
                0, 0,
                Convert.ToUInt32(camera.Resolution.X),
                Convert.ToUInt32(camera.Resolution.Y)
            );
            _renderCommand.SetViewport(
                0, 0,
                Convert.ToUInt32(camera.Resolution.X),
                Convert.ToUInt32(camera.Resolution.Y)
            );
            _renderCommand.BindVertexBuffer(MeshData.VertexBuffer);
            _renderCommand.BindIndexBuffer(MeshData.IndexBuffer);
            _renderCommand.DrawIndexed(
                Convert.ToUInt32(MeshData.Indices.Length)
            );
            _renderCommand.Draw(3);
            _renderCommand.End();
            return _renderCommand;
        }
    }
}