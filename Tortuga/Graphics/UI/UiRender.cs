using System.Collections.Generic;
using Vulkan;

namespace Tortuga.Graphics.UI
{
    public abstract class UiRender : UiComponent
    {
        internal static List<UiRender> UiRenderers = new List<UiRender>();
        internal Shader Shader;
        internal API.DescriptorSetLayout[] DescriptorSetLayout;
        internal API.DescriptorSetPool[] DescriptorSetPool;
        internal API.DescriptorSetPool.DescriptorSet[] DescriptorSet;
        internal PipelineInputBuilder PipelineInput;
        internal API.Pipeline Pipeline;
        internal API.CommandPool RenderCommandPool;
        internal API.CommandPool.Command RenderCommand;

        public UiRender()
        {
            RenderCommandPool = new API.CommandPool(
                Engine.Instance.MainDevice.GraphicsQueueFamily
            );
            RenderCommand = RenderCommandPool.AllocateCommands(VkCommandBufferLevel.Secondary)[0];
            UiRenderers.Add(this);
        }
        ~UiRender()
        {
            UiRenderers.Remove(this);
        }
        public void Destroy()
        {
            UiRenderers.Remove(this);
        }

        internal abstract API.BufferTransferObject[] UpdateBuffers();
        internal abstract API.CommandPool.Command BuildRenderCommand(Components.Camera camera);

    }
}