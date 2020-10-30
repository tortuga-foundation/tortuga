using System.Collections.Generic;
using System.Threading.Tasks;
using Tortuga.Graphics.API;
using System.IO;
using System.Text.Json;
using Tortuga.Graphics.Json;

namespace Tortuga.Graphics
{
    /// <summary>
    /// Shading type to use in vertex shader
    /// </summary>
    public enum ShadingType
    {
        /// <summary>
        /// use flat shading type
        /// </summary>
        Flat = 0,
        /// <summary>
        /// use smooth shading type
        /// </summary>
        Smooth = 1
    }

    /// <summary>
    /// Material used for rendering a mesh
    /// </summary>
    public class Material : DescriptorSetHelper
    {
        private const string TEXTURES_KEY = "TEXTURES";
        private const string MATERIAL_KEY = "MATERIAL";

        private API.Shader _vertexShader;
        private API.Shader _fragmentShader;
        internal API.Pipeline Pipeline => _pipeline;
        private API.Pipeline _pipeline;

        internal bool IsDirty => _isDirty;
        private bool _isDirty;

        /// <summary>
        /// constructor for material
        /// </summary>
        public Material(Shader vertex, Shader fragment)
        {
            SetShaders(vertex, fragment);
        }

        /// <summary>
        /// update's the current pipeline to use specific shaders
        /// </summary>
        /// <param name="vertex">vertex shader</param>
        /// <param name="fragment">fragment shader</param>
        public void SetShaders(Shader vertex, Shader fragment)
        {
            _isDirty = true;
            _vertexShader = vertex;
            _fragmentShader = fragment;
        }

        /// <summary>
        /// Re-compiles pipeline
        /// </summary>
        public void ReCompilePipeline()
        {
            var graphicsModule = Engine.Instance.GetModule<GraphicsModule>();

            var descriptorSetLayoutList = new List<DescriptorSetLayout>();
            foreach (var layout in graphicsModule.MeshDescriptorSetLayouts)
                descriptorSetLayoutList.Add(layout);
            foreach (var mapper in DescriptorObjectMapper.Values)
                descriptorSetLayoutList.Add(mapper.Layout);

            _pipeline = new API.Pipeline(
                graphicsModule.MeshRenderPassMRT,
                descriptorSetLayoutList.ToArray(),
                _vertexShader,
                _fragmentShader,
                new PipelineInputBuilder(
                    new PipelineInputBuilder.BindingElement[]
                    {
                        new PipelineInputBuilder.BindingElement()
                        {
                            Type = PipelineInputBuilder.BindingElement.BindingType.Vertex,
                            Elements = new PipelineInputBuilder.AttributeElement[]
                            {
                                //position
                                new PipelineInputBuilder.AttributeElement(
                                    PipelineInputBuilder.AttributeElement.FormatType.Float3
                                ),
                                //texture
                                new PipelineInputBuilder.AttributeElement(
                                    PipelineInputBuilder.AttributeElement.FormatType.Float2
                                ),
                                //normal
                                new PipelineInputBuilder.AttributeElement(
                                    PipelineInputBuilder.AttributeElement.FormatType.Float3
                                ),
                                //tangent
                                new PipelineInputBuilder.AttributeElement(
                                    PipelineInputBuilder.AttributeElement.FormatType.Float3
                                ),
                                //bi-tangent
                                new PipelineInputBuilder.AttributeElement(
                                    PipelineInputBuilder.AttributeElement.FormatType.Float3
                                ),
                            }
                        }
                    }
                )
            );
        }

        /// <summary>
        /// creates a descriptor set that can be used to pass data to the graphics card
        /// </summary>
        /// <param name="key">unique key for this descriptor set</param>
        /// <param name="layout">defines the type of data and at which stage the data should be passed to the graphics card</param>
        public override void InsertKey(string key, DescriptorSetLayout layout)
        {
            _isDirty = true;
            base.InsertKey(key, layout);
        }

        /// <summary>
        /// Removes a descriptor set
        /// </summary>
        /// <param name="key">unique key for this descriptor set</param>
        public override void RemoveKey(string key)
        {
            _isDirty = true;
            base.RemoveKey(key);
        }

        /// <summary>
        /// loads a material from json file
        /// </summary>
        /// <param name="filePath">path to jsonn file</param>
        /// <returns>material object</returns>
        public static async Task<Material> Load(string filePath)
        {
            var content = File.ReadAllText(filePath);
            var data = JsonSerializer.Deserialize<JsonMaterial>(content);
            return await data.ToMaterial();
        }
    }
}