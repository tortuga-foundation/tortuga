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

        private UiResources()
        {
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
                //color / texture
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
                    new RenderPass.CreateInfo(false, true)
                    {
                        InitialLayout = VkImageLayout.ColorAttachmentOptimal
                    }
                }
            );
            _pipeline = new Pipeline(
                _renderPass,
                _descriptorSetLayouts,
                new Shader(
                    Handler.MainDevice,
                    "Assets/Shaders/UI/Base.vert"
                ),
                new Shader(
                    Handler.MainDevice,
                    "Assets/Shaders/UI/Base.frag"
                ),
                new Graphics.PipelineInputBuilder()
            );
        }
    }
}