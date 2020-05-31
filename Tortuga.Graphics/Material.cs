namespace Tortuga.Graphics
{
    /// <summary>
    /// Material used for rendering a mesh
    /// </summary>
    public class Material
    {
        private API.Shader _vertexShader;
        private API.Shader _fragmentShader;
        internal API.Pipeline Pipeline => _pipeline;
        private API.Pipeline _pipeline;

        /// <summary>
        /// constructor for material
        /// </summary>
        public Material()
        {
            var graphicsModule = Engine.Instance.GetModule<GraphicsModule>();

            _vertexShader = new API.Shader(
                API.Handler.MainDevice,  
                "Assets/Shaders/Default/Default.vert"
            );
            _fragmentShader = new API.Shader(
                API.Handler.MainDevice,
                "Assets/Shaders/Default/Default.frag"
            );
            _pipeline = new API.Pipeline(
                graphicsModule.RenderPass,
                graphicsModule.RenderDescriptorLayouts,
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
                                )
                            }
                        }
                    }
                )
            );
        }
    }
}