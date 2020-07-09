using Tortuga.Graphics.API;
using Vulkan;

namespace Tortuga.UI
{
    /// <summary>
    /// Contains resources for rendering user interface
    /// </summary>
    public class UiResources
    {
        /// <summary>
        /// user interface resources
        /// </summary>
        public static UiResources Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new UiResources();

                return _instance;
            }
        }
        private static UiResources _instance;

        /// <summary>
        /// default ui render pass
        /// </summary>
        public RenderPass RenderPass => _renderPass;
        private RenderPass _renderPass;

        /// <summary>
        /// default ui pipeline
        /// </summary>
        public Pipeline Pipeline => _pipeline;
        private Pipeline _pipeline;

        /// <summary>
        /// default ui descriptor sets
        /// </summary>
        public DescriptorSetLayout[] DescriptorSetLayouts => _descriptorSetLayouts;
        private DescriptorSetLayout[] _descriptorSetLayouts;
        /// <summary>
        /// default font used by user interface
        /// </summary>
        public UiFont DefaultFont => _defaultFont;
        private UiFont _defaultFont;

        private UiResources()
        {
            //setup ui font
            var defualtFont = UI.UiFont.LoadFromTTF("Assets/Fonts/Roboto-Regular.ttf");
            defualtFont.Wait();
            _defaultFont = defualtFont.Result;

            //setup default ui descriptor set layouts
            _descriptorSetLayouts = new DescriptorSetLayout[]
            {
                //projection
                new DescriptorSetLayout(
                    Handler.MainDevice,
                    new DescriptorSetCreateInfo[]
                    {
                        new DescriptorSetCreateInfo()
                        {
                            type = DescriptorType.UniformBuffer,
                            stage = ShaderStageType.Vertex
                        }
                    }
                ),
                //ui data
                new DescriptorSetLayout(
                    Handler.MainDevice,
                    new DescriptorSetCreateInfo[]
                    {
                        new DescriptorSetCreateInfo()
                        {
                            type = DescriptorType.UniformBuffer,
                            stage = ShaderStageType.Vertex | ShaderStageType.Fragment
                        }
                    }
                ),
                //texture
                new DescriptorSetLayout(
                    Handler.MainDevice,
                    new DescriptorSetCreateInfo[]
                    {
                        new DescriptorSetCreateInfo()
                        {
                            type = DescriptorType.CombinedImageSampler,
                            stage = ShaderStageType.Fragment
                        }
                    }
                )
            };

            //setup defult ui pipeline
            _renderPass = new RenderPass(
                Handler.MainDevice,
                new RenderPass.CreateInfo[]
                {
                    new RenderPass.CreateInfo()
                }
            );
            _pipeline = new Pipeline(
                _renderPass,
                _descriptorSetLayouts,
                new Shader(
                    Handler.MainDevice,
                    "Assets/Shaders/Default/UI.vert"
                ),
                new Shader(
                    Handler.MainDevice,
                    "Assets/Shaders/Default/UI.frag"
                ),
                new Graphics.PipelineInputBuilder()
            );
        }
    }
}