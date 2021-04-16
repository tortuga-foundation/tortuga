using System;
using System.Collections.Generic;
using System.Numerics;
using Tortuga.Graphics.API;
using Vulkan;

namespace Tortuga.Graphics
{
    internal class InstanceDataHelper
    {
        public byte[] Cache;
        public API.Buffer Staging;
        public API.Buffer Buffer;
        public API.CommandBuffer TransferCommand;
    }

    /// <summary>
    /// Material used for rendering a mesh
    /// </summary>
    public class Material : DescriptorService
    {
        /// <summary>
        /// If this is true then the pipeline needs to be re-compiled
        /// </summary>
        public bool IsDirty => _isDirty;
        /// <summary>
        /// get's the json material object
        /// </summary>
        public AssetLoader.JsonMaterial JsonMaterail => _json;
        /// <summary>
        /// This material's pipeline
        /// </summary>
        public API.Pipeline Pipeline => _pipeline;
        /// <summary>
        /// is this material instanced
        /// </summary>
        public bool Instanced
        {
            get => _instanced;
            set
            {
                _instanced = value;
                _isDirty = true;
            }
        }
        internal Device Device => _module.GraphicsService.PrimaryDevice;

        private List<API.ShaderModule> _shaders;
        private API.Pipeline _pipeline;
        private bool _isDirty;
        private bool _instanced;
        private GraphicsModule _module;
        private AssetLoader.JsonMaterial _json;
        private API.CommandBuffer _instancedDrawCommand;
        private Dictionary<Mesh, InstanceDataHelper> _instanceData;

        /// <summary>
        /// Constructor for material
        /// </summary>
        public Material(AssetLoader.JsonMaterial json = null)
        {
            _json = json;
            _isDirty = true;
            _instanced = false;
            _module = Engine.Instance.GetModule<GraphicsModule>();
            _instanceData = new Dictionary<Mesh, InstanceDataHelper>();
        }

        /// <summary>
        /// set's the shaders being used for the pipeline
        /// </summary>
        /// <param name="shaders">list of shader modules</param>
        public void SetShaders(List<API.ShaderModule> shaders)
        {
            _shaders = shaders;
            _isDirty = true;
        }

        private List<API.DescriptorLayout> InitDescriptorLayouts()
        {
            var descriptorLayouts = new List<API.DescriptorLayout>();
            foreach (var o in _handle)
            {
                if (o.Key.StartsWith('_'))
                {
                    switch (o.Key)
                    {
                        case "_PROJECTION":
                            descriptorLayouts.Add(_module.DescriptorLayouts["_PROJECTION"]);
                            break;
                        case "_VIEW":
                            descriptorLayouts.Add(_module.DescriptorLayouts["_VIEW"]);
                            break;
                        case "_MODEL":
                            descriptorLayouts.Add(_module.DescriptorLayouts["_MODEL"]);
                            break;
                        default:
                            throw new NotSupportedException("this type of descriptor set is not supported");
                    }
                }
                else
                {
                    descriptorLayouts.Add(o.Value.Layout);
                }
            }
            return descriptorLayouts;
        }

        /// <summary>
        /// Compiles the pipeline using shaders set by the user
        /// </summary>
        public void ReCompilePipeline()
        {
            if (_isDirty == false)
                return;

            if (_shaders == null || _shaders.Count == 0)
                throw new Exception("No Shaders has been set for this material");


            var pipelineInput = Vertex.PipelineInput;
            if (_instanced)
                pipelineInput = Vertex.PipelineInstancedInput;
            var descriptorLayouts = InitDescriptorLayouts();
            _pipeline = new API.GraphicsPipeline(
                _module.GraphicsService.PrimaryDevice,
                _module.RenderPasses["_MRT"],
                descriptorLayouts,
                _shaders,
                pipelineInput
            );
            _instancedDrawCommand = _module.CommandBufferService.GetNewCommand(
                QueueFamilyType.Graphics,
                CommandType.Secondary
            );
        }

        /// <summary>
        /// Create a new descriptor type binding
        /// </summary>
        /// <param name="key">key for this descriptor set</param>
        /// <param name="layout">what type of data does this descriptor set have?</param>
        public override void InsertKey(string key, DescriptorLayout layout)
        {
            _isDirty = true;
            base.InsertKey(key, layout);
        }

        /// <summary>
        /// Remvoe an existing descriptor set
        /// </summary>
        /// <param name="key">key of the descriptor set</param>
        public override void RemoveKey(string key)
        {
            _isDirty = true;
            base.RemoveKey(key);
        }

        private List<DescriptorSet> GetDescriptorSets(
            DescriptorSet projection,
            DescriptorSet view
        )
        {
            var descriptors = new List<DescriptorSet>();
            foreach (var o in _handle)
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
        /// converts meshes position, rotation and scale to byte array
        /// </summary>
        /// <param name="meshRenderers">meshes to get the data from</param>
        /// <returns>byte array instanced data</returns>
        public byte[] GetInstancedData(List<MeshRenderer> meshRenderers)
        {
            var bytes = new List<byte>();
            foreach (var mesh in meshRenderers)
            {
                var transform = mesh.MyEntity.GetComponent<Core.Transform>();
                if (transform == null)
                {
                    for (int i = 0; i < 3; i++)
                        foreach (var b in Vector3.Zero.GetBytes())
                            bytes.Add(b);
                    continue;
                }

                foreach (var b in transform.InstancedData)
                    bytes.Add(b);
            }
            return bytes.ToArray();
        }

        /// <summary>
        /// updates the instanced buffer
        /// </summary>
        /// <param name="meshData">mesh data</param>
        /// <param name="meshRenderers">meshes being rendered</param>
        public API.CommandBuffer UpdateInstanceBuffers(Mesh meshData, List<MeshRenderer> meshRenderers)
        {
            var device = _module.GraphicsService.PrimaryDevice;
            var instancedData = GetInstancedData(meshRenderers);
            if (_instanceData.ContainsKey(meshData) && _instanceData[meshData].Cache == instancedData)
                return null;

            if (_instanceData.ContainsKey(meshData) == false)
            {
                _instanceData[meshData] = new InstanceDataHelper
                {
                    Cache = instancedData,
                    Staging = new API.Buffer(
                        device,
                        Convert.ToUInt32(sizeof(byte) * instancedData.Length),
                        VkBufferUsageFlags.TransferSrc,
                        VkMemoryPropertyFlags.HostCoherent | VkMemoryPropertyFlags.HostVisible
                    ),
                    Buffer = new API.Buffer(
                        device,
                        Convert.ToUInt32(sizeof(byte) * instancedData.Length),
                        VkBufferUsageFlags.VertexBuffer | VkBufferUsageFlags.TransferDst,
                        VkMemoryPropertyFlags.DeviceLocal
                    ),
                    TransferCommand = _module.CommandBufferService.GetNewCommand(
                        QueueFamilyType.Graphics,
                        CommandType.Primary
                    )
                };

                // setup copy command
                _instanceData[meshData].TransferCommand.Begin(
                    VkCommandBufferUsageFlags.SimultaneousUse
                );
                _instanceData[meshData].TransferCommand.CopyBuffer(
                    _instanceData[meshData].Staging,
                    _instanceData[meshData].Buffer
                );
                _instanceData[meshData].TransferCommand.End();
            }

            _instanceData[meshData].Staging.SetData(instancedData);
            return _instanceData[meshData].TransferCommand;
        }

        /// <summary>
        /// draws mesh using gpu instancing
        /// </summary>
        public API.CommandBuffer DrawInstanced(
            Mesh meshData,
            List<MeshRenderer> meshRenderers,
            API.Framebuffer framebuffer,
            uint subpass,
            API.DescriptorSet projectionDescriptorSet,
            API.DescriptorSet viewDescriptorSet,
            Vector4 viewport,
            Vector2 resolution
        )
        {
            if (_instanced == false)
                throw new InvalidOperationException("trying to draw using instanced function when material is not instanced");

            ReCompilePipeline();
            var descriptorsets = GetDescriptorSets(
                projectionDescriptorSet,
                viewDescriptorSet
            );

            var viewportX = Convert.ToInt32(viewport.X * resolution.X);
            var viewportY = Convert.ToInt32(viewport.Y * resolution.Y);
            var viewportWidth = Convert.ToUInt32(viewport.Z * resolution.X);
            var viewportHeight = Convert.ToUInt32(viewport.W * resolution.Y);

            _instancedDrawCommand.Begin(
                Vulkan.VkCommandBufferUsageFlags.OneTimeSubmit,
                framebuffer.RenderPass,
                framebuffer,
                subpass
            );
            _instancedDrawCommand.BindPipeline(_pipeline);
            _instancedDrawCommand.BindDescriptorSets(
                _pipeline,
                descriptorsets
            );
            _instancedDrawCommand.SetScissor(
                viewportX, viewportY,
                viewportWidth, viewportHeight
            );
            _instancedDrawCommand.SetViewport(
                viewportX, viewportY,
                viewportWidth, viewportHeight
            );
            _instancedDrawCommand.BindIndexBuffer(meshData.IndexBuffer);
            _instancedDrawCommand.BindVertexBuffers(
                new List<API.Buffer>
                {
                    meshData.VertexBuffer,
                    _instanceData[meshData].Buffer
                }
            );
            _instancedDrawCommand.DrawIndexed(
                Convert.ToUInt32(meshData.Indices.Length),
                Convert.ToUInt32(meshRenderers.Count)
            );
            _instancedDrawCommand.End();
            return _instancedDrawCommand;
        }
    }
}