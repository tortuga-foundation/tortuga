#pragma warning disable 0649
using System.Numerics;

namespace Tortuga.Graphics
{
    /// <summary>
    /// light component used for rendering
    /// </summary>
    public class Light : Core.BaseComponent
    {
        internal struct LightInfo
        {
            public Vector4 Position;
            public Vector4 Forward;
            public Vector4 Color;
            public int Type;
            public float Intensity;
            public int Reserved1;
            public int Reserved2;
        }

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
    
        internal LightInfo GetShaderInfo
        {
            get
            {
                var info = new LightInfo();
                info.Position = Vector4.Zero;
                info.Forward = new Vector4(0, 0, 1, 0);
                info.Color = new Vector4(255, 255, 255, 255);
                info.Intensity = 1.0f;
                info.Type = 0;
                return info;
            }
        }
    }
}