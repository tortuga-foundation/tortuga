using System.Threading.Tasks;

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

        internal API.DescriptorSetPool.DescriptorSet TexturesDescriptorSet
            => DescriptorObjectMapper[TEXTURES_KEY].Set;
        internal API.DescriptorSetPool.DescriptorSet MaterialDescriptorSet
            => DescriptorObjectMapper[MATERIAL_KEY].Set;


        /// <summary>
        /// constructor for material
        /// </summary>
        public Material(string vertexPath, string fragmentPath)
        {
            var graphicsModule = Engine.Instance.GetModule<GraphicsModule>();
            SetupShader(vertexPath, fragmentPath);
            this.InsertKey(
                TEXTURES_KEY,
                Engine.Instance.GetModule<GraphicsModule>().MeshDescriptorSetLayouts[3]
            );
            //color texture
            this.BindImage(TEXTURES_KEY, 0, new ShaderPixel[] { ShaderPixel.White }, 1, 1).Wait();
            //normal texture
            this.BindImage(TEXTURES_KEY, 1, new ShaderPixel[] { ShaderPixel.Blue }, 1, 1).Wait();
            //detail texture
            this.BindImage(TEXTURES_KEY, 2, new ShaderPixel[] { ShaderPixel.White }, 1, 1).Wait();

            //material
            this.InsertKey(
                MATERIAL_KEY,
                Engine.Instance.GetModule<GraphicsModule>().MeshDescriptorSetLayouts[4]
            );
            this.BindBuffer(MATERIAL_KEY, 0, new int[] { 0 }).Wait();
        }

        /// <summary>
        /// update shaders
        /// NOTE: this will auto call Re Compile pipeline
        /// </summary>
        public void SetupShader(string vertexPath, string fragmentPath)
        {
            _vertexShader = new API.Shader(API.Handler.MainDevice, vertexPath);
            _fragmentShader = new API.Shader(API.Handler.MainDevice, fragmentPath);
            ReCompilePipeline();
        }

        /// <summary>
        /// Re-compiles pipeline
        /// </summary>
        public void ReCompilePipeline()
        {
            var graphicsModule = Engine.Instance.GetModule<GraphicsModule>();
            _pipeline = new API.Pipeline(
                graphicsModule.MeshRenderPassMRT,
                graphicsModule.MeshDescriptorSetLayouts,
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
        /// Set a single color as the albedo texture
        /// </summary>
        /// <param name="pixel">color to set</param>
        public Task SetColor(ShaderPixel pixel)
        {
            return this.BindImage(TEXTURES_KEY, 0, new ShaderPixel[] { pixel }, 1, 1);
        }
        /// <summary>
        /// Set an image as albedo texture
        /// </summary>
        /// <param name="pixels">pixels</param>
        /// <param name="width">width</param>
        /// <param name="height">height</param>
        public Task SetColor(ShaderPixel[] pixels, int width, int height)
        {
            return this.BindImage(TEXTURES_KEY, 0, pixels, width, height);
        }

        /// <summary>
        /// Set a single color as the normal texture
        /// </summary>
        /// <param name="pixel"></param>
        public Task SetNormal(ShaderPixel pixel)
        {
            return this.BindImage(TEXTURES_KEY, 1, new ShaderPixel[] { pixel }, 1, 1);
        }

        /// <summary>
        /// Set an image as normal texture
        /// </summary>
        /// <param name="pixels">pixels</param>
        /// <param name="width">width</param>
        /// <param name="height">height</param>
        public Task SetNormal(ShaderPixel[] pixels, int width, int height)
        {
            return this.BindImage(TEXTURES_KEY, 1, pixels, width, height);
        }

        /// <summary>
        /// Set a single color as the detail texture
        /// </summary>
        /// <param name="pixel"></param>
        public Task SetDetail(ShaderPixel pixel)
        {
            return this.BindImage(TEXTURES_KEY, 2, new ShaderPixel[] { pixel }, 1, 1);
        }

        /// <summary>
        /// Set an image as detail texture
        /// </summary>
        /// <param name="pixels">pixels</param>
        /// <param name="width">width</param>
        /// <param name="height">height</param>
        public Task SetDetail(ShaderPixel[] pixels, int width, int height)
        {
            return this.BindImage(TEXTURES_KEY, 2, pixels, width, height);
        }

        /// <summary>
        /// Update's the shading model used for rendering the mesh
        /// </summary>
        /// <param name="type">type of shading model (flat, smooth)</param>
        public Task SetShading(ShadingType type)
        {
            return this.BindBuffer(MATERIAL_KEY, 0, new int[] { (int)type });
        }
    }
}