using System;
using Vulkan;
using Tortuga.Utils;
using static Vulkan.VulkanNative;

namespace Tortuga.Graphics.API
{
    internal class RenderPass
    {
        public VkRenderPass Handle => _renderPass;
        public Device DeviceUsed => _device;

        private VkRenderPass _renderPass;
        private Device _device;

        public unsafe RenderPass(Device device)
        {
            _device = device;

            //setup attachment descriptions
            var attachments = new NativeList<VkAttachmentDescription>();
            for (int i = 0; i < 4; i++)
            {
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
            }
            attachments[0].format = VkFormat.R32g32b32a32Sfloat;
            attachments[1].format = VkFormat.R32g32b32a32Sfloat;
            attachments[2].format = VkFormat.R32g32b32a32Sfloat;
            attachments[3].format = VkFormat.D32Sfloat;

            //color attachments
            var colorAttachmentRefs = new NativeList<VkAttachmentReference>();
            colorAttachmentRefs.Add(new VkAttachmentReference()
            {
                attachment = 0,
                layout = VkImageLayout.ColorAttachmentOptimal
            });
            colorAttachmentRefs.Add(new VkAttachmentReference()
            {
                attachment = 1,
                layout = VkImageLayout.ColorAttachmentOptimal
            });
            colorAttachmentRefs.Add(new VkAttachmentReference()
            {
                attachment = 2,
                layout = VkImageLayout.ColorAttachmentOptimal
            });
            //depth attachment
            var depthAttachmentRef = new VkAttachmentReference
            {
                attachment = 3,
                layout = VkImageLayout.DepthStencilAttachmentOptimal
            };

            //create sub pass
            var subpass = new VkSubpassDescription
            {
                pipelineBindPoint = VkPipelineBindPoint.Graphics,
                colorAttachmentCount = colorAttachmentRefs.Count,
                pColorAttachments = (VkAttachmentReference*)colorAttachmentRefs.Data.ToPointer(),
                pDepthStencilAttachment = &depthAttachmentRef
            };

            //create dependencies
            var dependencies = new NativeList<VkSubpassDependency>();
            dependencies.Add(new VkSubpassDependency
            {
                srcSubpass = (~0U),//VK_SUBPASS_EXTERNAL
                dstSubpass = 0,
                srcStageMask = VkPipelineStageFlags.BottomOfPipe,
                dstStageMask = VkPipelineStageFlags.ColorAttachmentOutput,
                srcAccessMask = VkAccessFlags.MemoryRead,
                dstAccessMask = VkAccessFlags.ColorAttachmentRead | VkAccessFlags.ColorAttachmentWrite,
                dependencyFlags = VkDependencyFlags.ByRegion
            });
            dependencies.Add(new VkSubpassDependency
            {
                srcSubpass = 0,
                dstSubpass = (~0U),//VK_SUBPASS_EXTERNAL
                srcStageMask = VkPipelineStageFlags.ColorAttachmentOutput,
                dstStageMask = VkPipelineStageFlags.BottomOfPipe,
                srcAccessMask = VkAccessFlags.ColorAttachmentRead | VkAccessFlags.ColorAttachmentWrite,
                dstAccessMask = VkAccessFlags.MemoryRead,
                dependencyFlags = VkDependencyFlags.ByRegion
            });

            var renderPassInfo = VkRenderPassCreateInfo.New();
            renderPassInfo.attachmentCount = attachments.Count;
            renderPassInfo.pAttachments = (VkAttachmentDescription*)attachments.Data.ToPointer();
            renderPassInfo.subpassCount = 1;
            renderPassInfo.pSubpasses = &subpass;


            VkRenderPass renderPass;
            if (vkCreateRenderPass(
                device.LogicalDevice,
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
                _device.LogicalDevice,
                _renderPass,
                null
            );
        }
    }
}