using System;
using Vulkan;
using Tortuga.Utils;
using static Vulkan.VulkanNative;

namespace Tortuga.Graphics.API
{
    internal class Pipeline
    {
        public VkPipeline Handle => _pipeline;
        public VkPipelineLayout Layout => _layout;
        public Device DeviceUsed => _device;
        public RenderPass RenderPassUsed => _renderPass;

        private VkPipelineLayout _layout;
        private VkPipeline _pipeline;
        private Device _device;
        private RenderPass _renderPass;

        private unsafe void SetupPipelineLayout(DescriptorSetLayout[] layouts)
        {
            var setLayouts = new NativeList<VkDescriptorSetLayout>();
            foreach (var l in layouts)
                setLayouts.Add(l.Handle);
            var pipelineLayoutInfo = VkPipelineLayoutCreateInfo.New();
            pipelineLayoutInfo.setLayoutCount = setLayouts.Count;
            pipelineLayoutInfo.pSetLayouts = (VkDescriptorSetLayout*)setLayouts.Data.ToPointer();

            VkPipelineLayout pipelineLayout;
            if (vkCreatePipelineLayout(
                _device.LogicalDevice,
                &pipelineLayoutInfo,
                null,
                &pipelineLayout
            ) != VkResult.Success)
                throw new Exception("failed to create pipeline layout");
            _layout = pipelineLayout;
        }

        private VkShaderStageFlags ShaderTypeToVulkanFlags(Shader.ShaderType type)
        {
            switch(type)
            {
                case Shader.ShaderType.Compute:
                    return VkShaderStageFlags.Compute;
                case Shader.ShaderType.Fragment:
                    return VkShaderStageFlags.Fragment;
                case Shader.ShaderType.Geometry:
                    return VkShaderStageFlags.Geometry;
                case Shader.ShaderType.Vertex:
                    return VkShaderStageFlags.Vertex;
                case Shader.ShaderType.TessellationControl:
                    return VkShaderStageFlags.TessellationControl;
                case Shader.ShaderType.TessellationEvaluation:
                    return VkShaderStageFlags.TessellationEvaluation;
            }
            throw new Exception("invalid shader type provided");
        }

        /// <summary>
        /// create graphics pipeline
        /// </summary>
        public unsafe Pipeline(
            RenderPass renderPass,
            DescriptorSetLayout[] layouts,
            Shader vertex,
            Shader fragment,
            PipelineInputBuilder pipelineInputBuilder
        )
        {
            _renderPass = renderPass;
            _device = renderPass.DeviceInUse;
            foreach (var layout in layouts)
            {
                if (layout.DeviceUsed != _device)
                    throw new Exception("The descriptor set layout provided belongs to a different device than the render pass");
            }

            var bindingDescriptions = pipelineInputBuilder.BindingDescriptions;
            var attributeDescriptions = pipelineInputBuilder.AttributeDescriptions;

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
            rasterizer.frontFace = VkFrontFace.Clockwise;
            rasterizer.depthBiasEnable = VkBool32.False;

            var multisampling = VkPipelineMultisampleStateCreateInfo.New();
            multisampling.sampleShadingEnable = VkBool32.False;
            multisampling.rasterizationSamples = VkSampleCountFlags.Count1;
            multisampling.minSampleShading = 1.0f;
            multisampling.pSampleMask = null;
            multisampling.alphaToCoverageEnable = VkBool32.False;
            multisampling.alphaToOneEnable = VkBool32.False;

            var colorBlendAttachments = new Utils.NativeList<VkPipelineColorBlendAttachmentState>();
            foreach (var attachment in renderPass.ColorAttachments)
            {
                colorBlendAttachments.Add(new VkPipelineColorBlendAttachmentState()
                {
                    colorWriteMask = VkColorComponentFlags.R | VkColorComponentFlags.G | VkColorComponentFlags.B | VkColorComponentFlags.A,
                    blendEnable = VkBool32.True,
                    srcColorBlendFactor = VkBlendFactor.SrcAlpha,
                    dstColorBlendFactor = VkBlendFactor.OneMinusSrcAlpha,
                    colorBlendOp = VkBlendOp.Add,
                    srcAlphaBlendFactor = VkBlendFactor.One,
                    dstAlphaBlendFactor = VkBlendFactor.Zero,
                    alphaBlendOp = VkBlendOp.Add
                });
            }

            var depthStencil = VkPipelineDepthStencilStateCreateInfo.New();
            depthStencil.depthTestEnable = VkBool32.True;
            depthStencil.depthWriteEnable = VkBool32.True;
            depthStencil.depthCompareOp = VkCompareOp.LessOrEqual;
            depthStencil.depthBoundsTestEnable = VkBool32.False;
            depthStencil.minDepthBounds = 0.0f;
            depthStencil.maxDepthBounds = 1.0f;
            depthStencil.stencilTestEnable = VkBool32.False;
            depthStencil.front = new VkStencilOpState();
            depthStencil.back = new VkStencilOpState();

            var colorBlending = VkPipelineColorBlendStateCreateInfo.New();
            colorBlending.logicOpEnable = VkBool32.False;
            colorBlending.logicOp = VkLogicOp.Copy;
            colorBlending.attachmentCount = colorBlendAttachments.Count;
            colorBlending.pAttachments = (VkPipelineColorBlendAttachmentState*)colorBlendAttachments.Data.ToPointer();
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

            this.SetupPipelineLayout(layouts);

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
            pipelineInfo.renderPass = _renderPass.Handle;
            pipelineInfo.subpass = 0;
            pipelineInfo.basePipelineHandle = VkPipeline.Null;
            pipelineInfo.basePipelineIndex = -1;

            VkPipeline pipeline;
            if (vkCreateGraphicsPipelines(
                _device.LogicalDevice,
                VkPipelineCache.Null,
                1,
                &pipelineInfo,
                null,
                &pipeline
            ) != VkResult.Success)
                throw new Exception("failed to create graphics pipeline");
            _pipeline = pipeline;
        }

        /// <summary>
        /// create compute pipeline
        /// </summary>
        public unsafe Pipeline(Shader shader, DescriptorSetLayout[] layouts)
        {
            _device = shader.DeviceUsed;
            this.SetupPipelineLayout(layouts);

            //specialization
            var specializationEntry = new Utils.NativeList<VkSpecializationMapEntry>();
            var specializationData = new Utils.NativeList<byte>();
            uint fullSize = 0;
            foreach (var specEntry in shader.Specializations)
            {
                specializationEntry.Add(new VkSpecializationMapEntry()
                {
                    constantID = specEntry.Identifier,
                    offset = fullSize,
                    size = (UIntPtr)specEntry.Size
                });
                foreach(var b in specEntry.Data)
                    specializationData.Add(b);
                fullSize += specEntry.Size;
            }
            var specialization = new VkSpecializationInfo()
            {
                mapEntryCount = specializationEntry.Count,
                pMapEntries = (VkSpecializationMapEntry*)specializationEntry.Data.ToPointer(),
                dataSize = (UIntPtr)fullSize,
                pData = specializationData.Data.ToPointer()
            };

            //setup shader
            var startFuncName = new FixedUtf8String("main");
            var stage = VkPipelineShaderStageCreateInfo.New();
            stage.stage = ShaderTypeToVulkanFlags(shader.Type);
            stage.module = shader.Handle;
            stage.pName = startFuncName;
            stage.pSpecializationInfo = &specialization;

            var createInfo = VkComputePipelineCreateInfo.New();
            createInfo.layout = _layout;
            createInfo.stage = stage;

            VkPipeline pipeline;
            if (vkCreateComputePipelines(
                _device.LogicalDevice,
                VkPipelineCache.Null,
                1,
                &createInfo,
                null,
                &pipeline
            ) != VkResult.Success)
                throw new Exception("failed to create compute pipeline");
            _pipeline = pipeline;
        }

        unsafe ~Pipeline()
        {
            vkDestroyPipeline(
                _device.LogicalDevice,
                _pipeline,
                null
            );
            vkDestroyPipelineLayout(
                _device.LogicalDevice,
                _layout,
                null
            );
        }
    }
}