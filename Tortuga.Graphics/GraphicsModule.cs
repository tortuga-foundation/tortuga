#pragma warning disable 1591
using Vulkan;

namespace Tortuga.Graphics
{
    /// <summary>
    /// Graphics module
    /// </summary>
    public class GraphicsModule : Core.BaseModule
    {
        internal API.DescriptorSetLayout RenderDescriptorLayout => _descriptorLayout;
        private API.DescriptorSetLayout _descriptorLayout;

        public override void Destroy()
        {
        }

        public override void Init()
        {
            //initialize vulkan
            API.Handler.Init();
            _descriptorLayout = new API.DescriptorSetLayout(
                API.Handler.MainDevice,
                new API.DescriptorSetCreateInfo[]
                {
                    new API.DescriptorSetCreateInfo()
                    {
                        stage = VkShaderStageFlags.Compute,
                        type = VkDescriptorType.StorageImage
                    }
                }
            );
        }

        public override void Update()
        {
        }
    }
}