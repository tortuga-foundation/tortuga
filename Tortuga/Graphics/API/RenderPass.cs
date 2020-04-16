using System;
using Vulkan;
using Tortuga.Utils;
using static Vulkan.VulkanNative;

namespace Tortuga.Graphics.API
{
    internal class RenderPass
    {
        public VkRenderPass Handle => _renderPass;

        VkRenderPass _renderPass;

        public unsafe RenderPass()
        {
            var attachments = new NativeList<VkAttachmentDescription>();
            attachments.Add(new VkAttachmentDescription
            {
                format = VkFormat.R32g32b32a32Sfloat,
                samples = VkSampleCountFlags.Count1,
                loadOp = VkAttachmentLoadOp.Clear,
                storeOp = VkAttachmentStoreOp.Store,
                stencilLoadOp = VkAttachmentLoadOp.DontCare,
                stencilStoreOp = VkAttachmentStoreOp.DontCare,
                initialLayout = VkImageLayout.Undefined,
                finalLayout = VkImageLayout.ColorAttachmentOptimal
            });
            attachments.Add(new VkAttachmentDescription
            {
                format = VkFormat.D32Sfloat,
                samples = VkSampleCountFlags.Count1,
                loadOp = VkAttachmentLoadOp.Clear,
                storeOp = VkAttachmentStoreOp.DontCare,
                stencilLoadOp = VkAttachmentLoadOp.DontCare,
                stencilStoreOp = VkAttachmentStoreOp.DontCare,
                initialLayout = VkImageLayout.Undefined,
                finalLayout = VkImageLayout.DepthStencilAttachmentOptimal
            });

            var colorAttachmentRef = new VkAttachmentReference
            {
                attachment = 0,
                layout = VkImageLayout.ColorAttachmentOptimal
            };
            var depthAttachmentRef = new VkAttachmentReference
            {
                attachment = 1,
                layout = VkImageLayout.DepthStencilAttachmentOptimal
            };
            var subpass = new VkSubpassDescription
            {
                pipelineBindPoint = VkPipelineBindPoint.Graphics,
                colorAttachmentCount = 1,
                pColorAttachments = &colorAttachmentRef,
                pDepthStencilAttachment = &depthAttachmentRef
            };

            var renderPassInfo = VkRenderPassCreateInfo.New();
            renderPassInfo.attachmentCount = attachments.Count;
            renderPassInfo.pAttachments = (VkAttachmentDescription*)attachments.Data.ToPointer();
            renderPassInfo.subpassCount = 1;
            renderPassInfo.pSubpasses = &subpass;


            VkRenderPass renderPass;
            if (vkCreateRenderPass(
                Engine.Instance.MainDevice.LogicalDevice,
                &renderPassInfo,
                null,
                &renderPass
            ) != VkResult.Success)
                throw new Exception("failed to create render pass");
            _renderPass = renderPass;
        }

        unsafe ~RenderPass()
        {
            vkDestroyRenderPass(
                Engine.Instance.MainDevice.LogicalDevice,
                _renderPass,
                null
            );
        }
    }
}