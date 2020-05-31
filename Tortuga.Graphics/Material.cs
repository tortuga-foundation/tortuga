namespace Tortuga.Graphics
{
    /// <summary>
    /// Material used for rendering a mesh
    /// </summary>
    public class Material
    {
        private const string TEXTURES_KEY = "TEXTURES";

        private API.Shader _vertexShader;
        private API.Shader _fragmentShader;
        internal API.Pipeline Pipeline => _pipeline;
        private API.Pipeline _pipeline;

        internal API.DescriptorSetPool.DescriptorSet DescriptorSet
            => _descriptorHelper.DescriptorObjectMapper[TEXTURES_KEY].Set;

        private DescriptorSetHelper _descriptorHelper;

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
            _descriptorHelper = new DescriptorSetHelper();
            var layouts = Engine.Instance.GetModule<GraphicsModule>().RenderDescriptorLayouts;
            _descriptorHelper.InsertKey(TEXTURES_KEY, layouts[3]);
            //color texture
            _descriptorHelper.BindImage(TEXTURES_KEY, 0, new ShaderPixel[] { ShaderPixel.White }, 1, 1);
            //normal texture
            _descriptorHelper.BindImage(TEXTURES_KEY, 1, new ShaderPixel[] { ShaderPixel.Blue }, 1, 1);
            //detail texture
            _descriptorHelper.BindImage(TEXTURES_KEY, 2, new ShaderPixel[] { ShaderPixel.White }, 1, 1);
        }

        /// <summary>
        /// Set a single color as the albedo texture
        /// </summary>
        /// <param name="pixel">color to set</param>
        public void SetColor(ShaderPixel pixel)
        {
            _descriptorHelper.BindImage(TEXTURES_KEY, 0, new ShaderPixel[] { pixel }, 1, 1);
        }
        /// <summary>
        /// Set an image as albedo texture
        /// </summary>
        /// <param name="pixels">pixels</param>
        /// <param name="width">width</param>
        /// <param name="height">height</param>
        public void SetColor(ShaderPixel[] pixels, int width, int height)
        {
            _descriptorHelper.BindImage(TEXTURES_KEY, 0, pixels, width, height);
        }

        /// <summary>
        /// Set a single color as the normal texture
        /// </summary>
        /// <param name="pixel"></param>
        public void SetNormal(ShaderPixel pixel)
        {
            _descriptorHelper.BindImage(TEXTURES_KEY, 1, new ShaderPixel[] { pixel }, 1, 1);
        }

        /// <summary>
        /// Set an image as normal texture
        /// </summary>
        /// <param name="pixels">pixels</param>
        /// <param name="width">width</param>
        /// <param name="height">height</param>
        public void SetNormal(ShaderPixel[] pixels, int width, int height)
        {
            _descriptorHelper.BindImage(TEXTURES_KEY, 1, pixels, width, height);
        }

        /// <summary>
        /// Set a single color as the detail texture
        /// </summary>
        /// <param name="pixel"></param>
        public void SetDetail(ShaderPixel pixel)
        {
            _descriptorHelper.BindImage(TEXTURES_KEY, 2, new ShaderPixel[] { pixel }, 1, 1);
        }

        /// <summary>
        /// Set an image as detail texture
        /// </summary>
        /// <param name="pixels">pixels</param>
        /// <param name="width">width</param>
        /// <param name="height">height</param>
        public void SetDetail(ShaderPixel[] pixels, int width, int height)
        {
            _descriptorHelper.BindImage(TEXTURES_KEY, 2, pixels, width, height);
        }
    }
}