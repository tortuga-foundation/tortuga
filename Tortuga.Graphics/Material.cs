using System;
using System.Collections.Generic;
using System.Linq;
using Tortuga.Graphics.API;
using Vulkan;

namespace Tortuga.Graphics
{
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
        /// This material's pipeline
        /// </summary>
        public API.Pipeline Pipeline => _pipeline;

        private API.ShaderModule _vertexShader;
        private API.ShaderModule _fragmentShader;
        private API.Pipeline _pipeline;
        private bool _isDirty;
        private GraphicsModule _module;

        /// <summary>
        /// Constructor for material
        /// </summary>
        public Material()
        {
            _isDirty = true;
            _module = Engine.Instance.GetModule<GraphicsModule>();
        }

        /// <summary>
        /// set's the shaders being used for the pipeline
        /// </summary>
        /// <param name="vertex">vertex shader file path</param>
        /// <param name="fragment">fragment shader file path</param>
        public void SetShaders(string vertex, string fragment)
        {
            _vertexShader = new API.ShaderModule(
                _module.GraphicsService.PrimaryDevice,
                vertex
            );
            _fragmentShader = new API.ShaderModule(
                _module.GraphicsService.PrimaryDevice,
                fragment
            );
            _isDirty = true;
        }

        /// <summary>
        /// Compiles the pipeline using shaders set by the user
        /// </summary>
        public void ReCompilePipeline()
        {
            if (_isDirty == false)
                return;

            if (_vertexShader == null)
                throw new InvalidOperationException("vertex shader is not set");
            if (_fragmentShader == null)
                throw new InvalidOperationException("fragment shader is not set");

            var descriptorLayouts = new List<API.DescriptorLayout>();
            descriptorLayouts.Add(_module.DescriptorLayouts["_PROJECTION"]);
            descriptorLayouts.Add(_module.DescriptorLayouts["_VIEW"]);
            descriptorLayouts.Add(_module.DescriptorLayouts["_MODEL"]);
            foreach (var o in _handle)
                descriptorLayouts.Add(o.Value.Layout);

            _pipeline = new API.GraphicsPipeline(
                _module.GraphicsService.PrimaryDevice,
                _module.RenderPasses["_MRT"],
                descriptorLayouts,
                _vertexShader,
                _fragmentShader,
                Vertex.PipelineInput
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
    }
}