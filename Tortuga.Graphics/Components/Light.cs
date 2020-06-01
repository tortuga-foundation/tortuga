

namespace Tortuga.Graphics
{
    /// <summary>
    /// light component used for rendering
    /// </summary>
    public class Light : Core.BaseComponent
    {
        internal API.Framebuffer Framebuffer => _framebuffer;
        private API.Framebuffer _framebuffer;

        internal API.Pipeline Pipeline => _pipeline;
        private API.Pipeline _pipeline;
        private API.Shader _vertexShader;
        private API.Shader _fragmentShader;

        /// <summary>
        /// constructor for light component
        /// </summary>
        public Light()
        {
            var module = Engine.Instance.GetModule<GraphicsModule>();
            _framebuffer = new API.Framebuffer(
                module.LightRenderPass,
                1024, 1024
            );
            _vertexShader = new API.Shader(
                API.Handler.MainDevice,
                "Assets/Shaders/Default/Light.vert"
            );
            _fragmentShader = new API.Shader(
                API.Handler.MainDevice,
                "Assets/Shaders/Default/Light.frag"
            );
            _pipeline = new API.Pipeline(
                module.LightRenderPass,
                module.LightDescriptorSetLayouts,
                _vertexShader,
                _fragmentShader,
                new PipelineInputBuilder()
            );
        }
    }
}