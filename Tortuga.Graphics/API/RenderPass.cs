#pragma warning disable CS1591
using System;
using System.Collections.Generic;
using Tortuga.Utils;
using Vulkan;

namespace Tortuga.Graphics.API
{
    public struct RenderPassAttachment
    {
        public VkFormat Format;

        public bool Clear;
        public bool Store;
        public VkImageLayout InitialLayout;
        public VkImageLayout FinalLayout;
        public VkImageUsageFlags ImageUsageFlags;
        public VkImageAspectFlags ImageAspectFlags;

        public static RenderPassAttachment Default
        => new RenderPassAttachment()
        {
            Format = VkFormat.R32g32b32a32Sfloat,
            Clear = true,
            Store = true,
            InitialLayout = VkImageLayout.Undefined,
            FinalLayout = VkImageLayout.ColorAttachmentOptimal,
            ImageUsageFlags = (
                VkImageUsageFlags.TransferDst |
                VkImageUsageFlags.TransferSrc |
                VkImageUsageFlags.ColorAttachment |
                VkImageUsageFlags.Sampled
            ),
            ImageAspectFlags = VkImageAspectFlags.Color
        };

        public static RenderPassAttachment DefaultDepth
        => new RenderPassAttachment() //depth
        {
            Format = VkFormat.D32Sfloat,
            Clear = true,
            Store = true,
            InitialLayout = VkImageLayout.Undefined,
            FinalLayout = VkImageLayout.DepthStencilAttachmentOptimal,
            ImageUsageFlags = (
                VkImageUsageFlags.TransferSrc |
                VkImageUsageFlags.TransferDst |
                VkImageUsageFlags.DepthStencilAttachment
            ),
            ImageAspectFlags = VkImageAspectFlags.Depth
        };
    }

    public struct RenderPassSubPass
    {
        public VkPipelineBindPoint BindPoint;
        public List<uint> ColorAttachments;
        public uint? DepthAttachments;
        public RenderPassSubPass(
            VkPipelineBindPoint bindPoint,
            List<uint> colorAttachments,
            uint? depthAttachments = null
        )
        {
            BindPoint = bindPoint;
            ColorAttachments = colorAttachments;
            DepthAttachments = depthAttachments;
        }
    }

    public class RenderPass
    {
        public Device Device => _device;
        public VkRenderPass Handle => _handle;
        public List<RenderPassAttachment> Attachments => _attachments;
        private VkRenderPass _handle;
        private Device _device;
        private List<RenderPassAttachment> _attachments;
        private List<RenderPassSubPass> _subPassInfo;

        public unsafe RenderPass(
            Device device,
            List<RenderPassAttachment> attachments,
            List<RenderPassSubPass> subPasses
        )
        {
            // update depth format to one that this device supports
            var correctDepthFormat = device.FindDepthFormat;
            for (int i = 0; i < attachments.Count; i++)
            {
                if (attachments[i].Format == RenderPassAttachment.DefaultDepth.Format)
                {
                    attachments[i] = new RenderPassAttachment
                    {
                        Format = correctDepthFormat,
                        Clear = attachments[i].Clear,
                        Store = attachments[i].Store,
                        InitialLayout = attachments[i].InitialLayout,
                        FinalLayout = attachments[i].FinalLayout,
                        ImageUsageFlags = attachments[i].ImageUsageFlags,
                        ImageAspectFlags = attachments[i].ImageAspectFlags
                    };
                }
            }
            _device = device;
            _attachments = attachments;
            _subPassInfo = subPasses;
            var subPassCount = Convert.ToUInt32(subPasses.Count);

            //setup attachments
            var attachmentDescriptions = new NativeList<VkAttachmentDescription>();
            foreach (var attachment in attachments)
            {
                attachmentDescriptions.Add(new VkAttachmentDescription
                {
                    format = attachment.Format,
                    samples = VkSampleCountFlags.Count1,
                    loadOp = (
                        attachment.Clear ?
                        VkAttachmentLoadOp.Clear :
                        VkAttachmentLoadOp.Load
                    ),
                    storeOp = (
                        attachment.Store ?
                        VkAttachmentStoreOp.Store :
                        VkAttachmentStoreOp.DontCare
                    ),
                    stencilLoadOp = VkAttachmentLoadOp.DontCare,
                    stencilStoreOp = VkAttachmentStoreOp.DontCare,
                    initialLayout = attachment.InitialLayout,
                    finalLayout = attachment.FinalLayout
                });
            }

            //setup subpasses
            var subPassInfo = new NativeList<VkSubpassDescription>();
            var dependencies = new NativeList<VkSubpassDependency>();
            var colorAttachmentRefs = new List<NativeList<VkAttachmentReference>>();
            var depthAttachmentRefs = new List<VkAttachmentReference>();
            foreach (var subpass in subPasses)
            {
                var colorAttachmentRef = new NativeList<VkAttachmentReference>();
                for (int i = 0; i < subpass.ColorAttachments.Count; i++)
                {
                    var colorAttachmentIndex = subpass.ColorAttachments[i];
                    colorAttachmentRef.Add(new VkAttachmentReference
                    {
                        attachment = colorAttachmentIndex,
                        layout = attachments[Convert.ToInt32(colorAttachmentIndex)].FinalLayout
                    });
                }
                colorAttachmentRefs.Add(colorAttachmentRef);
                var depthAttachment = new NativeList<VkAttachmentReference>();
                if (subpass.DepthAttachments != null)
                {
                    var depthAttachmentRef = new VkAttachmentReference
                    {
                        attachment = (uint)subpass.DepthAttachments,
                        layout = attachments[Convert.ToInt32(subpass.DepthAttachments)].FinalLayout
                    };
                    depthAttachmentRefs.Add(depthAttachmentRef);
                    depthAttachment.Add(depthAttachmentRef);
                }
                subPassInfo.Add(new VkSubpassDescription
                {
                    pipelineBindPoint = subpass.BindPoint,
                    colorAttachmentCount = colorAttachmentRef.Count,
                    pColorAttachments = (VkAttachmentReference*)colorAttachmentRef.Data.ToPointer(),
                    pDepthStencilAttachment = (
                        depthAttachment.Count != 1 ?
                        null :
                        (VkAttachmentReference*)depthAttachment.Data.ToPointer()
                    )
                });
                dependencies.Add(new VkSubpassDependency
                {
                    srcSubpass = 0,
                    dstSubpass = 0,
                    srcStageMask = (
                        VkPipelineStageFlags.ColorAttachmentOutput |
                        VkPipelineStageFlags.EarlyFragmentTests
                    ),
                    srcAccessMask = 0,
                    dstStageMask = (
                        VkPipelineStageFlags.ColorAttachmentOutput |
                        VkPipelineStageFlags.EarlyFragmentTests
                    ),
                    dstAccessMask = subpass.DepthAttachments != null ? (
                        VkAccessFlags.ColorAttachmentWrite |
                        VkAccessFlags.DepthStencilAttachmentWrite
                    ) : VkAccessFlags.ColorAttachmentWrite
                });
            }

            var renderPassInfo = new VkRenderPassCreateInfo
            {
                sType = VkStructureType.RenderPassCreateInfo,
                attachmentCount = attachmentDescriptions.Count,
                pAttachments = (VkAttachmentDescription*)attachmentDescriptions.Data.ToPointer(),
                subpassCount = subPassCount,
                pSubpasses = (VkSubpassDescription*)subPassInfo.Data.ToPointer(),
                dependencyCount = dependencies.Count,
                pDependencies = (VkSubpassDependency*)dependencies.Data.ToPointer()
            };

            VkRenderPass renderPass;
            if (VulkanNative.vkCreateRenderPass(
                device.Handle,
                &renderPassInfo,
                null,
                &renderPass
            ) != VkResult.Success)
                throw new Exception("failed to create render pass");
            _handle = renderPass;
        }
        unsafe ~RenderPass()
        {
            if (_handle != VkRenderPass.Null)
            {
                VulkanNative.vkDestroyRenderPass(
                    _device.Handle,
                    _handle,
                    null
                );
                _handle = VkRenderPass.Null;
            }
        }
    }
}