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

        public override void Destroy()
        {
        }

        public override void Init()
        {
            //initialize vulkan
            API.Handler.Init();
            //setup descriptor sets
            _descriptorLayouts = new API.DescriptorSetLayout[]
            {
                //output rendered image
                new API.DescriptorSetLayout(
                    API.Handler.MainDevice,
                    new API.DescriptorSetCreateInfo[]
                    {
                        new API.DescriptorSetCreateInfo()
                        {
                            stage = API.ShaderStageType.Compute,
                            type = API.DescriptorType.StorageImage
                        },
                    }
                ),
                //rendered image settings
                new API.DescriptorSetLayout(
                    API.Handler.MainDevice,
                    new API.DescriptorSetCreateInfo[]
                    {
                        new API.DescriptorSetCreateInfo()
                        {
                            stage = API.ShaderStageType.Compute,
                            type = API.DescriptorType.UniformBuffer
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