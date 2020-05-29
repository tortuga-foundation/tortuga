namespace Tortuga.Graphics
{
    /// <summary>
    /// Material used for rendering a mesh
    /// </summary>
    public class Material
    {
        private const string TEXTURES_KEY = "TEXTURES";
        private DescriptorSetHelper _descriptorHelper;

        /// <summary>
        /// constructor for material
        /// </summary>
        public Material()
        {
            _descriptorHelper = new DescriptorSetHelper();
            var layouts = Engine.Instance.GetModule<GraphicsModule>().RenderDescriptorLayouts;
            _descriptorHelper.InsertKey(TEXTURES_KEY, layouts[1]);

            _descriptorHelper.BindImage(TEXTURES_KEY, 0, new ShaderPixel[]{ ShaderPixel.White }, 1, 1);
            _descriptorHelper.BindImage(TEXTURES_KEY, 1, new ShaderPixel[]{ ShaderPixel.White }, 1, 1);
            _descriptorHelper.BindImage(TEXTURES_KEY, 2, new ShaderPixel[]{ ShaderPixel.White }, 1, 1);
        }

        /// <summary>
        /// Set a single color as the albedo texture
        /// </summary>
        /// <param name="pixel">color to set</param>
        public void SetColor(ShaderPixel pixel)
        {
            _descriptorHelper.BindImage(TEXTURES_KEY, 0, new ShaderPixel[]{ pixel }, 1, 1);
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
            _descriptorHelper.BindImage(TEXTURES_KEY, 1, new ShaderPixel[]{ pixel }, 1, 1);
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
            _descriptorHelper.BindImage(TEXTURES_KEY, 2, new ShaderPixel[]{ pixel }, 1, 1);
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