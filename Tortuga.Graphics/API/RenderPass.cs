using System;
using Vulkan;
using Tortuga.Utils;
using static Vulkan.VulkanNative;

namespace Tortuga.Graphics.API
{
    /// <summary>
    /// Vulkan render pass
    /// </summary>
    public class RenderPass
    {
        /// <summary>
        /// render pass create info
        /// </summary>
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
            internal VkImageLayout InitialLayout;
            internal VkImageLayout FinalLayout;

            /// <summary>
            /// constructor for create info
            /// </summary>
            /// <param name="clear">should clear image on render pass begin</param>
            /// <param name="store">should store image on render pass end</param>
            public CreateInfo(bool clear = true, bool store = true)
            {
                this.Clear = clear;
                this.Store = store;
                InitialLayout = VkImageLayout.Undefined;
                FinalLayout = VkImageLayout.ColorAttachmentOptimal;
            }
        }

        /// <summary>
        /// default render pass image color format
        /// </summary>
        public const VkFormat DEFAULT_COLOR_FORMAT = VkFormat.R32g32b32a32Sfloat;
        /// <summary>
        /// default render pass depth format
        /// </summary>
        public const VkFormat DEFAULT_DEPTH_FORMAT = VkFormat.D32Sfloat;

        /// <summary>
        /// vulkan render pass handle
        /// </summary>
        public VkRenderPass Handle => _renderPass;
        /// <summary>
        /// device being used for this render pass
        /// </summary>
        public Device DeviceInUse => _device;
        /// <summary>
        /// image color attachments used for this render pass
        /// </summary>
        public CreateInfo[] ColorAttachments => _colorAttachments;
        /// <summary>
        /// depth attachment used for this render pass (can be null if no depth attachment is used)
        /// </summary>
        public CreateInfo DepthAttachment => _depthAttachment;

        private VkRenderPass _renderPass;
        private Device _device;
        private CreateInfo[] _colorAttachments;
        private CreateInfo _depthAttachment;

        /// <summary>
        /// constructor for render pass
        /// </summary>
        /// <param name="device">vulkan device</param>
        /// <param name="colorAttachments">color attachments</param>
        /// <param name="depthAttachment">depth attachments</param>
        public unsafe RenderPass(Device device, CreateInfo[] colorAttachments, CreateInfo depthAttachment = null)
        {
            //validate
            if (colorAttachments.Length < 1)
                throw new InvalidOperationException("you must have atleast 1 color attachment");

            _device = device;
            _colorAttachments = colorAttachments;
            _depthAttachment = depthAttachment;

            var attachmentDescriptions = new NativeList<VkAttachmentDescription>();
            foreach (var attachment in colorAttachments)
            {
                attachmentDescriptions.Add(new VkAttachmentDescription
                {
                    format = DEFAULT_COLOR_FORMAT,
                    samples = VkSampleCountFlags.Count1,
                    loadOp = attachment.Clear ? VkAttachmentLoadOp.Clear : VkAttachmentLoadOp.Load,
                    storeOp = attachment.Store ? VkAttachmentStoreOp.Store : VkAttachmentStoreOp.DontCare,
                    stencilLoadOp = VkAttachmentLoadOp.DontCare,
                    stencilStoreOp = VkAttachmentStoreOp.DontCare,
                    initialLayout = attachment.InitialLayout,
                    finalLayout = attachment.FinalLayout
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

        /// <summary>
        /// de-constructor
        /// </summary>
        ~RenderPass()
        {
            Dispose();
        }

        /// <summary>
        /// destroy's render pass
        /// </summary>
        public unsafe void Dispose()
        {
            if (_renderPass != VkRenderPass.Null)
            {
                vkDestroyRenderPass(
                    _device.LogicalDevice,
                    _renderPass,
                    null
                );
                _renderPass = VkRenderPass.Null;
            }
        }
    }
}