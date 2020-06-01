#pragma warning disable 1591

namespace Tortuga.Graphics
{
    /// <summary>
    /// Graphics module
    /// </summary>
    public class GraphicsModule : Core.BaseModule
    {
        internal API.DescriptorSetLayout[] RenderDescriptorLayouts => _descriptorLayouts;
        private API.DescriptorSetLayout[] _descriptorLayouts;
        internal API.RenderPass RenderPass => _renderPass;
        private API.RenderPass _renderPass;

        public override void Destroy()
        {
        }

        public override void Init()
        {
            //initialize vulkan
            API.Handler.Init();
            //setup render pass
            _renderPass = new API.RenderPass(
                API.Handler.MainDevice,
                new API.RenderPass.CreateInfo[]
                {
                    new API.RenderPass.CreateInfo(),
                    new API.RenderPass.CreateInfo(),
                    new API.RenderPass.CreateInfo(),
                    new API.RenderPass.CreateInfo()
                },
                new API.RenderPass.CreateInfo()
            );
            //setup descriptor sets
            _descriptorLayouts = new API.DescriptorSetLayout[]
            {
                //PROJECTION
                new API.DescriptorSetLayout(
                    API.Handler.MainDevice,
                    new API.DescriptorSetCreateInfo[]
                    {
                        new API.DescriptorSetCreateInfo()
                        {
                            stage = API.ShaderStageType.Vertex,
                            type = API.DescriptorType.UniformBuffer
                        }
                    }
                ),
                //VIEW
                new API.DescriptorSetLayout(
                    API.Handler.MainDevice,
                    new API.DescriptorSetCreateInfo[]
                    {
                        new API.DescriptorSetCreateInfo()
                        {
                            stage = API.ShaderStageType.Vertex,
                            type = API.DescriptorType.UniformBuffer
                        }
                    }
                ),
                //MODEL
                new API.DescriptorSetLayout(
                    API.Handler.MainDevice,
                    new API.DescriptorSetCreateInfo[]
                    {
                        new API.DescriptorSetCreateInfo()
                        {
                            stage = API.ShaderStageType.Vertex,
                            type = API.DescriptorType.UniformBuffer
                        }
                    }
                ),
                //rendered image settings
                new API.DescriptorSetLayout(
                    API.Handler.MainDevice,
                    new API.DescriptorSetCreateInfo[]
                    {
                        new API.DescriptorSetCreateInfo()
                        {
                            stage = API.ShaderStageType.Fragment,
                            type = API.DescriptorType.CombinedImageSampler
                        },
                        new API.DescriptorSetCreateInfo()
                        {
                            stage = API.ShaderStageType.Fragment,
                            type = API.DescriptorType.CombinedImageSampler
                        },
                        new API.DescriptorSetCreateInfo()
                        {
                            stage = API.ShaderStageType.Fragment,
                            type = API.DescriptorType.CombinedImageSampler
                        }
                    }
                )
            };
        }

        public override void Update()
        {
        }
    }
}