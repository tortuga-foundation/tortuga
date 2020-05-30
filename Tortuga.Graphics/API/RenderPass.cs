using System;
using Vulkan;
using Tortuga.Utils;
using static Vulkan.VulkanNative;

namespace Tortuga.Graphics.API
{
    internal class RenderPass
    {
        public class CreateInfo
        {
            /// <summary>
            /// Should clear the image on begin render pass
            /// </summary>
            public bool Clear;
            /// <summary>
            /// Should store the image on end render pass
            /// </summary>
            public bool Store;

            public CreateInfo(bool clear = true, bool store = true)
            {
                this.Clear = clear;
                this.Store = store;
            }
        }

        internal const VkFormat DEFAULT_COLOR_FORMAT = VkFormat.R32g32b32a32Sfloat;
        internal const VkFormat DEFAULT_DEPTH_FORMAT = VkFormat.D32Sfloat;

        public VkRenderPass Handle => _renderPass;
        internal Device DeviceInUse => _device;

        private VkRenderPass _renderPass;
        private Device _device;

        public unsafe RenderPass(Device device, CreateInfo[] colorAttachments, CreateInfo depthAttachment = null)
        {
            //validate
            if (colorAttachments.Length < 1)
                throw new InvalidOperationException("you must have atleast 1 color attachment");

            _device = device;
            
            var attachmentDescriptions = new NativeList<VkAttachmentDescription>();
            foreach (var attachment in colorAttachments)
            {
                attachmentDescriptions.Add(new VkAttachmentDescription
                {
                    format = DEFAULT_COLOR_FORMAT,
                    samples = VkSampleCountFlags.Count1,
                    loadOp = attachment.Clear ? VkAttachmentLoadOp.Clear : VkAttachmentLoadOp.DontCare,
                    storeOp = attachment.Store ? VkAttachmentStoreOp.Store : VkAttachmentStoreOp.DontCare,
                    stencilLoadOp = VkAttachmentLoadOp.DontCare,
                    stencilStoreOp = VkAttachmentStoreOp.DontCare,
                    initialLayout = VkImageLayout.Undefined,
                    finalLayout = VkImageLayout.ColorAttachmentOptimal
                });
            }
            if (depthAttachment != null)
            {
                attachmentDescriptions.Add(new VkAttachmentDescription
                {
                    format = DEFAULT_DEPTH_FORMAT,
                    samples = VkSampleCountFlags.Count1,
                    loadOp = depthAttachment.Clear ? VkAttachmentLoadOp.Clear : VkAttachmentLoadOp.DontCare,
                    storeOp = depthAttachment.Store ? VkAttachmentStoreOp.Store : VkAttachmentStoreOp.DontCare,
                    stencilLoadOp = VkAttachmentLoadOp.DontCare,
                    stencilStoreOp = VkAttachmentStoreOp.DontCare,
                    initialLayout = VkImageLayout.Undefined,
                    finalLayout = VkImageLayout.DepthStencilAttachmentOptimal
                });
            }

            var colorAttachmentRef = new Utils.NativeList<VkAttachmentReference>();
            for (uint i = 0; i < colorAttachments.Length; i++)
            {
                colorAttachmentRef.Add(new VkAttachmentReference()
                {
                    attachment = i,
                    layout = VkImageLayout.ColorAttachmentOptimal
                });
            }
            var depthAttachmentRef = new VkAttachmentReference
            {
                attachment = Convert.ToUInt32(colorAttachments.Length),
                layout = VkImageLayout.DepthStencilAttachmentOptimal
            };
            var subpass = new VkSubpassDescription
            {
                pipelineBindPoint = VkPipelineBindPoint.Graphics,
                colorAttachmentCount = colorAttachmentRef.Count,
                pColorAttachments = (VkAttachmentReference*)colorAttachmentRef.Data.ToPointer(),
                pDepthStencilAttachment = depthAttachment != null ? &depthAttachmentRef : null
            };

            var renderPassInfo = VkRenderPassCreateInfo.New();
            renderPassInfo.attachmentCount = attachmentDescriptions.Count;
            renderPassInfo.pAttachments = (VkAttachmentDescription*)attachmentDescriptions.Data.ToPointer();
            renderPassInfo.subpassCount = 1;
            renderPassInfo.pSubpasses = &subpass;


            VkRenderPass renderPass;
            if (vkCreateRenderPass(
                _device.LogicalDevice,
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