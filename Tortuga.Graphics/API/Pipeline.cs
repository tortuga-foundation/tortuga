#pragma warning disable CS1591
using System;
using System.Collections.Generic;
using Tortuga.Utils;
using Vulkan;

namespace Tortuga.Graphics.API
{
    public class Pipeline
    {
        public Device Device => _device;
        public RenderPass RenderPass => _renderPass;
        public VkPipelineLayout Layout => _layout;
        public VkPipeline Handle => _handle;

        protected Device _device;
        protected RenderPass _renderPass;
        protected VkPipelineLayout _layout;
        protected VkPipeline _handle;

        public Pipeline(
            Device device,
            RenderPass renderPass,
            VkPipelineLayout layout,
            VkPipeline handle
        )
        {
            _device = device;
            _renderPass = renderPass;
            _layout = layout;
            _handle = handle;
        }

        unsafe ~Pipeline()
        {
            if (_handle != VkPipeline.Null)
            {
                VulkanNative.vkDestroyPipeline(
                    _device.Handle,
                    _handle,
                    null
                );
                _handle = VkPipeline.Null;
            }
            if (_layout != VkPipelineLayout.Null)
            {
                VulkanNative.vkDestroyPipelineLayout(
                    _device.Handle,
                    _layout,
                    null
                );
                _layout = VkPipelineLayout.Null;
            }
        }
    }

    internal class GraphicsPipeline : Pipeline
    {
        public unsafe GraphicsPipeline(
            Device device,
            RenderPass renderPass,
            List<DescriptorLayout> layouts,
            List<ShaderModule> shaders,
            PipelineInputBuilder pipelineInputBuilder,
            uint subPass = 0,
            VkPrimitiveTopology topology = VkPrimitiveTopology.TriangleList
        ) : base(device, renderPass, VkPipelineLayout.Null, VkPipeline.Null)
        {
            #region Pipeline Layout

            var setLayout = new NativeList<VkDescriptorSetLayout>();
            foreach (var layout in layouts)
                setLayout.Add(layout.Handle);

            var pipelineLayoutInfo = new VkPipelineLayoutCreateInfo
            {
                sType = VkStructureType.PipelineLayoutCreateInfo,
                setLayoutCount = setLayout.Count,
                pSetLayouts = (VkDescriptorSetLayout*)setLayout.Data.ToPointer()
            };

            VkPipelineLayout pipelineLayout;
            if (VulkanNative.vkCreatePipelineLayout(
                device.Handle,
                &pipelineLayoutInfo,
                null,
                &pipelineLayout
            ) != VkResult.Success)
                throw new Exception("failed to create pipeline layout");
            _layout = pipelineLayout;

            #endregion

            var bindingDescriptions = pipelineInputBuilder.BindingDescriptions;
            var attributeDescriptions = pipelineInputBuilder.AttributeDescriptions;

            var vertexInputInfo = new VkPipelineVertexInputStateCreateInfo
            {
                sType = VkStructureType.PipelineVertexInputStateCreateInfo,
                vertexBindingDescriptionCount = bindingDescriptions.Count,
                pVertexBindingDescriptions = (VkVertexInputBindingDescription*)bindingDescriptions.Data.ToPointer(),
                vertexAttributeDescriptionCount = attributeDescriptions.Count,
                pVertexAttributeDescriptions = (VkVertexInputAttributeDescription*)attributeDescriptions.Data.ToPointer()
            };

            var inputAssemble = new VkPipelineInputAssemblyStateCreateInfo
            {
                sType = VkStructureType.PipelineInputAssemblyStateCreateInfo,
                topology = topology,
                primitiveRestartEnable = VkBool32.False
            };

            var viewport = new VkViewport(); //dynamic (ignored)
            var scissor = new VkRect2D(); //dynamic (ignored)

            var viewportState = new VkPipelineViewportStateCreateInfo
            {
                sType = VkStructureType.PipelineViewportStateCreateInfo,
                viewportCount = 1,
                pViewports = &viewport,
                scissorCount = 1,
                pScissors = &scissor
            };

            var rasterizer = new VkPipelineRasterizationStateCreateInfo
            {
                sType = VkStructureType.PipelineRasterizationStateCreateInfo,
                depthClampEnable = false,
                rasterizerDiscardEnable = VkBool32.False,
                polygonMode = VkPolygonMode.Fill,
                lineWidth = 1.0f,
                cullMode = VkCullModeFlags.Back,
                frontFace = VkFrontFace.CounterClockwise,
                depthBiasEnable = VkBool32.False
            };

            var multisampling = new VkPipelineMultisampleStateCreateInfo
            {
                sType = VkStructureType.PipelineMultisampleStateCreateInfo,
                sampleShadingEnable = VkBool32.False,
                rasterizationSamples = VkSampleCountFlags.Count1,
                minSampleShading = 1.0f,
                pSampleMask = null,
                alphaToCoverageEnable = VkBool32.False,
                alphaToOneEnable = VkBool32.False
            };

            bool hasDepthAttachment = false;
            var colorBlendAttachments = new NativeList<VkPipelineColorBlendAttachmentState>();
            foreach (var attachment in renderPass.Attachments)
            {
                if ((attachment.ImageUsageFlags & VkImageUsageFlags.DepthStencilAttachment) != 0)
                {
                    hasDepthAttachment = true;
                    continue;
                }

                colorBlendAttachments.Add(new VkPipelineColorBlendAttachmentState
                {
                    colorWriteMask = (
                        VkColorComponentFlags.R |
                        VkColorComponentFlags.G |
                        VkColorComponentFlags.B |
                        VkColorComponentFlags.A
                    ),
                    blendEnable = VkBool32.True,
                    srcColorBlendFactor = VkBlendFactor.SrcAlpha,
                    dstColorBlendFactor = VkBlendFactor.OneMinusSrc1Alpha,
                    colorBlendOp = VkBlendOp.Add,
                    srcAlphaBlendFactor = VkBlendFactor.One,
                    dstAlphaBlendFactor = VkBlendFactor.Zero,
                    alphaBlendOp = VkBlendOp.Add
                });
            }

            var depthStencil = new VkPipelineDepthStencilStateCreateInfo
            {
                sType = VkStructureType.PipelineDepthStencilStateCreateInfo,
                depthTestEnable = VkBool32.True,
                depthWriteEnable = VkBool32.True,
                depthCompareOp = VkCompareOp.LessOrEqual,
                depthBoundsTestEnable = VkBool32.False,
                minDepthBounds = 0.0f,
                maxDepthBounds = 1.0f,
                stencilTestEnable = VkBool32.False,
                front = new VkStencilOpState(),
                back = new VkStencilOpState()
            };

            var colorBlending = new VkPipelineColorBlendStateCreateInfo
            {
                sType = VkStructureType.PipelineColorBlendStateCreateInfo,
                logicOpEnable = VkBool32.False,
                logicOp = VkLogicOp.Copy,
                attachmentCount = colorBlendAttachments.Count,
                pAttachments = (VkPipelineColorBlendAttachmentState*)colorBlendAttachments.Data.ToPointer(),
                blendConstants_0 = 0.0f,
                blendConstants_1 = 0.0f,
                blendConstants_2 = 0.0f,
                blendConstants_3 = 0.0f
            };

            var dynamicStates = new NativeList<VkDynamicState>();
            dynamicStates.Add(VkDynamicState.Viewport);
            dynamicStates.Add(VkDynamicState.Scissor);
            dynamicStates.Add(VkDynamicState.LineWidth);

            var dynamicStateInfo = new VkPipelineDynamicStateCreateInfo
            {
                sType = VkStructureType.PipelineDynamicStateCreateInfo,
                dynamicStateCount = dynamicStates.Count,
                pDynamicStates = (VkDynamicState*)dynamicStates.Data.ToPointer()
            };

            var shaderInfo = new NativeList<VkPipelineShaderStageCreateInfo>();
            foreach (var shader in shaders)
            {
                shaderInfo.Add(new VkPipelineShaderStageCreateInfo
                {
                    sType = VkStructureType.PipelineShaderStageCreateInfo,
                    module = shader.Handle,
                    stage = (VkShaderStageFlags)shader.Type,
                    pName = GraphicsApiConstants.MAIN
                });
            }

            var pipelineInfo = new VkGraphicsPipelineCreateInfo
            {
                sType = VkStructureType.GraphicsPipelineCreateInfo,
                stageCount = shaderInfo.Count,
                pStages = (VkPipelineShaderStageCreateInfo*)shaderInfo.Data.ToPointer(),
                pVertexInputState = &vertexInputInfo,
                pInputAssemblyState = &inputAssemble,
                pViewportState = &viewportState,
                pRasterizationState = &rasterizer,
                pMultisampleState = &multisampling,
                pDepthStencilState = hasDepthAttachment ? &depthStencil : null,
                pColorBlendState = &colorBlending,
                pDynamicState = &dynamicStateInfo,
                layout = _layout,
                renderPass = _renderPass.Handle,
                subpass = subPass,
                basePipelineHandle = VkPipeline.Null,
                basePipelineIndex = -1
            };

            VkPipeline pipeline;
            if (VulkanNative.vkCreateGraphicsPipelines(
                _device.Handle,
                VkPipelineCache.Null,
                1,
                &pipelineInfo,
                null,
                &pipeline
            ) != VkResult.Success)
                throw new Exception("faield to create graphics pipeline");
            _handle = pipeline;
        }
    }
}