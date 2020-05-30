using System;
using Vulkan;
using Tortuga.Utils;
using static Vulkan.VulkanNative;

namespace Tortuga.Graphics.API
{
    internal class RenderPass
    {
        public const VkFormat DEFAULT_COLOR_IMAGE_FORMAT = VkFormat.R8g8b8a8Unorm;
        public const VkFormat DEFAULT_DEPTH_IMAGE_FORMAT = VkFormat.R32Sfloat;
        public enum RenderPassAttachmentType
        {
            Image,
            Depth
        }

        public VkRenderPass Handle => _renderPass;
        public Device DeviceUsed => _device;
        public RenderPassAttachmentType[] AttachmentInfo => _attachmentInfo;

        private VkRenderPass _renderPass;
        private Device _device;
        private RenderPassAttachmentType[] _attachmentInfo;

        public unsafe RenderPass(
            Device device,
            RenderPassAttachmentType[] attachments
        )
        {
            //validate attachments provided
            bool foundDepth = false;
            for (int i = 0; i < attachments.Length; i++)
            {
                if (attachments[i] == RenderPassAttachmentType.Depth)
                {
                    if (foundDepth == false)
                        foundDepth = true;
                    else
                        throw new InvalidOperationException("there must be only one depth attachment");
                }
            }

            _device = device;
            _attachmentInfo = attachments;

            //setup attachment descriptions
            var attachmentDescriptions = new NativeList<VkAttachmentDescription>();
            var colorAttachmentRefs = new NativeList<VkAttachmentReference>();
            var depthAttachmentRefs = new VkAttachmentReference();
            for (int i = 0; i < attachments.Length; i++)
            {
                var format = VkFormat.Undefined;
                var layout = VkImageLayout.Undefined;
                var storeOp = VkAttachmentStoreOp.DontCare;
                var loadOp = VkAttachmentLoadOp.DontCare;
                var stencilLoadOp = VkAttachmentLoadOp.DontCare;
                var stencilStoreOp = VkAttachmentStoreOp.DontCare;
                switch (attachments[i])
                {
                    case RenderPassAttachmentType.Image:
                        format = DEFAULT_COLOR_IMAGE_FORMAT;
                        layout = VkImageLayout.ColorAttachmentOptimal;
                        storeOp = VkAttachmentStoreOp.Store;
                        loadOp = VkAttachmentLoadOp.Clear;
                        stencilLoadOp = VkAttachmentLoadOp.DontCare;
                        stencilStoreOp = VkAttachmentStoreOp.DontCare;
                        break;
                    case RenderPassAttachmentType.Depth:
                        format = DEFAULT_DEPTH_IMAGE_FORMAT;
                        layout = VkImageLayout.DepthStencilAttachmentOptimal;
                        storeOp = VkAttachmentStoreOp.DontCare;
                        loadOp = VkAttachmentLoadOp.DontCare;
                        stencilLoadOp = VkAttachmentLoadOp.Clear;
                        stencilStoreOp = VkAttachmentStoreOp.Store;
                        break;
                }
                //setup attachment descriptions
                attachmentDescriptions.Add(new VkAttachmentDescription
                {
                    format = format,
                    samples = VkSampleCountFlags.Count1,
                    loadOp = loadOp,
                    storeOp = storeOp,
                    stencilLoadOp = stencilLoadOp,
                    stencilStoreOp = stencilStoreOp,
                    initialLayout = VkImageLayout.Undefined,
                    finalLayout = layout
                });

                //setup attachment references
                var reference = new VkAttachmentReference()
                {
                    attachment = Convert.ToUInt32(i),
                    layout = layout
                };
                if (attachments[i] == RenderPassAttachmentType.Depth)
                    depthAttachmentRefs = reference;
                else
                    colorAttachmentRefs.Add(reference);
            }

            //create sub pass
            var subpass = new VkSubpassDescription
            {
                pipelineBindPoint = VkPipelineBindPoint.Graphics,
                colorAttachmentCount = colorAttachmentRefs.Count,
                pColorAttachments = (VkAttachmentReference*)colorAttachmentRefs.Data.ToPointer(),
                pDepthStencilAttachment = foundDepth ? &depthAttachmentRefs : null
            };

            var renderPassInfo = VkRenderPassCreateInfo.New();
            renderPassInfo.attachmentCount = attachmentDescriptions.Count;
            renderPassInfo.pAttachments = (VkAttachmentDescription*)attachmentDescriptions.Data.ToPointer();
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