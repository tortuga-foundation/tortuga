using System;
using System.Collections.Generic;
using Tortuga.Graphics.API;
using Tortuga.Utils.SDL2;
using Vulkan;

namespace Tortuga.Graphics
{
    /// <summary>
    /// This contains global objects used by Tortuga.Graphics
    /// </summary>
    public class GraphicsModule : Core.BaseModule
    {
        internal GraphicsService GraphicsService => _graphicsService;
        internal CommandBufferService CommandBufferService => _commandBufferService;
        internal Dictionary<string, DescriptorLayout> DescriptorLayouts => _descriptorLayouts;
        internal Dictionary<string, RenderPass> RenderPasses => _renderPasses;
        private GraphicsService _graphicsService;
        private CommandBufferService _commandBufferService;
        private Dictionary<string, DescriptorLayout> _descriptorLayouts;
        private Dictionary<string, RenderPass> _renderPasses;

        /// <summary>
        /// runs on engine close
        /// </summary>
        public override void Destroy() { }

        /// <summary>
        /// runs on engine start
        /// </summary>
        public override void Init()
        {
            _graphicsService = new GraphicsService();
            _commandBufferService = new CommandBufferService(_graphicsService.PrimaryDevice);
            _descriptorLayouts = new Dictionary<string, DescriptorLayout>();
            _renderPasses = new Dictionary<string, RenderPass>();
            InitDescriptorLayouts();
            InitRenderPasses();
            MaterialLoader.Init();
        }

        private void InitRenderPasses()
        {
            _renderPasses["_MRT"] = new RenderPass(
                _graphicsService.PrimaryDevice,
                new List<RenderPassAttachment>
                {
                    RenderPassAttachment.Default,//albedo
                    RenderPassAttachment.Default,//normal
                    RenderPassAttachment.Default,//position
                    RenderPassAttachment.Default,//detail
                    RenderPassAttachment.DefaultDepth
                },
                new List<RenderPassSubPass>
                {
                    new RenderPassSubPass
                    {
                        BindPoint = VkPipelineBindPoint.Graphics,
                        ColorAttachments = new List<uint>
                        {
                            0, 1, 2, 3
                        },
                        DepthAttachments = 4
                    }
                }
            );

            _renderPasses["_DEFFERED"] = new RenderPass(
                _graphicsService.PrimaryDevice,
                new List<RenderPassAttachment>
                {
                    RenderPassAttachment.Default,
                    RenderPassAttachment.DefaultDepth
                },
                new List<RenderPassSubPass>
                {
                    new RenderPassSubPass
                    {
                        BindPoint = VkPipelineBindPoint.Graphics,
                        ColorAttachments = new List<uint> { 0 },
                        DepthAttachments = 1
                    }
                }
            );

            _renderPasses["_LIGHT"] = new RenderPass(
                _graphicsService.PrimaryDevice,
                new List<RenderPassAttachment>
                {
                    RenderPassAttachment.Default,
                    RenderPassAttachment.DefaultDepth
                },
                new List<RenderPassSubPass>
                {
                    new RenderPassSubPass
                    {
                        BindPoint = VkPipelineBindPoint.Graphics,
                        ColorAttachments = new List<uint>{ 0 },
                        DepthAttachments = 1
                    }
                }
            );
        }

        private void InitDescriptorLayouts()
        {
            _descriptorLayouts["_PROJECTION"] = new DescriptorLayout(
                _graphicsService.PrimaryDevice,
                new List<DescriptorBindingInfo>
                {
                    new DescriptorBindingInfo
                    {
                        DescriptorType = VkDescriptorType.UniformBuffer,
                        ShaderStageFlags = VkShaderStageFlags.Vertex,
                        DescriptorCounts = 1,
                        Index = 0
                    }
                }
            );

            _descriptorLayouts["_VIEW"] = new DescriptorLayout(
                _graphicsService.PrimaryDevice,
                new List<DescriptorBindingInfo>
                {
                    new DescriptorBindingInfo
                    {
                        DescriptorType = VkDescriptorType.UniformBuffer,
                        ShaderStageFlags = VkShaderStageFlags.Vertex,
                        DescriptorCounts = 1,
                        Index = 0
                    }
                }
            );

            _descriptorLayouts["_MODEL"] = new DescriptorLayout(
                _graphicsService.PrimaryDevice,
                new List<DescriptorBindingInfo>
                {
                    new DescriptorBindingInfo
                    {
                        DescriptorType = VkDescriptorType.UniformBuffer,
                        ShaderStageFlags = VkShaderStageFlags.Vertex,
                        DescriptorCounts = 1,
                        Index = 0
                    }
                }
            );

            _descriptorLayouts["_MRT"] = new DescriptorLayout(
                _graphicsService.PrimaryDevice,
                new List<DescriptorBindingInfo>
                {
                    //albedo
                    new DescriptorBindingInfo
                    {
                        DescriptorType = VkDescriptorType.CombinedImageSampler,
                        ShaderStageFlags = VkShaderStageFlags.Fragment,
                        DescriptorCounts = 1,
                        Index = 0
                    },
                    //normal
                    new DescriptorBindingInfo
                    {
                        DescriptorType = VkDescriptorType.CombinedImageSampler,
                        ShaderStageFlags = VkShaderStageFlags.Fragment,
                        DescriptorCounts = 1,
                        Index = 1
                    },
                    //position
                    new DescriptorBindingInfo
                    {
                        DescriptorType = VkDescriptorType.CombinedImageSampler,
                        ShaderStageFlags = VkShaderStageFlags.Fragment,
                        DescriptorCounts = 1,
                        Index = 2
                    },
                    //detail
                    new DescriptorBindingInfo
                    {
                        DescriptorType = VkDescriptorType.CombinedImageSampler,
                        ShaderStageFlags = VkShaderStageFlags.Fragment,
                        DescriptorCounts = 1,
                        Index = 3
                    }
                }
            );

            _descriptorLayouts["_CAMERA"] = new DescriptorLayout(
                _graphicsService.PrimaryDevice,
                new List<DescriptorBindingInfo>
                {
                    new DescriptorBindingInfo
                    {
                        DescriptorType = VkDescriptorType.UniformBuffer,
                        ShaderStageFlags = VkShaderStageFlags.Fragment,
                        DescriptorCounts = 1,
                        Index = 0
                    }
                }
            );

            _descriptorLayouts["_LIGHT"] = new DescriptorLayout(
                _graphicsService.PrimaryDevice,
                new List<DescriptorBindingInfo>
                {
                    new DescriptorBindingInfo
                    {
                        DescriptorType = VkDescriptorType.UniformBuffer,
                        ShaderStageFlags = VkShaderStageFlags.Fragment,
                        DescriptorCounts = 1,
                        Index = 0
                    }
                }
            );
        }

        /// <summary>
        /// runs once per frame
        /// </summary>
        public override void Update()
        {
        }
    }
}