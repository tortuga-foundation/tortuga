using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace Tortuga.Graphics
{
    /// <summary>
    /// different types of light sources
    /// </summary>
    public enum TypeOfLight
    {
        /// <summary>
        /// directional light source
        /// </summary> 
        Directional = 0,
        /// <summary>
        /// point light source
        /// </summary> 
        Point = 1
    }

    /// <summary>
    /// light component used for rendering
    /// </summary>
    public class Light : Core.BaseComponent
    {
        /// <summary>
        /// Color of the light source
        /// </summary>
        public System.Drawing.Color Color = System.Drawing.Color.White;
        /// <summary>
        /// How strong is the light source
        /// </summary>
        public float Intensity = 1.0f;
        /// <summary>
        /// Defines the type of light
        /// </summary>
        public TypeOfLight Type = TypeOfLight.Directional;

        /// <summary>
        /// Shader information structure
        /// </summary>
        public struct LightShaderInfo
        {
            /// <summary>
            /// position of the light
            /// </summary>
            public Vector4 Position;
            /// <summary>
            /// direction of the light
            /// </summary>
            public Vector4 Forward;
            /// <summary>
            /// color of the light
            /// </summary>
            public Vector4 Color;
            /// <summary>
            /// type of light
            /// </summary>
            public int Type;
            /// <summary>
            /// intensity of light
            /// </summary>
            public float Intensity;
            /// <summary>
            /// reserved for future use
            /// </summary>
            public int Reserved1;
            /// <summary>
            /// reserved for future use
            /// </summary>
            public int Reserved2;
        }

        /// <summary>
        /// light framebuffer
        /// </summary>
        public API.Framebuffer Framebuffer => _framebuffer;
        /// <summary>
        /// pipeline for this light source
        /// </summary>
        public static API.Pipeline Pipeline => _pipeline;
        private API.Framebuffer _framebuffer;
        private static API.Pipeline _pipeline;

        /// <summary>
        /// Runs when component is enabled in the scene
        /// </summary>
        public override Task OnEnable()
        => Task.Run(() => 
        {
            var module = Engine.Instance.GetModule<GraphicsModule>();
            var LIGHT_KEY = "_LIGHT";
            _framebuffer = new API.Framebuffer(
                module.RenderPasses[LIGHT_KEY],
                1024, 1024
            );
            if (_pipeline == null)
            {
                _pipeline = new API.GraphicsPipeline(
                    module.GraphicsService.PrimaryDevice,
                    module.RenderPasses[LIGHT_KEY],
                    new List<API.DescriptorLayout>
                    {
                        module.DescriptorLayouts["_PROJECTION"],
                        module.DescriptorLayouts["_VIEW"],
                        module.DescriptorLayouts["_MODEL"]
                    },
                    new API.ShaderModule(
                        module.GraphicsService.PrimaryDevice,
                        "Assets/Shaders/Default/Light.vert"
                    ),
                    new API.ShaderModule(
                        module.GraphicsService.PrimaryDevice,
                        "Assets/Shaders/Default/Light.frag"
                    ),
                    new PipelineInputBuilder()
                );
            }
        });

        /// <summary>
        /// get's the shader information data for this component
        /// </summary>
        public LightShaderInfo ToShaderInfo
        {
            get
            {
                var position = Vector4.Zero;
                var forward = new Vector4(0, 0, 1, 0);

                var transform = MyEntity.GetComponent<Core.Transform>();
                if (transform != null)
                {
                    position = new Vector4(transform.Position, 1.0f);
                    forward = new Vector4(transform.Forward, 0.0f);
                }

                return new LightShaderInfo
                {
                    Position = position,
                    Forward = forward,
                    Color = new Vector4(
                        Color.R,
                        Color.G,
                        Color.B,
                        Color.A
                    ),
                    Intensity = Intensity,
                    Type = (int)Type
                };
            }
        }
    }
}