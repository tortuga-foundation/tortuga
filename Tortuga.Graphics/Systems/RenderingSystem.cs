using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tortuga.Graphics.API;

namespace Tortuga.Graphics
{
    /// <summary>
    /// Responsible for rendering objects into the scene
    /// </summary>
    public class RenderingSystem : Core.BaseSystem
    {
        private GraphicsModule _module;
        private API.Device _device;
        private CommandBuffer _renderCommand;
        private CommandBuffer _deferredCommand;
        private CommandBuffer _presentCommand;
        private Semaphore _transferCommandSemaphore;
        private Semaphore _renderCommandSemaphore;
        private Semaphore _deferredCommandSemaphore;
        private Semaphore _presentCommandSemaphore;
        private Dictionary<Window, Task<uint>> _swapchainIndexes;

        /// <summary>
        /// 
        /// </summary>
        public override void OnEnable()
        {
            _module = Engine.Instance.GetModule<GraphicsModule>();
            _device = _module.GraphicsService.PrimaryDevice;

            _renderCommand = _module.CommandBufferService.GetNewCommand(
                QueueFamilyType.Graphics,
                CommandType.Primary
            );
            _deferredCommand = _module.CommandBufferService.GetNewCommand(
                QueueFamilyType.Graphics,
                CommandType.Primary
            );
            _presentCommand = _module.CommandBufferService.GetNewCommand(
                QueueFamilyType.Graphics,
                CommandType.Primary
            );

            //sync helpers
            _transferCommandSemaphore = new Semaphore(_device);
            _renderCommandSemaphore = new Semaphore(_device);
            _deferredCommandSemaphore = new Semaphore(_device);
            _presentCommandSemaphore = new Semaphore(_device);
        }

        /// <summary>
        /// 
        /// </summary>
        public override Task EarlyUpdate()
        => Task.Run(() =>
        {
            _swapchainIndexes = new Dictionary<Window, Task<uint>>();
            var cameras = MyScene.GetComponents<Camera>();
            foreach (var camera in cameras)
            {
                var window = camera.RenderTarget as Window;
                if (window == null) continue;
                _swapchainIndexes.Add(
                    window,
                    window.AcquireSwapchainImage()
                );
            }
        });

        /// <summary>
        /// 
        /// </summary>
        public override Task Update()
        => Task.Run(() =>
        {
            var cameras = MyScene.GetComponents<Camera>();
            var meshRenderers = MyScene.GetComponents<MeshRenderer>();
            var transferCommands = new List<CommandBuffer>();

            #region transfer command

            foreach (var camera in cameras)
                camera.UpdateDescriptorSets();

            foreach (var meshRenderer in meshRenderers)
            {
                meshRenderer.UpdateDescriptorSet();
                transferCommands.Add(meshRenderer.Material.TransferImages());
            }
            #endregion

            #region render command

            _renderCommand.Begin(Vulkan.VkCommandBufferUsageFlags.OneTimeSubmit);
            foreach (var camera in cameras)
            {
                int attachmentCount = 0;
                foreach (var attachment in camera.MrtFramebuffer.RenderPass.Attachments)
                {
                    if (attachment.Format != API.RenderPassAttachment.Default.Format) continue;

                    _renderCommand.TransferImageLayout(
                        camera.MrtFramebuffer.Images[attachmentCount],
                        Vulkan.VkImageLayout.ColorAttachmentOptimal
                    );
                    attachmentCount++;
                }
                _renderCommand.BeginRenderPass(
                    camera.MrtFramebuffer,
                    Vulkan.VkSubpassContents.SecondaryCommandBuffers
                );


                var secondaryDrawCommands = new List<API.CommandBuffer>();
                foreach (var meshRenderer in meshRenderers)
                {
                    meshRenderer.UpdateDescriptorSet();
                    secondaryDrawCommands.Add(meshRenderer.DrawCommand(
                        camera.MrtFramebuffer,
                        0,
                        camera.ProjectionDescriptorSet,
                        camera.ViewDescriptorSet,
                        camera.Viewport
                    ));
                }
                if (secondaryDrawCommands.Count > 0)
                    _renderCommand.ExecuteCommands(secondaryDrawCommands);

                _renderCommand.EndRenderPass();
            }
            _renderCommand.End();

            #endregion

            #region deffered command

            _deferredCommand.Begin(Vulkan.VkCommandBufferUsageFlags.OneTimeSubmit);
            foreach (var camera in cameras)
            {
                int attachmentCount = 0;
                foreach (var attachment in camera.MrtFramebuffer.RenderPass.Attachments)
                {
                    if (attachment.Format != API.RenderPassAttachment.Default.Format) continue;

                    _deferredCommand.TransferImageLayout(
                        camera.MrtFramebuffer.Images[attachmentCount],
                        Vulkan.VkImageLayout.ShaderReadOnlyOptimal
                    );
                    attachmentCount++;
                }
                _deferredCommand.BeginRenderPass(
                    camera.DefferedFramebuffer,
                    Vulkan.VkSubpassContents.Inline
                );
                _deferredCommand.BindPipeline(Camera.DefferedPipeline);
                _deferredCommand.BindDescriptorSets(
                    Camera.DefferedPipeline,
                    camera.MrtDescriptorSets
                );
                _deferredCommand.SetScissor(
                    0, 0,
                    camera.DefferedFramebuffer.Width,
                    camera.DefferedFramebuffer.Height
                );
                _deferredCommand.SetViewport(
                    0, 0,
                    camera.DefferedFramebuffer.Width,
                    camera.DefferedFramebuffer.Height
                );
                _deferredCommand.Draw(6, 1);
                _deferredCommand.EndRenderPass();
            }
            _deferredCommand.End();

            #endregion

            #region update render targets

            _presentCommand.Begin(Vulkan.VkCommandBufferUsageFlags.OneTimeSubmit);

            Task.WaitAll(_swapchainIndexes.Values.ToArray());
            foreach (var camera in cameras)
            {
                if (camera.RenderTarget == null) continue;

                _presentCommand.TransferImageLayout(
                    camera.DefferedFramebuffer.Images[0],
                    Vulkan.VkImageLayout.TransferSrcOptimal
                );
                _presentCommand.TransferImageLayout(
                    camera.RenderTarget.RenderedImage,
                    Vulkan.VkImageLayout.TransferDstOptimal
                );
                // copy image from camera framebuffer to render target
                _presentCommand.BlitImage(
                    camera.DefferedFramebuffer.Images[0],
                    0, 0,
                    (int)camera.DefferedFramebuffer.Width,
                    (int)camera.DefferedFramebuffer.Height,
                    0,
                    camera.RenderTarget.RenderedImage,
                    0, 0,
                    (int)camera.RenderTarget.RenderedImage.Width,
                    (int)camera.RenderTarget.RenderedImage.Height,
                    0
                );
            }

            foreach (var swapchain in _swapchainIndexes)
            {
                swapchain.Value.Wait();
                var swapchainIndex = (int)swapchain.Value.Result;
                var window = swapchain.Key;
                _presentCommand.TransferImageLayout(
                    window.RenderedImage,
                    Vulkan.VkImageLayout.TransferSrcOptimal
                );
                _presentCommand.TransferImageLayout(
                    window.Swapchain.Images[swapchainIndex],
                    Vulkan.VkImageLayout.TransferDstOptimal
                );
                _presentCommand.BlitImage(
                    window.RenderedImage,
                    0, 0,
                    (int)window.RenderedImage.Width,
                    (int)window.RenderedImage.Height,
                    0,
                    window.Swapchain.Images[swapchainIndex],
                    0, 0,
                    (int)window.Swapchain.SurfaceExtent.width,
                    (int)window.Swapchain.SurfaceExtent.height,
                    0
                );
            }

            _presentCommand.End();

            #endregion

            #region submit Commands

            _module.CommandBufferService.Submit(
                transferCommands,
                new List<Semaphore> { _transferCommandSemaphore }
            );
            _module.CommandBufferService.Submit(
                _renderCommand,
                new List<Semaphore> { _renderCommandSemaphore },
                new List<Semaphore> { _transferCommandSemaphore }
            );
            _module.CommandBufferService.Submit(
                _deferredCommand,
                new List<Semaphore> { _deferredCommandSemaphore },
                new List<Semaphore> { _renderCommandSemaphore }
            );
            _module.CommandBufferService.Submit(
                _presentCommand,
                new List<Semaphore> { _presentCommandSemaphore },
                new List<Semaphore> { _deferredCommandSemaphore }
            );
            foreach (var swapchain in _swapchainIndexes)
            {
                var window = swapchain.Key;
                _module.CommandBufferService.Present(
                    window.Swapchain,
                    swapchain.Value.Result,
                    new List<Semaphore>{ _presentCommandSemaphore }
                );
            }

            #endregion
        });
    }
}