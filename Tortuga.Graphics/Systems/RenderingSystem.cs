#pragma warning disable 1591
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Vulkan;

namespace Tortuga.Graphics
{
    public class RenderingSystem : Core.BaseSystem
    {
        private API.CommandPool _renderCommandPool;
        private API.CommandPool.Command _renderCommand;
        private API.Fence _renderFence;
        private API.Shader _shader;
        private API.Pipeline _pipeline;

        public override void OnDisable()
        {
        }

        public override void OnEnable()
        {
            _renderCommandPool = new API.CommandPool(API.Handler.MainDevice, API.Handler.MainDevice.GraphicsQueueFamily);
            _renderCommand = _renderCommandPool.AllocateCommands()[0];
            _renderFence = new API.Fence(API.Handler.MainDevice);

            _shader = new API.Shader(API.Handler.MainDevice, "Assets/Shaders/ray.comp");
            _pipeline = new API.Pipeline(
                _shader,
                new API.DescriptorSetLayout[]
                {
                    Engine.Instance.GetModule<GraphicsModule>().RenderDescriptorLayout
                }
            );
        }

        public override Task EarlyUpdate()
        {
            return Task.Run(() =>
            {
                var cameras = MyScene.GetComponents<Camera>();
                foreach (var camera in cameras)
                {
                    if (camera.RenderToWindow != null)
                        camera.RenderToWindow.AcquireSwapchainImage();
                }
            });
        }

        public override Task LateUpdate()
        {
            return Task.Run(() =>
            {
                var cameras = MyScene.GetComponents<Camera>();
                foreach (var camera in cameras)
                {
                    if (camera.RenderToWindow != null)
                        camera.RenderToWindow.Present();
                }
            });
        }

        public override Task Update()
        {
            return Task.Run(() =>
            {
                _renderCommand.Begin(VkCommandBufferUsageFlags.OneTimeSubmit);
                _renderCommand.BindPipeline(_pipeline, VkPipelineBindPoint.Compute);
                var cameras = MyScene.GetComponents<Camera>();
                foreach (var camera in cameras)
                {
                    _renderCommand.BindDescriptorSets(
                        _pipeline,
                        new API.DescriptorSetPool.DescriptorSet[]
                        { camera.RenderDescriptorSet },
                        VkPipelineBindPoint.Compute
                    );
                    _renderCommand.TransferImageLayout(camera.RenderedImage, VkImageLayout.Undefined, VkImageLayout.TransferDstOptimal);
                    _renderCommand.Dispatch(
                        Convert.ToUInt32(camera.RenderedImage.Width),
                        Convert.ToUInt32(camera.RenderedImage.Height),
                        1
                    );
                    if (camera.RenderToWindow != null)
                    {
                        var swapchian = camera.RenderToWindow.Swapchain;
                        var windowResolution = camera.RenderToWindow.Size;
                        _renderCommand.TransferImageLayout(camera.RenderedImage, VkImageLayout.TransferDstOptimal, VkImageLayout.TransferSrcOptimal);
                        _renderCommand.TransferImageLayout(camera.RenderToWindow.CurrentImage, swapchian.ImagesFormat, VkImageLayout.Undefined, VkImageLayout.TransferDstOptimal);
                        _renderCommand.BlitImage(
                            camera.RenderedImage.ImageHandle,
                            0, 0,
                            Convert.ToInt32(camera.Resolution.X),
                            Convert.ToInt32(camera.Resolution.Y),
                            0,
                            camera.RenderToWindow.CurrentImage,
                            0, 0,
                            Convert.ToInt32(windowResolution.X),
                            Convert.ToInt32(windowResolution.Y),
                            0
                        );
                        _renderCommand.TransferImageLayout(camera.RenderToWindow.CurrentImage, swapchian.ImagesFormat, VkImageLayout.TransferDstOptimal, VkImageLayout.PresentSrcKHR);
                    }
                }
                _renderCommand.End();
                _renderCommand.Submit(
                    API.Handler.MainDevice.GraphicsQueueFamily.Queues[0],
                    null,
                    null,
                    _renderFence
                );
                _renderFence.Wait();
            });
        }
    }
}