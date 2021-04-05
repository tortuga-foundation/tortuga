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
        internal Device Device => _module.GraphicsService.PrimaryDevice;

        private List<API.ShaderModule> _shaders;
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
        /// <param name="shadersPaths">path to shader files</param>
        public void SetShaders(List<string> shadersPaths)
        {
            _shaders = new List<ShaderModule>();
            foreach (var shaderPath in shadersPaths)
            {
                _shaders.Add(new ShaderModule(
                    _module.GraphicsService.PrimaryDevice,
                    shaderPath
                ));
            }
            _isDirty = true;
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
                _shaders,
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