using System;
using Vulkan;
using static Vulkan.VulkanNative;

namespace Tortuga.Graphics.API
{
    internal class Pipeline
    {
        VkPipelineLayout _layout;
        VkPipeline _pipeline;

        public unsafe Pipeline(DescriptorSetLayout[] layouts, RenderPass renderPass, Shader vertex, Shader fragment)
        {
            var bindingDescriptions = VertexLayoutBuilder.BindingDescriptions;
            var attributeDescriptions = VertexLayoutBuilder.AttributeDescriptions;

            var vertexInputInfo = VkPipelineVertexInputStateCreateInfo.New();
            vertexInputInfo.vertexBindingDescriptionCount = bindingDescriptions.Count;
            vertexInputInfo.pVertexBindingDescriptions = (VkVertexInputBindingDescription*)bindingDescriptions.Data.ToPointer();
            vertexInputInfo.vertexAttributeDescriptionCount = attributeDescriptions.Count;
            vertexInputInfo.pVertexAttributeDescriptions = (VkVertexInputAttributeDescription*)attributeDescriptions.Data.ToPointer();

            var inputAssemble = VkPipelineInputAssemblyStateCreateInfo.New();
            inputAssemble.topology = VkPrimitiveTopology.TriangleList;
            inputAssemble.primitiveRestartEnable = VkBool32.False;

            var viewport = new VkViewport(); //dynamic
            var scissor = new VkRect2D(); //dynamic

            var viewportState = VkPipelineViewportStateCreateInfo.New();
            viewportState.viewportCount = 1;
            viewportState.pViewports = &viewport;
            viewportState.scissorCount = 1;
            viewportState.pScissors = &scissor;

            var rasterizer = VkPipelineRasterizationStateCreateInfo.New();
            rasterizer.depthClampEnable = VkBool32.False;
            rasterizer.rasterizerDiscardEnable = VkBool32.False;
            rasterizer.polygonMode = VkPolygonMode.Fill;
            rasterizer.lineWidth = 1.0f;
            rasterizer.cullMode = VkCullModeFlags.Back;
            rasterizer.frontFace = VkFrontFace.CounterClockwise;
            rasterizer.depthBiasEnable = VkBool32.False;
            rasterizer.depthBiasConstantFactor = 0.0f;
            rasterizer.depthBiasClamp = 0.0f;
            rasterizer.depthBiasSlopeFactor = 0.0f;

            var multisampling = VkPipelineMultisampleStateCreateInfo.New();
            multisampling.sampleShadingEnable = VkBool32.False;
            multisampling.rasterizationSamples = VkSampleCountFlags.Count1;
            multisampling.minSampleShading = 1.0f;
            multisampling.pSampleMask = null;
            multisampling.alphaToCoverageEnable = VkBool32.False;
            multisampling.alphaToOneEnable = VkBool32.False;

            var colorBlendAttachment = new VkPipelineColorBlendAttachmentState()
            {
                colorWriteMask = VkColorComponentFlags.R | VkColorComponentFlags.G | VkColorComponentFlags.B | VkColorComponentFlags.A,
                blendEnable = VkBool32.False,
                srcColorBlendFactor = VkBlendFactor.SrcAlpha,
                dstColorBlendFactor = VkBlendFactor.OneMinusSrcAlpha,
                colorBlendOp = VkBlendOp.Add,
                srcAlphaBlendFactor = VkBlendFactor.One,
                dstAlphaBlendFactor = VkBlendFactor.Zero,
                alphaBlendOp = VkBlendOp.Add
            };

            var depthStencil = VkPipelineDepthStencilStateCreateInfo.New();
            depthStencil.depthTestEnable = VkBool32.True;
            depthStencil.depthWriteEnable = VkBool32.True;
            depthStencil.depthCompareOp = VkCompareOp.Less;
            depthStencil.depthBoundsTestEnable = VkBool32.False;
            depthStencil.minDepthBounds = 0.0f;
            depthStencil.maxDepthBounds = 1.0f;
            depthStencil.stencilTestEnable = VkBool32.False;
            depthStencil.front = new VkStencilOpState();
            depthStencil.back = new VkStencilOpState();

            var colorBlending = VkPipelineColorBlendStateCreateInfo.New();
            colorBlending.logicOpEnable = VkBool32.False;
            colorBlending.logicOp = VkLogicOp.Copy;
            colorBlending.attachmentCount = 1;
            colorBlending.pAttachments = &colorBlendAttachment;
            colorBlending.blendConstants_0 = 0.0f;
            colorBlending.blendConstants_1 = 0.0f;
            colorBlending.blendConstants_2 = 0.0f;
            colorBlending.blendConstants_3 = 0.0f;

            var dynamicStates = new NativeList<VkDynamicState>();
            dynamicStates.Add(VkDynamicState.Viewport);
            dynamicStates.Add(VkDynamicState.Scissor);
            dynamicStates.Add(VkDynamicState.LineWidth);

            var dynamicState = VkPipelineDynamicStateCreateInfo.New();
            dynamicState.dynamicStateCount = dynamicStates.Count;
            dynamicState.pDynamicStates = (VkDynamicState*)dynamicStates.Data.ToPointer();

            var setLayouts = new NativeList<VkDescriptorSetLayout>();
            foreach (var l in layouts)
                setLayouts.Add(l.Handle);
            var pipelineLayoutInfo = VkPipelineLayoutCreateInfo.New();
            pipelineLayoutInfo.setLayoutCount = setLayouts.Count;
            pipelineLayoutInfo.pSetLayouts = (VkDescriptorSetLayout*)setLayouts.Data.ToPointer();

            VkPipelineLayout pipelineLayout;
            if (vkCreatePipelineLayout(
                Engine.Instance.MainDevice.LogicalDevice,
                &pipelineLayoutInfo,
                null,
                &pipelineLayout
            ) != VkResult.Success)
                throw new Exception("failed to create pipeline layout");
            _layout = pipelineLayout;

            var shaderInfo = new NativeList<VkPipelineShaderStageCreateInfo>();
            var startFuncName = new FixedUtf8String("main");
            shaderInfo.Add(new VkPipelineShaderStageCreateInfo
            {
                sType = VkStructureType.PipelineShaderStageCreateInfo,
                module = vertex.Handle,
                stage = VkShaderStageFlags.Vertex,
                pName = startFuncName
            });
            shaderInfo.Add(new VkPipelineShaderStageCreateInfo
            {
                sType = VkStructureType.PipelineShaderStageCreateInfo,
                module = fragment.Handle,
                stage = VkShaderStageFlags.Fragment,
                pName = startFuncName
            });

            var pipelineInfo = VkGraphicsPipelineCreateInfo.New();
            pipelineInfo.stageCount = shaderInfo.Count;
            pipelineInfo.pStages = (VkPipelineShaderStageCreateInfo*)shaderInfo.Data.ToPointer();
            pipelineInfo.pVertexInputState = &vertexInputInfo;
            pipelineInfo.pInputAssemblyState = &inputAssemble;
            pipelineInfo.pViewportState = &viewportState;
            pipelineInfo.pRasterizationState = &rasterizer;
            pipelineInfo.pMultisampleState = &multisampling;
            pipelineInfo.pDepthStencilState = &depthStencil;
            pipelineInfo.pColorBlendState = &colorBlending;
            pipelineInfo.pDynamicState = &dynamicState;
            pipelineInfo.layout = _layout;
            pipelineInfo.renderPass = renderPass.Handle;
            pipelineInfo.subpass = 0;
            pipelineInfo.basePipelineHandle = VkPipeline.Null;
            pipelineInfo.basePipelineIndex = -1;

            VkPipeline pipeline;
            if (vkCreateGraphicsPipelines(
                Engine.Instance.MainDevice.LogicalDevice,
                VkPipelineCache.Null,
                1,
                &pipelineInfo,
                null,
                &pipeline
            ) != VkResult.Success)
                throw new Exception("failed to create graphics pipeline");
            _pipeline = pipeline;
        }
        unsafe ~Pipeline()
        {
            vkDestroyPipeline(
                Engine.Instance.MainDevice.LogicalDevice,
                _pipeline,
                null
            );
            vkDestroyPipelineLayout(
                Engine.Instance.MainDevice.LogicalDevice,
                _layout,
                null
            );
        }
    }
}