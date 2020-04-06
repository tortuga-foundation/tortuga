using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Vulkan;

namespace Tortuga.Graphics.UI.Base
{
    /// <summary>
    /// This is a material class specialized for rendering user interface
    /// </summary>
    public class UiMaterial : DescritporSetHelper
    {
        internal API.Pipeline Pipeline => _pipeline;

        private Shader _shader;
        private PipelineInputBuilder _pipelineInput;
        private API.Pipeline _pipeline;

        /// <summary>
        /// Creates a Ui Material
        /// </summary>
        public UiMaterial(Shader shader, PipelineInputBuilder pipelineInputBuilder = null)
        {
            _shader = shader;
            _isDirty = true;
            if (pipelineInputBuilder == null)
                _pipelineInput = new PipelineInputBuilder();
            else
                _pipelineInput = pipelineInputBuilder;
        }

        /// <summary>
        /// Re Compile pipeline if it is dirty
        /// </summary>
        public void ReCompilePipeline()
        {
            if (_isDirty == false)
                return;

            var layouts = new List<API.DescriptorSetLayout>();
            layouts.Add(Engine.Instance.UiCameraDescriptorLayout);
            layouts.Add(Engine.Instance.UiBaseDescriptorLayout);
            foreach (var l in this.DescriptorMapper.Values)
                layouts.Add(l.Layout);
            _pipeline = new API.Pipeline(layouts.ToArray(), _shader, _pipelineInput);
            _isDirty = false;
        }

        /// <summary>
        /// Change shader being used
        /// </summary>
        /// <param name="shader">new shader to use</param>
        public void UpdateShader(Shader shader)
        {
            _shader = shader;
            _isDirty = true;
        }
    }
}